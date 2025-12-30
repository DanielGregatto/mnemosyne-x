using Domain.DTO.Infrastructure.CQRS;
using Domain.DTO.Responses;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Services.Features.RagDocument.Commands.ProcessMarkdownFile
{
    /// <summary>
    /// Command to process a markdown file and store it in the RAG system
    /// </summary>
    public class ProcessMarkdownFileCommand : IRequest<Result<ProcessFileResultDto>>
    {
        /// <summary>
        /// The markdown file to process
        /// </summary>
        public IFormFile File { get; set; }

        /// <summary>
        /// Category for organizing documents (e.g., "documentation", "tutorial", "reference")
        /// </summary>
        public string Category { get; set; } = "general";

        /// <summary>
        /// Weight for search ranking (1-10, higher = more important)
        /// </summary>
        public int Weight { get; set; } = 5;

        /// <summary>
        /// Access level (0=public, 1=authenticated, 2=admin)
        /// </summary>
        public int AccessLevel { get; set; } = 0;

        /// <summary>
        /// Number of characters per chunk
        /// </summary>
        public int ChunkSize { get; set; } = 1000;

        /// <summary>
        /// Number of overlapping characters between chunks
        /// </summary>
        public int ChunkOverlap { get; set; } = 200;

        /// <summary>
        /// Optional comma-separated keywords for metadata search
        /// </summary>
        public string? Keywords { get; set; }

        /// <summary>
        /// Optional source identifier
        /// </summary>
        public string? Source { get; set; }
    }
}
