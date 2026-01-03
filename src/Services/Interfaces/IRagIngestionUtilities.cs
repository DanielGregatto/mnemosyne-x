using System.Collections.Generic;

namespace Services.Interfaces
{
    public interface IRagIngestionUtilities
    {
        /// <summary>
        /// Splits the specified string into a sequence of overlapping chunks of the given size.
        /// </summary>
        /// <remarks>Each chunk contains up to <paramref name="chunkSize"/> characters from <paramref
        /// name="content"/>. Chunks are created with the specified number of overlapping characters, so each chunk
        /// (except the first) shares <paramref name="overlap"/> characters with the previous chunk. This method is
        /// useful for processing large strings in segments with context preserved between chunks.</remarks>
        /// <param name="content">The string to be divided into chunks. Cannot be <see langword="null"/>.</param>
        /// <param name="chunkSize">The maximum length, in characters, of each chunk. Must be greater than zero.</param>
        /// <param name="overlap">The number of characters that each chunk overlaps with the previous chunk. Must be zero or greater and less
        /// than <paramref name="chunkSize"/>.</param>
        /// <returns>A list of strings, each representing a chunk of the original content. The final chunk may be shorter if the
        /// content length is not a multiple of <paramref name="chunkSize"/>.</returns>
        List<string> ChunkContent(string content, int chunkSize, int overlap);

        /// <summary>
        /// Computes a SHA-256 hash of the specified embedding and returns it as a Base64-encoded string.
        /// </summary>
        /// <param name="embedding">An array of floating-point values representing the embedding to hash. Cannot be <see langword="null"/>.</param>
        /// <returns>A Base64-encoded string containing the SHA-256 hash of the embedding.</returns>
        string ComputeHash(float[] embedding);

        /// <summary>
        /// Computes a SHA-256 hash of the specified string and returns the result as a Base64-encoded string.
        /// </summary>
        /// <param name="content">The input string to hash. Cannot be <see langword="null"/>.</param>
        /// <returns>A Base64-encoded string representing the SHA-256 hash of <paramref name="content"/>.</returns>
        string ComputeHash(string content);

        /// <summary>
        /// Generates a deterministic fallback embedding vector for the specified content.
        /// </summary>
        /// <remarks>This method provides a simple, deterministic embedding as a fallback when a true
        /// model-based embedding is unavailable. The resulting vector is not semantically meaningful and should only be
        /// used as a placeholder.</remarks>
        /// <param name="content">The input string for which to generate the fallback embedding. Cannot be <see langword="null"/>.</param>
        /// <returns>A float array of length 1536 representing the embedding for the provided content. The same input will always
        /// produce the same embedding.</returns>
        float[] GenerateFallbackEmbedding(string content);
    }
}