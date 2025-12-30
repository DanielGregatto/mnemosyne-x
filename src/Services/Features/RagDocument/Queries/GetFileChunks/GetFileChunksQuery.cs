using Domain.DTO.Infrastructure.CQRS;
using Domain.DTO.Responses;
using MediatR;
using System.Collections.Generic;

namespace Services.Features.RagDocument.Queries.GetFileChunks
{
    /// <summary>
    /// Query to get all chunks of a file by filename
    /// </summary>
    public class GetFileChunksQuery : IRequest<Result<List<RagDocumentDto>>>
    {
        /// <summary>
        /// The filename of the chunks to retrieve
        /// </summary>
        public string FileName { get; set; }
    }
}
