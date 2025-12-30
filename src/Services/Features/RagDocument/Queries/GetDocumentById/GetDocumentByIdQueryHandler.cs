using Data.Context;
using Domain;
using Domain.DTO.Infrastructure.CQRS;
using Domain.DTO.Responses;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Services.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Features.RagDocument.Queries.GetDocumentById
{
    /// <summary>
    /// Handler for retrieving a RAG document by ID
    /// </summary>
    public class GetDocumentByIdQueryHandler : BaseQueryHandler,
        IRequestHandler<GetDocumentByIdQuery, Result<RagDocumentDto>>
    {
        private readonly IQdrantRagDocumentRepository _qdrantRepository;
        private readonly ILogger<GetDocumentByIdQueryHandler> _logger;

        public GetDocumentByIdQueryHandler(
            AppDbContext context,
            IUser user,
            IQdrantRagDocumentRepository qdrantRepository,
            ILogger<GetDocumentByIdQueryHandler> logger)
            : base(context, user)
        {
            _qdrantRepository = qdrantRepository;
            _logger = logger;
        }

        public async Task<Result<RagDocumentDto>> Handle(
            GetDocumentByIdQuery request,
            CancellationToken cancellationToken)
        {
            if (request.DocumentId == Guid.Empty)
            {
                return Result<RagDocumentDto>.Failure("Invalid document ID");
            }

            var document = await _qdrantRepository.GetByIdAsync(request.DocumentId);

            if (document == null)
            {
                _logger.LogWarning("Document {DocumentId} not found", request.DocumentId);
                return Result<RagDocumentDto>.NotFound($"Document {request.DocumentId} not found");
            }

            var ragDoc = (Domain.RagDocument)document;

            // Check access level
            var userAccessLevel = DetermineUserAccessLevel();
            if (ragDoc.AccessLevel > userAccessLevel)
            {
                _logger.LogWarning("User denied access to document {DocumentId} (requires level {RequiredLevel}, user has {UserLevel})",
                    request.DocumentId, ragDoc.AccessLevel, userAccessLevel);
                return Result<RagDocumentDto>.Unauthorized("You do not have permission to access this document");
            }

            var dto = MapToDto(ragDoc);

            return Result<RagDocumentDto>.Success(dto);
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
