using Data.Context;
using Domain.DTO.Infrastructure.CQRS;
using Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Services.Core;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Features.RagDocument.Commands.DeleteFileChunks
{
    /// <summary>
    /// Handles the deletion of all file chunks associated with a specified file in the repository.
    /// </summary>
    /// <remarks>This command handler processes <see cref="DeleteFileChunksCommand"/> requests by validating
    /// the command, retrieving all related file chunks, and deleting them from the underlying repository. The handler
    /// logs the deletion process and returns the number of successfully deleted chunks. If no chunks are found for the
    /// specified file, the handler returns zero.</remarks>
    public class DeleteFileChunksCommandHandler : BaseCommandHandler,
        IRequestHandler<DeleteFileChunksCommand, Result<int>>
    {
        private readonly IQdrantRagDocumentRepository _qdrantRepository;
        private readonly ILogger<DeleteFileChunksCommandHandler> _logger;
        private readonly IValidator<DeleteFileChunksCommand> _validator;

        public DeleteFileChunksCommandHandler(
            AppDbContext context,
            IUser user,
            IQdrantRagDocumentRepository qdrantRepository,
            ILogger<DeleteFileChunksCommandHandler> logger,
            IValidator<DeleteFileChunksCommand> validator)
            : base(context, user)
        {
            _qdrantRepository = qdrantRepository;
            _logger = logger;
            _validator = validator;
        }

        public async Task<Result<int>> Handle(
            DeleteFileChunksCommand request,
            CancellationToken cancellationToken)
        {
            var validationError = await ValidateAsync<DeleteFileChunksCommand, int>(_validator, request, cancellationToken);
            if (validationError != null)
                return validationError;

            _logger.LogInformation("Deleting all chunks for file {FileName}", request.FileName);

            // Get all chunks for this file
            var documents = await _qdrantRepository.GetBySourceAsync(request.FileName);
            var docList = documents.Cast<Domain.RagDocument>().ToList();

            if (!docList.Any())
            {
                _logger.LogInformation("No chunks found for file {FileName}", request.FileName);
                return Result<int>.Success(0);
            }

            // Delete each chunk
            var deletedCount = 0;
            foreach (var doc in docList)
            {
                var success = await _qdrantRepository.DeleteAsync(doc.Id);
                if (success)
                    deletedCount++;
                else
                    _logger.LogWarning("Failed to delete chunk {ChunkIndex} of file {FileName}", doc.ChunkIndex, request.FileName);
            }

            _logger.LogInformation("Deleted {DeletedCount} of {TotalCount} chunks for file {FileName}",
                deletedCount, docList.Count, request.FileName);

            return Result<int>.Success(deletedCount);
        }
    }
}