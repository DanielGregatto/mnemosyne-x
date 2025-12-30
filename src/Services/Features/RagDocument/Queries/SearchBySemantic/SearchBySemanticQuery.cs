using Domain.DTO.Infrastructure.CQRS;
using Domain.DTO.Responses;
using MediatR;
using System.Collections.Generic;

namespace Services.Features.RagDocument.Queries.SearchBySemantic
{
    /// <summary>
    /// Query to search RAG documents by semantic similarity
    /// </summary>
    public class SearchBySemanticQuery : IRequest<Result<List<RagDocumentDto>>>
    {
        /// <summary>
        /// The user's search query
        /// </summary>
        public string UserQuery { get; set; }

        /// <summary>
        /// Maximum number of results to return
        /// </summary>
        public int Limit { get; set; } = 10;
    }
}
