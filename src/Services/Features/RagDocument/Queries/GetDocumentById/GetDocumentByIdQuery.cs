using Domain.DTO.Infrastructure.CQRS;
using Domain.DTO.Responses;
using MediatR;
using System;

namespace Services.Features.RagDocument.Queries.GetDocumentById
{
    /// <summary>
    /// Represents a request to retrieve a document by its unique identifier.
    /// </summary>
    /// <remarks>This query is typically used with a mediator pattern to fetch a <see cref="RagDocumentDto"/>
    /// corresponding to the specified <see cref="DocumentId"/>.</remarks>
    public class GetDocumentByIdQuery : IRequest<Result<RagDocumentDto>>
    {
        public Guid DocumentId { get; set; }
    }
}
