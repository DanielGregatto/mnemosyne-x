using Data.Context;
using Domain.DTO.Infrastructure.CQRS;
using Domain.DTO.Responses;
using Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Services.Core;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Features.RagDocument.Commands.UpdateMarkdownFile
{
    /// <summary>
    /// Handler for updating an existing markdown file
    /// </summary>
    public class UpdateMarkdownFileCommandHandler : BaseCommandHandler,
        IRequestHandler<UpdateMarkdownFileCommand, Result<ProcessFileResultDto>>
    {
        private readonly IValidator<UpdateMarkdownFileCommand> _validator;
        private readonly IAIService _aiService;
        private readonly IQdrantRagDocumentRepository _qdrantRepository;
        private readonly ILogger<UpdateMarkdownFileCommandHandler> _logger;
        private readonly IRagIngestionUtilities _ragIngestionUtilities;
        private readonly IStringLocalizer<Domain.Resources.Messages> _localizer;

        public UpdateMarkdownFileCommandHandler(
            AppDbContext context,
            IUser user,
            IValidator<UpdateMarkdownFileCommand> validator,
            IAIService aiService,
            IQdrantRagDocumentRepository qdrantRepository,
            ILogger<UpdateMarkdownFileCommandHandler> logger,
            IRagIngestionUtilities ragIngestionUtilities,
            IStringLocalizer<Domain.Resources.Messages> localizer)
            : base(context, user)
        {
            _validator = validator;
            _aiService = aiService;
            _qdrantRepository = qdrantRepository;
            _logger = logger;
            _ragIngestionUtilities = ragIngestionUtilities;
            _localizer = localizer;
        }

        public async Task<Result<ProcessFileResultDto>> Handle(
            UpdateMarkdownFileCommand request,
            CancellationToken cancellationToken)
        {
            // Validate
            var validationError = await ValidateAsync<UpdateMarkdownFileCommand, ProcessFileResultDto>(_validator, request, cancellationToken);
            if (validationError != null)
                return validationError;

            string? tempFilePath = null;
            try
            {
                // Delete existing file chunks
                _logger.LogInformation("Deleting existing chunks for file {ExistingFileName}", request.ExistingFileName);

                var existingDocuments = await _qdrantRepository.GetByFileNameAsync(request.ExistingFileName);
                var existingList = existingDocuments.Cast<Domain.RagDocument>().ToList();

                var currentVersion = 1;
                if (existingList.Any())
                {
                    currentVersion = existingList.Max(d => d.Version) + 1;

                    foreach (var doc in existingList)
                    {
                        await _qdrantRepository.DeleteAsync(doc.Id);
                    }

                    _logger.LogInformation("Deleted {Count} existing chunks for file {ExistingFileName}",
                        existingList.Count, request.ExistingFileName);
                }

                // Save new file to temp location
                tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_{request.File.FileName}");
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await request.File.CopyToAsync(stream, cancellationToken);
                }

                // Read content
                var content = await File.ReadAllTextAsync(tempFilePath, cancellationToken);

                if (string.IsNullOrWhiteSpace(content))
                    return Result<ProcessFileResultDto>.Failure(_localizer["RagMarkdownFile_EmptyContent"]);

                _logger.LogInformation("Processing updated file {FileName} with {ContentLength} characters",
                    request.File.FileName, content.Length);

                // Chunk the content
                var chunks = _ragIngestionUtilities.ChunkContent(content, request.ChunkSize, request.ChunkOverlap);

                if (chunks.Count == 0)
                    return Result<ProcessFileResultDto>.Failure(_localizer["RagMarkdownFile_FailedToChunk"]);

                _logger.LogInformation("Created {ChunkCount} chunks from {FileName}",
                    chunks.Count, request.File.FileName);

                // Process each chunk
                var documents = new List<Domain.RagDocument>();
                var processedChunks = 0;

                for (int i = 0; i < chunks.Count; i++)
                {
                    var chunk = chunks[i];

                    // Generate embedding
                    var embedding = await _aiService.GenerateEmbeddingAsync(chunk);

                    if (embedding == null || embedding.Length == 0)
                    {
                        _logger.LogWarning("Failed to generate embedding for chunk {ChunkIndex} of {FileName}, using fallback",
                            i, request.File.FileName);

                        embedding = _ragIngestionUtilities.GenerateFallbackEmbedding(chunk);
                    }

                    // Create document with incremented version
                    var document = new Domain.RagDocument
                    {
                        Id = Guid.NewGuid(),
                        FileName = request.File.FileName,
                        Content = chunk,
                        Source = request.Source ?? request.ExistingFileName,
                        Category = request.Category,
                        Weight = request.Weight,
                        AccessLevel = request.AccessLevel,
                        ChunkIndex = i,
                        TotalChunks = chunks.Count,
                        FilePath = request.File.FileName,
                        FileSize = request.File.Length,
                        Keywords = request.Keywords ?? string.Empty,
                        Embedding = embedding,
                        EmbeddingModel = "text-embedding-ada-002",
                        EmbeddingHash = _ragIngestionUtilities.ComputeHash(embedding),
                        ContentHash = _ragIngestionUtilities.ComputeHash(chunk),
                        Version = currentVersion,
                        LastProcessed = DateTime.UtcNow,
                        CustomMetadata = string.Empty
                    };

                    documents.Add(document);
                }

                // Link chunks with Previous/Next IDs
                for (int i = 0; i < documents.Count; i++)
                {
                    if (i > 0)
                        documents[i].PreviousDocumentId = documents[i - 1].Id;

                    if (i < documents.Count - 1)
                        documents[i].NextDocumentId = documents[i + 1].Id;
                }

                // Upsert to Qdrant
                foreach (var document in documents)
                {
                    try
                    {
                        await _qdrantRepository.UpsertAsync(document);
                        processedChunks++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to upsert chunk {ChunkIndex} of {FileName}",
                            document.ChunkIndex, request.File.FileName);
                    }
                }

                _logger.LogInformation("Successfully updated file {FileName} - version {Version} with {ProcessedChunks}/{TotalChunks} chunks",
                    request.File.FileName, currentVersion, processedChunks, chunks.Count);

                // Return result
                var result = new ProcessFileResultDto
                {
                    FileName = request.File.FileName,
                    TotalChunks = chunks.Count,
                    ProcessedChunks = processedChunks,
                    Success = processedChunks == chunks.Count,
                    Message = processedChunks == chunks.Count
                        ? _localizer["RagMarkdownFile_UpdateSuccess", currentVersion, processedChunks]
                        : _localizer["RagMarkdownFile_UpdateWithErrors", processedChunks, chunks.Count]
                };

                return Result<ProcessFileResultDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating file {FileName}", request.File.FileName);
                return Result<ProcessFileResultDto>.Failure(_localizer["RagMarkdownFile_Failed", ex.Message]);
            }
            finally
            {
                // Clean up temp file
                if (tempFilePath != null && File.Exists(tempFilePath))
                {
                    try
                    {
                        File.Delete(tempFilePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete temp file {TempFilePath}", tempFilePath);
                    }
                }
            }
        }
    }
}