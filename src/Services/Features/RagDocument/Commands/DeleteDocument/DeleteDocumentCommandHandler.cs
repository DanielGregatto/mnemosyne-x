using Data.Context;
using Domain.DTO.Infrastructure.CQRS;
using Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Services.Core;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Features.RagDocument.Commands.DeleteDocument
{
    /// <summary>
    /// Handler for deleting a RAG document
    /// </summary>
    public class DeleteDocumentCommandHandler : BaseCommandHandler,
        IRequestHandler<DeleteDocumentCommand, Result<bool>>
    {
        private readonly IValidator<DeleteDocumentCommand> _validator;
        private readonly IQdrantRagDocumentRepository _qdrantRepository;
        private readonly ILogger<DeleteDocumentCommandHandler> _logger;
        private readonly IStringLocalizer<Domain.Resources.Messages> _localizer;

        public DeleteDocumentCommandHandler(
            AppDbContext context,
            IUser user,
            IValidator<DeleteDocumentCommand> validator,
            IQdrantRagDocumentRepository qdrantRepository,
            ILogger<DeleteDocumentCommandHandler> logger,
            IStringLocalizer<Domain.Resources.Messages> localizer)
            : base(context, user)
        {
            _validator = validator;
            _qdrantRepository = qdrantRepository;
            _logger = logger;
            _localizer = localizer;
        }

        public async Task<Result<bool>> Handle(
            DeleteDocumentCommand request,
            CancellationToken cancellationToken)
        {
            // 1. Validate
            var validationError = await ValidateAsync<DeleteDocumentCommand, bool>(
                _validator, request, cancellationToken);
            if (validationError != null)
                return validationError;

            // 2. Delete from Qdrant
            var success = await _qdrantRepository.DeleteAsync(request.DocumentId);

            if (!success)
            {
                _logger.LogWarning("Failed to delete document {DocumentId}", request.DocumentId);
                return Result<bool>.Failure(_localizer["RagDocument_DeleteFailed", request.DocumentId]);
            }

            _logger.LogInformation("Successfully deleted document {DocumentId}", request.DocumentId);

            // 3. Return result
            return Result<bool>.Success(true);
        }
    }
}
