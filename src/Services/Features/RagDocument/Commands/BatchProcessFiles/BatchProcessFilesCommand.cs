using Domain.DTO.Infrastructure.CQRS;
using Domain.DTO.Responses;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Services.Features.RagDocument.Commands.BatchProcessFiles
{
    /// <summary>
    /// Command to process multiple markdown files in batch
    /// </summary>
    public class BatchProcessFilesCommand : IRequest<Result<List<ProcessFileResultDto>>>
    {
        /// <summary>
        /// List of files to process
        /// </summary>
        public List<IFormFile> Files { get; set; } = new List<IFormFile>();

        /// <summary>
        /// Category for organizing documents
        /// </summary>
        public string Category { get; set; } = "general";

        /// <summary>
        /// Weight for search ranking (1-10)
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
        /// Optional comma-separated keywords
        /// </summary>
        public string? Keywords { get; set; }

        /// <summary>
        /// Optional source identifier
        /// </summary>
        public string? Source { get; set; }
    }
}
