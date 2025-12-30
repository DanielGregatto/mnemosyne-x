using Domain.DTO.Infrastructure.CQRS;
using MediatR;

namespace Services.Features.RagDocument.Commands.DeleteFileChunks
{
    /// <summary>
    /// Command to delete all chunks of a file by filename
    /// </summary>
    public class DeleteFileChunksCommand : IRequest<Result<int>>
    {
        /// <summary>
        /// The filename of the document chunks to delete
        /// </summary>
        public string FileName { get; set; }
    }
}
