using Data.Context;
using Domain;
using Domain.DTO.Infrastructure.CQRS;
using Domain.DTO.Responses;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Services.Core;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Features.RagDocument.Queries.SearchBySemantic
{
    /// <summary>
    /// Handler for searching RAG documents by semantic similarity
    /// </summary>
    public class SearchBySemanticQueryHandler : BaseQueryHandler,
        IRequestHandler<SearchBySemanticQuery, Result<List<RagDocumentDto>>>
    {
        private readonly IAIService _aiService;
        private readonly IQdrantRagDocumentRepository _qdrantRepository;
        private readonly ILogger<SearchBySemanticQueryHandler> _logger;

        public SearchBySemanticQueryHandler(
            AppDbContext context,
            IUser user,
            IAIService aiService,
            IQdrantRagDocumentRepository qdrantRepository,
            ILogger<SearchBySemanticQueryHandler> logger)
            : base(context, user)
        {
            _aiService = aiService;
            _qdrantRepository = qdrantRepository;
            _logger = logger;
        }

        public async Task<Result<List<RagDocumentDto>>> Handle(
            SearchBySemanticQuery request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.UserQuery))
            {
                return Result<List<RagDocumentDto>>.Failure("Search query cannot be empty");
            }

            _logger.LogInformation("Semantic search for query: {Query} (limit: {Limit})",
                request.UserQuery, request.Limit);

            // 1. Generate query embedding
            var queryEmbedding = await _aiService.GenerateEmbeddingAsync(request.UserQuery);

            if (queryEmbedding == null || queryEmbedding.Length == 0)
            {
                return Result<List<RagDocumentDto>>.Failure("Failed to generate query embedding");
            }

            // 2. Determine user access level
            var accessLevel = DetermineUserAccessLevel();

            _logger.LogDebug("User access level: {AccessLevel}", accessLevel);

            // 3. Search Qdrant with access level filter
            var documents = await _qdrantRepository.SearchBySimilarityAsync(
                queryEmbedding,
                request.Limit,
                accessLevel);

            // 4. Map to DTOs
            var dtos = documents.Cast<Domain.RagDocument>()
                .Select(MapToDto)
                .ToList();

            _logger.LogInformation("Found {Count} documents for semantic search", dtos.Count);

            // 5. Return result
            return Result<List<RagDocumentDto>>.Success(dtos);
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
