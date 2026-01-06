using Data.Context;
using Domain.DTO.Infrastructure.CQRS;
using Domain.DTO.Responses;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Services.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Features.RagDocument.Queries.GetDocumentById
{
    /// <summary>
    /// Handles queries to retrieve a RAG document by its unique identifier, validating access permissions and returning
    /// the result.
    /// </summary>
    /// <remarks>This handler checks whether the requested document exists and whether the current user has
    /// sufficient access rights before returning the document data. If the document is not found or the user lacks
    /// permission, an appropriate failure result is returned. Logging and localized error messages are provided for not
    /// found and unauthorized access scenarios.</remarks>
    public class GetDocumentByIdQueryHandler : BaseQueryHandler,
        IRequestHandler<GetDocumentByIdQuery, Result<RagDocumentDto>>
    {
        private readonly IQdrantRagDocumentRepository _qdrantRepository;
        private readonly ILogger<GetDocumentByIdQueryHandler> _logger;
        private IStringLocalizer<Domain.Resources.Messages> _localizer;

        public GetDocumentByIdQueryHandler(
            AppDbContext context,
            IUser user,
            IQdrantRagDocumentRepository qdrantRepository,
            ILogger<GetDocumentByIdQueryHandler> logger,
            IStringLocalizer<Domain.Resources.Messages> localizer)
            : base(context, user)
        {
            _qdrantRepository = qdrantRepository;
            _logger = logger;
            _localizer = localizer;
        }

        public async Task<Result<RagDocumentDto>> Handle(
            GetDocumentByIdQuery request,
            CancellationToken cancellationToken)
        {
            if (request.DocumentId == Guid.Empty)
                return Result<RagDocumentDto>.Failure(_localizer["InvalidEmpty", nameof(GetDocumentByIdQuery.DocumentId)]);

            var document = await _qdrantRepository.GetByIdAsync(request.DocumentId);

            if (document == null)
            {
                _logger.LogWarning("Document {DocumentId} not found", request.DocumentId);
                return Result<RagDocumentDto>.NotFound(_localizer["RagDocument_NotFound", request.DocumentId]);
            }

            var ragDoc = (Domain.RagDocument)document;

            var userAccessLevel = DetermineUserAccessLevel();
            if (ragDoc.AccessLevel > userAccessLevel)
            {
                _logger.LogWarning("User denied access to document {DocumentId} (requires level {RequiredLevel}, user has {UserLevel})",
                    request.DocumentId, ragDoc.AccessLevel, userAccessLevel);

                return Result<RagDocumentDto>.Unauthorized(_localizer["RagDocument_NoPermissionToAccess"]);
            }

            return Result<RagDocumentDto>.Success(ragDoc.MapToDto());
        }
    }
}