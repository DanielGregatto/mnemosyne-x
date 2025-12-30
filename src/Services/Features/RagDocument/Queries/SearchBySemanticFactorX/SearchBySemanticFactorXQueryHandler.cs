using Data.Context;
using Domain;
using Domain.DTO.Infrastructure.CQRS;
using Domain.DTO.Responses;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Services.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Features.RagDocument.Queries.SearchBySemanticFactorX
{
    /// <summary>
    /// Handler for searching RAG documents with Factor X contextual expansion
    /// Expands search results by including adjacent chunks based on similarity thresholds
    /// </summary>
    public class SearchBySemanticFactorXQueryHandler : BaseQueryHandler,
        IRequestHandler<SearchBySemanticFactorXQuery, Result<List<RagDocumentDto>>>
    {
        private readonly IAIService _aiService;
        private readonly IQdrantRagDocumentRepository _qdrantRepository;
        private readonly ILogger<SearchBySemanticFactorXQueryHandler> _logger;

        public SearchBySemanticFactorXQueryHandler(
            AppDbContext context,
            IUser user,
            IAIService aiService,
            IQdrantRagDocumentRepository qdrantRepository,
            ILogger<SearchBySemanticFactorXQueryHandler> logger)
            : base(context, user)
        {
            _aiService = aiService;
            _qdrantRepository = qdrantRepository;
            _logger = logger;
        }

        public async Task<Result<List<RagDocumentDto>>> Handle(
            SearchBySemanticFactorXQuery request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.UserQuery))
            {
                return Result<List<RagDocumentDto>>.Failure("Search query cannot be empty");
            }

            _logger.LogInformation("Factor X semantic search for query: {Query} (limit: {Limit}, factorX: {FactorX})",
                request.UserQuery, request.Limit, request.FactorX);

            // 1. Generate query embedding
            var queryEmbedding = await _aiService.GenerateEmbeddingAsync(request.UserQuery);

            if (queryEmbedding == null || queryEmbedding.Length == 0)
            {
                return Result<List<RagDocumentDto>>.Failure("Failed to generate query embedding");
            }

            // 2. Determine user access level
            var accessLevel = DetermineUserAccessLevel();

            // 3. Initial semantic search
            var initialDocuments = await _qdrantRepository.SearchBySimilarityAsync(
                queryEmbedding,
                request.Limit,
                accessLevel);

            var initialList = initialDocuments.Cast<Domain.RagDocument>().ToList();

            if (!initialList.Any())
            {
                _logger.LogInformation("No documents found for Factor X search");
                return Result<List<RagDocumentDto>>.Success(new List<RagDocumentDto>());
            }

            _logger.LogInformation("Initial search found {Count} documents, starting Factor X expansion",
                initialList.Count);

            // 4. Expand with Factor X algorithm
            var expandedDocuments = new Dictionary<Guid, Domain.RagDocument>();

            foreach (var doc in initialList)
            {
                // Get similarity score for this document
                var scoreResult = await _qdrantRepository.GetSimilarityScoreWithQueryAsync(doc.Id, queryEmbedding);

                if (scoreResult == null)
                {
                    _logger.LogWarning("Could not get similarity score for document {DocumentId}", doc.Id);
                    continue;
                }

                var (document, firstChunkScore) = scoreResult.Value;
                var currentDoc = (Domain.RagDocument)document;

                // Add the initial chunk
                if (!expandedDocuments.ContainsKey(currentDoc.Id))
                {
                    expandedDocuments[currentDoc.Id] = currentDoc;
                }

                var scoreThreshold = firstChunkScore - request.FactorX;

                _logger.LogDebug("Expanding document {DocumentId} (score: {Score}, threshold: {Threshold})",
                    currentDoc.Id, firstChunkScore, scoreThreshold);

                // 5. Expand NEXT chunks
                await ExpandNextChunks(currentDoc, queryEmbedding, scoreThreshold, request.MaxExpansionDepth, expandedDocuments);

                // 6. Expand PREVIOUS chunks
                await ExpandPreviousChunks(currentDoc, queryEmbedding, scoreThreshold, request.MaxExpansionDepth, expandedDocuments);
            }

            // 7. Deduplicate and sort by FileName + ChunkIndex
            var sortedDocuments = expandedDocuments.Values
                .OrderBy(d => d.FileName)
                .ThenBy(d => d.ChunkIndex)
                .ToList();

            _logger.LogInformation("Factor X expansion complete: {InitialCount} -> {ExpandedCount} documents",
                initialList.Count, sortedDocuments.Count);

            // 8. Map to DTOs
            var dtos = sortedDocuments.Select(MapToDto).ToList();

            return Result<List<RagDocumentDto>>.Success(dtos);
        }

        private async Task ExpandNextChunks(
            Domain.RagDocument startDoc,
            float[] queryEmbedding,
            float scoreThreshold,
            int maxDepth,
            Dictionary<Guid, Domain.RagDocument> expandedDocuments)
        {
            var currentDoc = startDoc;
            var depth = 0;

            while (depth < maxDepth && currentDoc.NextDocumentId.HasValue)
            {
                var nextDoc = await _qdrantRepository.GetByIdAsync(currentDoc.NextDocumentId.Value);

                if (nextDoc == null)
                    break;

                var nextRagDoc = (Domain.RagDocument)nextDoc;

                // Get similarity score for next chunk
                var scoreResult = await _qdrantRepository.GetSimilarityScoreWithQueryAsync(nextRagDoc.Id, queryEmbedding);

                if (scoreResult == null)
                    break;

                var (_, score) = scoreResult.Value;

                // Include if score meets threshold
                if (score >= scoreThreshold)
                {
                    if (!expandedDocuments.ContainsKey(nextRagDoc.Id))
                    {
                        expandedDocuments[nextRagDoc.Id] = nextRagDoc;
                        _logger.LogDebug("Included NEXT chunk {DocumentId} with score {Score}",
                            nextRagDoc.Id, score);
                    }

                    currentDoc = nextRagDoc;
                    depth++;
                }
                else
                {
                    _logger.LogDebug("Stopped NEXT expansion at chunk {DocumentId} (score {Score} < threshold {Threshold})",
                        nextRagDoc.Id, score, scoreThreshold);
                    break;
                }
            }
        }

        private async Task ExpandPreviousChunks(
            Domain.RagDocument startDoc,
            float[] queryEmbedding,
            float scoreThreshold,
            int maxDepth,
            Dictionary<Guid, Domain.RagDocument> expandedDocuments)
        {
            var currentDoc = startDoc;
            var depth = 0;

            while (depth < maxDepth && currentDoc.PreviousDocumentId.HasValue)
            {
                var prevDoc = await _qdrantRepository.GetByIdAsync(currentDoc.PreviousDocumentId.Value);

                if (prevDoc == null)
                    break;

                var prevRagDoc = (Domain.RagDocument)prevDoc;

                // Get similarity score for previous chunk
                var scoreResult = await _qdrantRepository.GetSimilarityScoreWithQueryAsync(prevRagDoc.Id, queryEmbedding);

                if (scoreResult == null)
                    break;

                var (_, score) = scoreResult.Value;

                // Include if score meets threshold
                if (score >= scoreThreshold)
                {
                    if (!expandedDocuments.ContainsKey(prevRagDoc.Id))
                    {
                        expandedDocuments[prevRagDoc.Id] = prevRagDoc;
                        _logger.LogDebug("Included PREVIOUS chunk {DocumentId} with score {Score}",
                            prevRagDoc.Id, score);
                    }

                    currentDoc = prevRagDoc;
                    depth++;
                }
                else
                {
                    _logger.LogDebug("Stopped PREVIOUS expansion at chunk {DocumentId} (score {Score} < threshold {Threshold})",
                        prevRagDoc.Id, score, scoreThreshold);
                    break;
                }
            }
        }

        private RagDocumentDto MapToDto(Domain.RagDocument doc)
        {
            return new RagDocumentDto
            {
                Id = doc.Id,
                FileName = doc.FileName,
                Content = doc.Content,
                Source = doc.Source,
                Category = doc.Category,
                Weight = doc.Weight,
                AccessLevel = doc.AccessLevel,
                ChunkIndex = doc.ChunkIndex,
                TotalChunks = doc.TotalChunks,
                FilePath = doc.FilePath,
                FileSize = doc.FileSize,
                Keywords = doc.Keywords,
                EmbeddingModel = doc.EmbeddingModel,
                Version = doc.Version,
                LastProcessed = doc.LastProcessed
            };
        }
    }
}
