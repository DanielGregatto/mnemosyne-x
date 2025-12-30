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

namespace Services.Features.RagDocument.Queries.GetFileChunks
{
    /// <summary>
    /// Handler for retrieving all chunks of a file
    /// </summary>
    public class GetFileChunksQueryHandler : BaseQueryHandler,
        IRequestHandler<GetFileChunksQuery, Result<List<RagDocumentDto>>>
    {
        private readonly IQdrantRagDocumentRepository _qdrantRepository;
        private readonly ILogger<GetFileChunksQueryHandler> _logger;

        public GetFileChunksQueryHandler(
            AppDbContext context,
            IUser user,
            IQdrantRagDocumentRepository qdrantRepository,
            ILogger<GetFileChunksQueryHandler> logger)
            : base(context, user)
        {
            _qdrantRepository = qdrantRepository;
            _logger = logger;
        }

        public async Task<Result<List<RagDocumentDto>>> Handle(
            GetFileChunksQuery request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.FileName))
            {
                return Result<List<RagDocumentDto>>.Failure("FileName cannot be empty");
            }

            _logger.LogInformation("Retrieving all chunks for file: {FileName}", request.FileName);

            // Get all chunks for this file
            var documents = await _qdrantRepository.GetBySourceAsync(request.FileName);
            var docList = documents.Cast<Domain.RagDocument>().ToList();

            if (!docList.Any())
            {
                _logger.LogInformation("No chunks found for file {FileName}", request.FileName);
                return Result<List<RagDocumentDto>>.Success(new List<RagDocumentDto>());
            }

            // Determine user access level
            var userAccessLevel = DetermineUserAccessLevel();

            // Filter by access level and sort by chunk index
            var filteredDocs = docList
                .Where(d => d.AccessLevel <= userAccessLevel)
                .OrderBy(d => d.ChunkIndex)
                .ToList();

            _logger.LogInformation("Found {TotalCount} chunks for file {FileName}, {FilteredCount} accessible to user",
                docList.Count, request.FileName, filteredDocs.Count);

            var dtos = filteredDocs.Select(MapToDto).ToList();

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
