using System.Text.Json.Serialization;

namespace Domain.DTO.OpenAI
{
    /// <summary>
    /// Request model for OpenAI Embeddings API
    /// </summary>
    public class EmbeddingRequest
    {
        /// <summary>
        /// Input text to generate embeddings for
        /// </summary>
        [JsonPropertyName("input")]
        public string Input { get; set; }

        /// <summary>
        /// Model to use for embedding generation (default: text-embedding-ada-002)
        /// </summary>
        [JsonPropertyName("model")]
        public string Model { get; set; } = "text-embedding-ada-002";

        /// <summary>
        /// Format for returned embeddings (default: float)
        /// </summary>
        [JsonPropertyName("encoding_format")]
        public string EncodingFormat { get; set; } = "float";
    }
}
