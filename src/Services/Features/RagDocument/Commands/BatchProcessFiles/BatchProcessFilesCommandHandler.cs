using Data.Context;
using Domain.DTO.Infrastructure.CQRS;
using Domain.DTO.Responses;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Services.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Features.RagDocument.Commands.BatchProcessFiles
{
    /// <summary>
    /// Handler for batch processing multiple markdown files
    /// </summary>
    public class BatchProcessFilesCommandHandler : BaseCommandHandler,
        IRequestHandler<BatchProcessFilesCommand, Result<List<ProcessFileResultDto>>>
    {
        private readonly IAIService _aiService;
        private readonly IQdrantRagDocumentRepository _qdrantRepository;
        private readonly ILogger<BatchProcessFilesCommandHandler> _logger;
        private readonly IStringLocalizer<Domain.Resources.Messages> _localizer;

        public BatchProcessFilesCommandHandler(
            AppDbContext context,
            IStringLocalizer<Domain.Resources.Messages> localizer,
            IUser user,
            IAIService aiService,
            IQdrantRagDocumentRepository qdrantRepository,
            ILogger<BatchProcessFilesCommandHandler> logger)
            : base(context, user)
        {
            _localizer = localizer;
            _aiService = aiService;
            _qdrantRepository = qdrantRepository;
            _logger = logger;
        }

        public async Task<Result<List<ProcessFileResultDto>>> Handle(
            BatchProcessFilesCommand request,
            CancellationToken cancellationToken)
        {
            if (request.Files == null || request.Files.Count == 0)
            {
                return Result<List<ProcessFileResultDto>>.Failure(_localizer["RagDocument_NoFilesProvided"]);
            }

            _logger.LogInformation("Batch processing {FileCount} files", request.Files.Count);

            var results = new List<ProcessFileResultDto>();

            foreach (var file in request.Files)
            {
                try
                {
                    var result = await ProcessSingleFile(file, request, cancellationToken);
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing file {FileName} in batch", file.FileName);
                    results.Add(new ProcessFileResultDto
                    {
                        FileName = file.FileName,
                        TotalChunks = 0,
                        ProcessedChunks = 0,
                        Success = false,
                        Message = $"Error: {ex.Message}"
                    });
                }
            }

            var successCount = results.Count(r => r.Success);
            _logger.LogInformation("Batch processing complete: {SuccessCount}/{TotalCount} files successful",
                successCount, request.Files.Count);

            return Result<List<ProcessFileResultDto>>.Success(results);
        }

        private async Task<ProcessFileResultDto> ProcessSingleFile(
            Microsoft.AspNetCore.Http.IFormFile file,
            BatchProcessFilesCommand request,
            CancellationToken cancellationToken)
        {
            string? tempFilePath = null;

            try
            {
                // Save file to temp location
                tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_{file.FileName}");
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream, cancellationToken);
                }

                // Read content
                var content = await File.ReadAllTextAsync(tempFilePath, cancellationToken);

                if (string.IsNullOrWhiteSpace(content))
                {
                    return new ProcessFileResultDto
                    {
                        FileName = file.FileName,
                        TotalChunks = 0,
                        ProcessedChunks = 0,
                        Success = false,
                        Message = _localizer["RagDocument_FileEmpty"]
                    };
                }

                // Chunk the content
                var chunks = ChunkContent(content, request.ChunkSize, request.ChunkOverlap);

                if (chunks.Count == 0)
                {
                    return new ProcessFileResultDto
                    {
                        FileName = file.FileName,
                        TotalChunks = 0,
                        ProcessedChunks = 0,
                        Success = false,
                        Message = _localizer["RagDocument_ChunkingFailed"]
                    };
                }

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
                            i, file.FileName);
                        embedding = GenerateFallbackEmbedding(chunk);
                    }

                    // Create document
                    var document = new Domain.RagDocument
                    {
                        Id = Guid.NewGuid(),
                        FileName = file.FileName,
                        Content = chunk,
                        Source = request.Source ?? file.FileName,
                        Category = request.Category,
                        Weight = request.Weight,
                        AccessLevel = request.AccessLevel,
                        ChunkIndex = i,
                        TotalChunks = chunks.Count,
                        FilePath = file.FileName,
                        FileSize = file.Length,
                        Keywords = request.Keywords ?? string.Empty,
                        Embedding = embedding,
                        EmbeddingModel = "text-embedding-ada-002",
                        EmbeddingHash = ComputeHash(embedding),
                        ContentHash = ComputeHash(chunk),
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
                            document.ChunkIndex, file.FileName);
                    }
                }

                return new ProcessFileResultDto
                {
                    FileName = file.FileName,
                    TotalChunks = chunks.Count,
                    ProcessedChunks = processedChunks,
                    Success = processedChunks == chunks.Count,
                    Message = processedChunks == chunks.Count
                        ? _localizer["RagDocument_SChunks", processedChunks]
                        : _localizer["RagDocumen_SChunksWithErrors", processedChunks, chunks.Count]
                };
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

        private List<string> ChunkContent(string content, int chunkSize, int overlap)
        {
            var chunks = new List<string>();
            var position = 0;

            while (position < content.Length)
            {
                var remainingLength = content.Length - position;
                var currentChunkSize = Math.Min(chunkSize, remainingLength);

                var chunk = content.Substring(position, currentChunkSize);
                chunks.Add(chunk);

                position += chunkSize - overlap;

                if (position >= content.Length)
                    break;
            }

            return chunks;
        }

        private float[] GenerateFallbackEmbedding(string content)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(content));
            var embedding = new float[1536];

            for (int i = 0; i < embedding.Length; i++)
            {
                var byteIndex = i % hash.Length;
                embedding[i] = (hash[byteIndex] / 128f) - 1f;
            }

            return embedding;
        }

        private string ComputeHash(string content)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(content));
            return Convert.ToBase64String(hash);
        }

        private string ComputeHash(float[] embedding)
        {
            var bytes = new byte[embedding.Length * sizeof(float)];
            Buffer.BlockCopy(embedding, 0, bytes, 0, bytes.Length);
            var hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
