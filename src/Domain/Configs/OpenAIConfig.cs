namespace Domain.Configs
{
    /// <summary>
    /// Configuration for OpenAI API integration
    /// </summary>
    public class OpenAIConfig
    {
        /// <summary>
        /// OpenAI API key (sk-...)
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// OpenAI Assistant ID for assistant API (optional)
        /// </summary>
        public string AssistantID { get; set; }

        /// <summary>
        /// Embedding model to use (default: text-embedding-ada-002)
        /// </summary>
        public string EmbeddingModel { get; set; }
    }
}
