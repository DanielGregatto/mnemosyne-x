using Domain.DTO.Responses;
using System;
using System.Collections.Generic;

namespace Domain
{
    /// <summary>
    /// RAG Document domain model - stored in Qdrant vector database
    /// Note: This entity is NOT persisted in SQL Server (Qdrant-only storage)
    /// </summary>
    public class RagDocument
    {
        public RagDocument()
        {
            if (BackgroundInformation == null)
                BackgroundInformation = new List<RagDocument>();
        }

        public Guid Id { get; set; }

        /// <summary>
        /// Name of the markdown file including extension (e.g. "user-guide.md", "api-docs.md")
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Text content extracted from the markdown file (e.g. "# Getting Started\nThis guide will...")
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// SHA-256 hash of the embedding vector for deduplication (e.g. "a1b2c3d4e5f6...")
        /// </summary>
        public string EmbeddingHash { get; set; }

        /// <summary>
        /// Vector representation of the content for similarity search (e.g. [0.1, -0.3, 0.8, ...])
        /// 1536 dimensions for text-embedding-ada-002
        /// </summary>
        public float[] Embedding { get; set; }

        /// <summary>
        /// SHA-256 hash of the content for change detection (e.g. "f6e5d4c3b2a1...")
        /// </summary>
        public string ContentHash { get; set; }

        /// <summary>
        /// Index of this chunk within the original document (e.g. 0, 1, 2 for multi-chunk files)
        /// </summary>
        public int ChunkIndex { get; set; }

        /// <summary>
        /// Total number of chunks this document was split into (e.g. 1 for small files, 5 for large files)
        /// </summary>
        public int TotalChunks { get; set; }

        /// <summary>
        /// Navigation: ID of the previous document chunk in sequence (null if this is the first chunk)
        /// Used for Factor X context expansion
        /// </summary>
        public Guid? PreviousDocumentId { get; set; }

        /// <summary>
        /// Navigation: ID of the next document chunk in sequence (null if this is the last chunk)
        /// Used for Factor X context expansion
        /// </summary>
        public Guid? NextDocumentId { get; set; }

        /// <summary>
        /// Full path to the source file on disk (e.g. "/docs/guides/user-guide.md")
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Size of the original file in bytes (e.g. 2048, 4096)
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Document classification for organizing content by topic or type (e.g. "Documentation", "FAQ", "API Guide")
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Priority/importance score for search result ranking (e.g. 1=low, 5=medium, 10=high)
        /// </summary>
        public int Weight { get; set; }

        /// <summary>
        /// Source system or origin of the document (e.g. "Wiki", "GitHub", "Internal Docs")
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Document version for tracking changes and updates (e.g. 1, 2, 3)
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Access control level for permission management (e.g. 0=public, 1=internal, 2=admin)
        /// </summary>
        public int AccessLevel { get; set; }

        /// <summary>
        /// Optional search keywords for better discoverability (e.g. "authentication, login, security")
        /// </summary>
        public string Keywords { get; set; }

        /// <summary>
        /// AI model used for generating embeddings (e.g. "text-embedding-ada-002", "all-MiniLM-L6-v2")
        /// </summary>
        public string EmbeddingModel { get; set; }

        /// <summary>
        /// Timestamp of when the document was last processed/embedded (e.g. 2024-01-15 14:30:00)
        /// </summary>
        public DateTime LastProcessed { get; set; }

        /// <summary>
        /// Optional JSON metadata for flexible custom properties (e.g. "{"author": "John", "department": "IT"}")
        /// </summary>
        public string CustomMetadata { get; set; }

        /// <summary>
        /// Background information documents to provide additional context during retrieval
        /// Populated during Factor X expansion in search queries
        /// </summary>
        public List<RagDocument> BackgroundInformation { get; set; }

        /// <summary>
        /// Maps the current <see cref="RagDocument"/> instance to a <see cref="RagDocumentDto"/> object.
        /// </summary>
        /// <remarks>This method creates a new <see cref="RagDocumentDto"/> and copies all relevant
        /// properties from the current <see cref="RagDocument"/>. Use this method to convert domain entities to data
        /// transfer objects for serialization, transport, or API responses.</remarks>
        /// <returns>A <see cref="RagDocumentDto"/> containing the property values of the current <see cref="RagDocument"/>.</returns>
        public RagDocumentDto MapToDto()
        {
            return new RagDocumentDto
            {
                Id = this.Id,
                FileName = this.FileName,
                Content = this.Content,
                Source = this.Source,
                Category = this.Category,
                Weight = this.Weight,
                AccessLevel = this.AccessLevel,
                ChunkIndex = this.ChunkIndex,
                TotalChunks = this.TotalChunks,
                FilePath = this.FilePath,
                FileSize = this.FileSize,
                Keywords = this.Keywords,
                EmbeddingModel = this.EmbeddingModel,
                Version = this.Version,
                LastProcessed = this.LastProcessed
            };
        }
    }
}
