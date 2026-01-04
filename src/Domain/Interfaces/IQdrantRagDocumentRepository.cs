using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    /// <summary>
    /// Repository interface for Qdrant vector database operations
    /// Note: RagDocument refers to the domain model from rag-api-master
    /// </summary>
    public interface IQdrantRagDocumentRepository
    {
        /// <summary>
        /// Initialize Qdrant collection if it doesn't exist
        /// </summary>
        Task<bool> InitializeAsync();

        /// <summary>
        /// Insert or update a document in Qdrant
        /// </summary>
        Task<string> UpsertAsync(object document);

        /// <summary>
        /// Search documents by semantic similarity
        /// </summary>
        /// <param name="queryEmbedding">Query embedding vector</param>
        /// <param name="limit">Maximum number of results</param>
        /// <param name="accessLevel">Filter by access level (null = no filter)</param>
        Task<IEnumerable<object>> SearchBySimilarityAsync(float[] queryEmbedding, int limit = 10, int? accessLevel = null);

        /// <summary>
        /// Get similarity score for a specific document against a query
        /// Used by Factor X expansion algorithm
        /// </summary>
        Task<(object Document, float Score)?> GetSimilarityScoreWithQueryAsync(Guid documentId, float[] queryEmbedding);

        /// <summary>
        /// Search documents by metadata filters
        /// </summary>
        Task<IEnumerable<object>> SearchByMetadataAsync(Dictionary<string, object> filters, int limit = 100);

        /// <summary>
        /// Get document by ID
        /// </summary>
        Task<object?> GetByIdAsync(Guid id);

        /// <summary>
        /// Get all documents from a specific source
        /// </summary>
        Task<IEnumerable<object>> GetBySourceAsync(string source);

        /// <summary>
        /// Asynchronously retrieves all objects associated with the specified file name.
        /// </summary>
        /// <param name="existingFileName">The name of the file to search for. Cannot be <see langword="null"/> or empty.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of objects
        /// associated with the specified file name. The collection is empty if no objects are found.</returns>
        Task<IEnumerable<object>> GetByFileNameAsync(string existingFileName);

        /// <summary>
        /// Delete document by ID
        /// </summary>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Delete all documents from a specific source
        /// </summary>
        Task<bool> DeleteBySourceAsync(string source);

        /// <summary>
        /// Get total document count
        /// </summary>
        Task<long> CountAsync();

        /// <summary>
        /// Get statistics (by category, source, etc.)
        /// </summary>
        Task<object> GetStatisticsAsync();        
    }
}
