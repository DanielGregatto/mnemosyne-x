using Data.Context;
using Domain;
using Domain.DTO.Infrastructure.CQRS;
using Domain.DTO.Responses;
using Domain.Interfaces;
using FluentValidation;
using MediatR;
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

namespace Services.Features.RagDocument.Commands.ProcessMarkdownFile
{
    /// <summary>
    /// Handler for processing markdown files and storing them in the RAG system
    /// </summary>
    public class ProcessMarkdownFileCommandHandler : BaseCommandHandler,
        IRequestHandler<ProcessMarkdownFileCommand, Result<ProcessFileResultDto>>
    {
        private readonly IValidator<ProcessMarkdownFileCommand> _validator;
        private readonly IAIService _aiService;
        private readonly IQdrantRagDocumentRepository _qdrantRepository;
        private readonly ILogger<ProcessMarkdownFileCommandHandler> _logger;

        public ProcessMarkdownFileCommandHandler(
            AppDbContext context,
            IUser user,
            IValidator<ProcessMarkdownFileCommand> validator,
            IAIService aiService,
            IQdrantRagDocumentRepository qdrantRepository,
            ILogger<ProcessMarkdownFileCommandHandler> logger)
            : base(context, user)
        {
            _validator = validator;
            _aiService = aiService;
            _qdrantRepository = qdrantRepository;
            _logger = logger;
        }

        public async Task<Result<ProcessFileResultDto>> Handle(
            ProcessMarkdownFileCommand request,
            CancellationToken cancellationToken)
        {
            // 1. Validate
            var validationError = await ValidateAsync<ProcessMarkdownFileCommand, ProcessFileResultDto>(
                _validator, request, cancellationToken);
            if (validationError != null)
                return validationError;

            string? tempFilePath = null;
            try
            {
                // 2. Save file to temp location
                tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_{request.File.FileName}");
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await request.File.CopyToAsync(stream, cancellationToken);
                }

                // 3. Read content
                var content = await File.ReadAllTextAsync(tempFilePath, cancellationToken);

                if (string.IsNullOrWhiteSpace(content))
                {
                    return Result<ProcessFileResultDto>.Failure("File content is empty");
                }

                _logger.LogInformation("Processing file {FileName} with {ContentLength} characters",
                    request.File.FileName, content.Length);

                // 4. Chunk the content
                var chunks = ChunkContent(content, request.ChunkSize, request.ChunkOverlap);

                if (chunks.Count == 0)
                {
                    return Result<ProcessFileResultDto>.Failure("Failed to chunk content");
                }

                _logger.LogInformation("Created {ChunkCount} chunks from {FileName}",
                    chunks.Count, request.File.FileName);

                // 5. Process each chunk
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
                        embedding = GenerateFallbackEmbedding(chunk);
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
                        EmbeddingHash = ComputeHash(embedding),
                        ContentHash = ComputeHash(chunk),
                        Version = 1,
                        LastProcessed = DateTime.UtcNow,
                        CustomMetadata = string.Empty
                    };

                    documents.Add(document);
                }

                // 6. Link chunks with Previous/Next IDs
                for (int i = 0; i < documents.Count; i++)
                {
                    if (i > 0)
                        documents[i].PreviousDocumentId = documents[i - 1].Id;

                    if (i < documents.Count - 1)
                        documents[i].NextDocumentId = documents[i + 1].Id;
                }

                // 7. Upsert to Qdrant
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

                // 8. Return result
                var result = new ProcessFileResultDto
                {
                    FileName = request.File.FileName,
                    TotalChunks = chunks.Count,
                    ProcessedChunks = processedChunks,
                    Success = processedChunks == chunks.Count,
                    Message = processedChunks == chunks.Count
                        ? $"Successfully processed {processedChunks} chunks"
                        : $"Processed {processedChunks} of {chunks.Count} chunks with errors"
                };

                return Result<ProcessFileResultDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file {FileName}", request.File.FileName);
                return Result<ProcessFileResultDto>.Failure($"Failed to process file: {ex.Message}");
            }
            finally
            {
                // 9. Clean up temp file
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

        /// <summary>
        /// Chunks content into overlapping segments
        /// </summary>
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

                // Move position forward by (chunkSize - overlap)
                position += chunkSize - overlap;

                // If we're near the end and would create a tiny chunk, stop
                if (position >= content.Length)
                    break;
            }

            return chunks;
        }

        /// <summary>
        /// Generates a deterministic fallback embedding based on content hash
        /// Used when OpenAI API is unavailable
        /// </summary>
        private float[] GenerateFallbackEmbedding(string content)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(content));
            var embedding = new float[1536]; // Match OpenAI ada-002 dimensions

            // Convert hash bytes to float values in range [-1, 1]
            for (int i = 0; i < embedding.Length; i++)
            {
                var byteIndex = i % hash.Length;
                embedding[i] = (hash[byteIndex] / 128f) - 1f;
            }

            return embedding;
        }

        /// <summary>
        /// Computes SHA256 hash for content or embedding
        /// </summary>
        private string ComputeHash(string content)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(content));
            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Computes SHA256 hash for embedding array
        /// </summary>
        private string ComputeHash(float[] embedding)
        {
            var bytes = new byte[embedding.Length * sizeof(float)];
            Buffer.BlockCopy(embedding, 0, bytes, 0, bytes.Length);
            var hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
