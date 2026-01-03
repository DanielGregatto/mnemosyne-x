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
using System.Threading;
using System.Threading.Tasks;

namespace Services.Features.RagDocument.Commands.ProcessMarkdownFile
{
    /// <summary>
    /// Handles the processing of markdown file commands, including validation, content chunking, embedding generation,
    /// and document storage.
    /// </summary>
    /// <remarks>This command handler coordinates the end-to-end workflow for ingesting markdown files into
    /// the system. It validates the incoming command, splits the file content into overlapping chunks, generates
    /// embeddings for each chunk (using an AI service or a deterministic fallback), and stores the resulting documents
    /// in the Qdrant vector database. Logging and localization are integrated throughout the process. The handler
    /// returns a result indicating the number of successfully processed chunks and any errors encountered.</remarks>
    public class ProcessMarkdownFileCommandHandler : BaseCommandHandler,
        IRequestHandler<ProcessMarkdownFileCommand, Result<ProcessFileResultDto>>
    {
        private readonly IValidator<ProcessMarkdownFileCommand> _validator;
        private readonly IAIService _aiService;
        private readonly IQdrantRagDocumentRepository _qdrantRepository;
        private readonly ILogger<ProcessMarkdownFileCommandHandler> _logger;
        private readonly IStringLocalizer<Domain.Resources.Messages> _localizer;
        private readonly IRagIngestionUtilities _ragIngestionUtilities;

        public ProcessMarkdownFileCommandHandler(
            AppDbContext context,
            IUser user,
            IValidator<ProcessMarkdownFileCommand> validator,
            IAIService aiService,
            IQdrantRagDocumentRepository qdrantRepository,
            ILogger<ProcessMarkdownFileCommandHandler> logger,
            IStringLocalizer<Domain.Resources.Messages> loalizer,
            IRagIngestionUtilities ragIngestionUtilities)
            : base(context, user)
        {
            _validator = validator;
            _aiService = aiService;
            _qdrantRepository = qdrantRepository;
            _logger = logger;
            _localizer = loalizer;
            _ragIngestionUtilities = ragIngestionUtilities;
        }

        public async Task<Result<ProcessFileResultDto>> Handle(
            ProcessMarkdownFileCommand request,
            CancellationToken cancellationToken)
        {
            var validationError = await ValidateAsync<ProcessMarkdownFileCommand, ProcessFileResultDto>(_validator, request, cancellationToken);
            if (validationError != null)
                return validationError;

            string? tempFilePath = null;
            try
            {
                // Save file to temp location
                tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_{request.File.FileName}");
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await request.File.CopyToAsync(stream, cancellationToken);
                }

                // Read content
                var content = await File.ReadAllTextAsync(tempFilePath, cancellationToken);

                if (string.IsNullOrWhiteSpace(content))
                    return Result<ProcessFileResultDto>.Failure(_localizer["RagMarkdownFile_EmptyContent"]);

                _logger.LogInformation("Processing file {FileName} with {ContentLength} characters",
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

                    // Create document
                    var document = new Domain.RagDocument
                    {
                        Id = Guid.NewGuid(),
                        FileName = request.File.FileName,
                        Content = chunk,
                        Source = request.Source ?? request.File.FileName,
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
                        Version = 1,
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

                _logger.LogInformation("Successfully processed {ProcessedChunks}/{TotalChunks} chunks for {FileName}",
                    processedChunks, chunks.Count, request.File.FileName);

                // Return result
                var result = new ProcessFileResultDto
                {
                    FileName = request.File.FileName,
                    TotalChunks = chunks.Count,
                    ProcessedChunks = processedChunks,
                    Success = processedChunks == chunks.Count,
                    Message = processedChunks == chunks.Count
                        ? _localizer["RagMarkdownFile_Success", processedChunks]
                        : _localizer["RagMarkdownFile_SuccessWithErrors", processedChunks, chunks.Count]
                };

                return Result<ProcessFileResultDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file {FileName}", request.File.FileName);
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