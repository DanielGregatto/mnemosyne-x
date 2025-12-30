using Domain.DTO.Infrastructure.CQRS;
using MediatR;
using System;

namespace Services.Features.RagDocument.Commands.DeleteDocument
{
    /// <summary>
    /// Command to delete a RAG document by ID
    /// </summary>
    public class DeleteDocumentCommand : IRequest<Result<bool>>
    {
        /// <summary>
        /// The ID of the document to delete
        /// </summary>
        public Guid DocumentId { get; set; }
    }
}
