using System;

namespace Domain.DTO.Responses
{
    /// <summary>
    /// DTO for RAG document responses
    /// </summary>
    public class RagDocumentDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public string Content { get; set; }
        public string FilePath { get; set; }
        public long FileSize { get; set; }
        public string Category { get; set; }
        public int Weight { get; set; }
        public string Source { get; set; }
        public int Version { get; set; }
        public int AccessLevel { get; set; }
        public string Keywords { get; set; }
        public int ChunkIndex { get; set; }
        public int TotalChunks { get; set; }
        public string EmbeddingModel { get; set; }
        public DateTime LastProcessed { get; set; }
        public string CustomMetadata { get; set; }
    }
}
