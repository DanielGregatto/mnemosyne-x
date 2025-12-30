using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Domain.DTO.OpenAI
{
    /// <summary>
    /// Response model from OpenAI Embeddings API
    /// </summary>
    public class EmbeddingResponse
    {
        [JsonPropertyName("object")]
        public string Object { get; set; }

        [JsonPropertyName("data")]
        public List<EmbeddingData> Data { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("usage")]
        public EmbeddingUsage Usage { get; set; }
    }

    /// <summary>
    /// Individual embedding data item
    /// </summary>
    public class EmbeddingData
    {
        [JsonPropertyName("object")]
        public string Object { get; set; }

        /// <summary>
        /// The embedding vector (float array)
        /// </summary>
        [JsonPropertyName("embedding")]
        public float[] Embedding { get; set; }

        [JsonPropertyName("index")]
        public int Index { get; set; }
    }

    /// <summary>
    /// Token usage information
    /// </summary>
    public class EmbeddingUsage
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }
}
