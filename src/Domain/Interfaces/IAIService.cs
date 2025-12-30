using System.Threading.Tasks;

namespace Domain.Interfaces
{
    /// <summary>
    /// AI service interface for embedding generation
    /// </summary>
    public interface IAIService
    {
        /// <summary>
        /// Generate embedding vector for the given text using OpenAI
        /// </summary>
        /// <param name="text">Text to embed</param>
        /// <returns>Embedding vector (1536 dimensions for ada-002)</returns>
        Task<float[]> GenerateEmbeddingAsync(string text);
    }
}
