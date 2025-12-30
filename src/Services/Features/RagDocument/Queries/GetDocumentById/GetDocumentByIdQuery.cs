using Domain.DTO.Infrastructure.CQRS;
using Domain.DTO.Responses;
using MediatR;
using System;

namespace Services.Features.RagDocument.Queries.GetDocumentById
{
    /// <summary>
    /// Query to get a specific RAG document by ID
    /// </summary>
    public class GetDocumentByIdQuery : IRequest<Result<RagDocumentDto>>
    {
        /// <summary>
        /// The ID of the document to retrieve
        /// </summary>
        public Guid DocumentId { get; set; }
    }
}
