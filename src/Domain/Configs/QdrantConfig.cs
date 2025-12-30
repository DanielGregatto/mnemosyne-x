namespace Domain.Configs
{
    /// <summary>
    /// Configuration for Qdrant vector database
    /// </summary>
    public class QdrantConfig
    {
        /// <summary>
        /// Qdrant server host (e.g., "localhost" or "qdrant.example.com")
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Qdrant server port (default: 6334 for gRPC)
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Optional API key for authentication
        /// </summary>
        public string? ApiKey { get; set; }

        /// <summary>
        /// Use HTTPS for connection (default: false)
        /// </summary>
        public bool UseHttps { get; set; } = false;

        /// <summary>
        /// Collection name for RAG documents
        /// </summary>
        public string CollectionName { get; set; }

        /// <summary>
        /// Vector dimension size (1536 for text-embedding-ada-002)
        /// </summary>
        public int VectorSize { get; set; }

        /// <summary>
        /// Distance metric: "cosine", "dot", or "euclidean"
        /// </summary>
        public string Distance { get; set; }

        /// <summary>
        /// Connection timeout in milliseconds
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        /// Maximum number of retry attempts
        /// </summary>
        public int MaxRetries { get; set; }
    }
}
