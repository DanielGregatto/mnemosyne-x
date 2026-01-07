using Domain.DTO.Infrastructure.CQRS;
using Domain.DTO.Responses;
using MediatR;
using System.Collections.Generic;

namespace Services.Features.RagDocument.Queries.GetFileChunks
{
    /// <summary>
    /// Represents a query to retrieve the list of document chunks associated with a specified file.
    /// </summary>
    /// <remarks>This query is typically used to obtain segmented content from a file for further processing
    /// or analysis.</remarks>
    public class GetFileChunksQuery : IRequest<Result<List<RagDocumentDto>>>
    {
        public string FileName { get; set; }
    }
}
