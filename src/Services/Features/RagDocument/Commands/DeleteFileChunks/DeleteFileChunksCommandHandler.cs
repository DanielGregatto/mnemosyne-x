using Data.Context;
using Domain;
using Domain.DTO.Infrastructure.CQRS;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Services.Core;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Features.RagDocument.Commands.DeleteFileChunks
{
    /// <summary>
    /// Handler for deleting all chunks of a file
    /// </summary>
    public class DeleteFileChunksCommandHandler : BaseCommandHandler,
        IRequestHandler<DeleteFileChunksCommand, Result<int>>
    {
        private readonly IQdrantRagDocumentRepository _qdrantRepository;
        private readonly ILogger<DeleteFileChunksCommandHandler> _logger;

        public DeleteFileChunksCommandHandler(
            AppDbContext context,
            IUser user,
            IQdrantRagDocumentRepository qdrantRepository,
            ILogger<DeleteFileChunksCommandHandler> logger)
            : base(context, user)
        {
            _qdrantRepository = qdrantRepository;
            _logger = logger;
        }

        public async Task<Result<int>> Handle(
            DeleteFileChunksCommand request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.FileName))
            {
                return Result<int>.Failure("FileName cannot be empty");
            }

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
                {
                    deletedCount++;
                }
                else
                {
                    _logger.LogWarning("Failed to delete chunk {ChunkIndex} of file {FileName}",
                        doc.ChunkIndex, request.FileName);
                }
            }

            _logger.LogInformation("Deleted {DeletedCount} of {TotalCount} chunks for file {FileName}",
                deletedCount, docList.Count, request.FileName);

            return Result<int>.Success(deletedCount);
        }
    }
}
