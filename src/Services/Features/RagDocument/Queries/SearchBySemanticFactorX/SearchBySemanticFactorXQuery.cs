using Domain.DTO.Infrastructure.CQRS;
using Domain.DTO.Responses;
using MediatR;
using System.Collections.Generic;

namespace Services.Features.RagDocument.Queries.SearchBySemanticFactorX
{
    /// <summary>
    /// Query to search RAG documents by semantic similarity with Factor X contextual expansion
    /// Factor X expands results by including adjacent chunks based on similarity thresholds
    /// </summary>
    public class SearchBySemanticFactorXQuery : IRequest<Result<List<RagDocumentDto>>>
    {
        /// <summary>
        /// The user's search query
        /// </summary>
        public string UserQuery { get; set; }

        /// <summary>
        /// Maximum number of initial results to return (before expansion)
        /// </summary>
        public int Limit { get; set; } = 10;

        /// <summary>
        /// Factor X similarity threshold drop (default 0.05)
        /// Lower = stricter context inclusion, Higher = more surrounding chunks
        /// Adjacent chunks are included if their score >= (firstChunkScore - FactorX)
        /// </summary>
        public float FactorX { get; set; } = 0.05f;

        /// <summary>
        /// Maximum number of chunks to expand in each direction (next/previous)
        /// </summary>
        public int MaxExpansionDepth { get; set; } = 5;
    }
}
