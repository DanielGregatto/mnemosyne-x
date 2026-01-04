using Domain;
using Domain.Configs;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.Repository
{
    /// <summary>
    /// Repository for managing RAG documents in Qdrant vector database
    /// </summary>
    public class QdrantRagDocumentRepository : IQdrantRagDocumentRepository
    {
        private readonly QdrantClient _client;
        private readonly QdrantConfig _config;
        private readonly ILogger<QdrantRagDocumentRepository> _logger;

        public QdrantRagDocumentRepository(IOptions<QdrantConfig> config, ILogger<QdrantRagDocumentRepository> logger)
        {
            _config = config.Value;
            _logger = logger;

            _client = new QdrantClient(
                host: _config.Host,
                port: _config.Port,
                https: _config.UseHttps,
                apiKey: _config.ApiKey
            );

            try
            {
                _logger.LogInformation("Initializing Qdrant connection to {Host}:{Port}", _config.Host, _config.Port);

                // Initialize collection (will create if doesn't exist)
                _ = this.InitializeAsync().GetAwaiter().GetResult();

                _logger.LogInformation("Connected to Qdrant successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to Qdrant at {Host}:{Port}", _config.Host, _config.Port);
                throw;
            }
        }

        public async Task<bool> InitializeAsync()
        {
            try
            {
                // Check if collection exists
                if (await CollectionExistsAsync())
                {
                    _logger.LogInformation("Collection '{CollectionName}' already exists", _config.CollectionName);
                    return true;
                }

                // Create collection with vector configuration
                await _client.CreateCollectionAsync(_config.CollectionName, new VectorParams
                {
                    Size = (ulong)_config.VectorSize,
                    Distance = ParseDistance(_config.Distance)
                });

                _logger.LogInformation("Created Qdrant collection '{CollectionName}' with {VectorSize} dimensions",
                    _config.CollectionName, _config.VectorSize);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Qdrant collection '{CollectionName}'", _config.CollectionName);
                return false;
            }
        }

        public async Task<string> UpsertAsync(object doc)
        {
            if (doc is not RagDocument document)
                throw new ArgumentException("Document must be of type RagDocument");

            try
            {
                if (document.Embedding == null || document.Embedding.Length == 0)
                    throw new ArgumentException("Document must have a valid embedding");

                var pointId = document.Id.ToString();
                var payload = CreatePayloadFromDocument(document);

                var point = new PointStruct
                {
                    Id = new PointId { Uuid = pointId },
                    Vectors = document.Embedding,
                    Payload = { payload }
                };

                await _client.UpsertAsync(_config.CollectionName, new[] { point });

                _logger.LogDebug("Upserted document {DocumentId} to collection '{CollectionName}'",
                    pointId, _config.CollectionName);
                return pointId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upsert document {DocumentId}", document.Id);
                throw;
            }
        }

        public async Task<IEnumerable<object>> SearchBySimilarityAsync(float[] queryEmbedding, int limit = 10, int? accessLevel = null)
        {
            try
            {
                Filter? filter = null;

                // Add access level filter if specified
                if (accessLevel.HasValue)
                {
                    filter = new Filter
                    {
                        Should =
                        {
                            new Condition
                            {
                                Field = new FieldCondition
                                {
                                    Key = "access_level",
                                    Range = new Qdrant.Client.Grpc.Range
                                    {
                                        Lte = accessLevel.Value
                                    }
                                }
                            }
                        }
                    };
                }

                var searchResult = await _client.SearchAsync(_config.CollectionName,
                                                             queryEmbedding,
                                                             filter,
                                                             limit: (ulong)limit
                                                            );

                var ids = searchResult.Select(r => new PointId { Uuid = r.Id.Uuid }).ToArray();

                var retrieved = await _client.RetrieveAsync(
                    _config.CollectionName,
                    ids,
                    withVectors: true,
                    withPayload: true);

                var documents = new List<RagDocument>();

                foreach (var point in retrieved)
                {
                    var document = CreateDocumentFromPayload(
                        point.Id.Uuid,
                        point.Payload.ToDictionary(p => p.Key, p => p.Value));

                    if (point.Vectors != null && point.Vectors.Vector != null)
                        document.Embedding = point.Vectors.Vector.Data.ToArray();

                    documents.Add(document);
                }

                _logger.LogDebug("Found {Count} documents in similarity search (limit: {Limit})",
                    documents.Count, limit);
                return documents.Cast<object>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to search documents by similarity");
                throw;
            }
        }

        public async Task<(object Document, float Score)?> GetSimilarityScoreWithQueryAsync(Guid documentId, float[] queryEmbedding)
        {
            // retrieve the doc (to ensure it exists and to get embedding)
            var point = await this.GetByIdAsync(documentId);

            if (point == null)
                return null;

            // perform search against this single point
            var searchResult = await _client.SearchAsync(
                _config.CollectionName,
                queryEmbedding,
                filter: new Filter
                {
                    Must =
                    {
                        new Condition
                        {
                            Field = new FieldCondition
                            {
                                Key = "id",
                                Match = new Match { Text = documentId.ToString() }
                            }
                        }
                    }
                },
                limit: 1
            );

            var ids = searchResult.Select(r => new PointId { Uuid = r.Id.Uuid }).ToArray();

            var retrieved = await _client.RetrieveAsync(
                _config.CollectionName,
                ids,
                withVectors: true,
                withPayload: true);

            var firstSearch = searchResult.FirstOrDefault();
            var firstRetrieved = retrieved.FirstOrDefault();
            if (firstRetrieved == null)
                return null;

            var document = CreateDocumentFromPayload(
                firstRetrieved.Id.Uuid,
                firstRetrieved.Payload.ToDictionary(p => p.Key, p => p.Value));

            if (firstRetrieved.Vectors != null && firstRetrieved.Vectors.Vector != null)
                document.Embedding = firstRetrieved.Vectors.Vector.Data.ToArray();

            return (document, firstSearch.Score);
        }

        public async Task<IEnumerable<object>> SearchByMetadataAsync(Dictionary<string, object> filters, int limit = 100)
        {
            try
            {
                var filter = CreateFilterFromDictionary(filters);

                var scrollResult = await _client.ScrollAsync(_config.CollectionName, filter, limit: (uint)limit);
                var documents = new List<RagDocument>();

                foreach (var point in scrollResult.Result)
                {
                    var document = CreateDocumentFromPayload(
                        point.Id.Uuid,
                        point.Payload.ToDictionary(p => p.Key, p => p.Value));

                    if (document != null)
                    {
                        documents.Add(document);
                    }
                }

                _logger.LogDebug("Found {Count} documents in metadata search", documents.Count);
                return documents.Cast<object>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to search documents by metadata");
                throw;
            }
        }

        public async Task<object?> GetByIdAsync(Guid id)
        {
            try
            {
                var points = await _client.RetrieveAsync(
                    _config.CollectionName,
                    new[] { new PointId { Uuid = id.ToString() } },
                    withPayload: true,
                    withVectors: true);

                var point = points.FirstOrDefault();

                if (point != null)
                {
                    var document = CreateDocumentFromPayload(
                        point.Id.Uuid,
                        point.Payload.ToDictionary(p => p.Key, p => p.Value));

                    if (point.Vectors != null && point.Vectors.Vector != null)
                        document.Embedding = point.Vectors.Vector.Data.ToArray();

                    return document;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get document {DocumentId}", id);
                return null;
            }
        }

        public async Task<IEnumerable<object>> GetBySourceAsync(string source)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(source))
                    return Enumerable.Empty<object>();

                var filters = new Dictionary<string, object>
                {
                    ["source"] = source
                };

                return await SearchByMetadataAsync(filters, 1000);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get documents by source: {Source}", source);
                throw;
            }
        }

        public async Task<IEnumerable<object>> GetByFileNameAsync(string existingFileName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(existingFileName))
                    return Enumerable.Empty<object>();

                var filters = new Dictionary<string, object>
                {
                    ["file_name"] = existingFileName
                };

                return await SearchByMetadataAsync(filters, 1000);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get documents by file_name: {existingFileName}", existingFileName);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                var filter = new Filter
                {
                    Must =
                    {
                        new Condition
                        {
                            Field = new FieldCondition
                            {
                                Key = "id",
                                Match = new Match { Text = id.ToString() }
                            }
                        }
                    }
                };

                await _client.DeleteAsync(_config.CollectionName, filter);
                _logger.LogDebug("Deleted document {DocumentId} from Qdrant", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete document {DocumentId}", id);
                return false;
            }
        }

        public async Task<bool> DeleteBySourceAsync(string source)
        {
            try
            {
                var filter = new Filter
                {
                    Must =
                    {
                        new Condition
                        {
                            Field = new FieldCondition
                            {
                                Key = "source",
                                Match = new Match { Text = source }
                            }
                        }
                    }
                };

                await _client.DeleteAsync(_config.CollectionName, filter);
                _logger.LogInformation("Deleted all documents with source '{Source}'", source);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete documents by source '{Source}'", source);
                return false;
            }
        }

        public async Task<long> CountAsync()
        {
            try
            {
                var info = await _client.GetCollectionInfoAsync(_config.CollectionName);
                return (long)info.PointsCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get document count");
                return 0;
            }
        }

        public async Task<object> GetStatisticsAsync()
        {
            try
            {
                var info = await _client.GetCollectionInfoAsync(_config.CollectionName);
                var totalDocuments = await CountAsync();

                return new
                {
                    TotalDocuments = totalDocuments,
                    CollectionName = _config.CollectionName,
                    VectorSize = _config.VectorSize,
                    Distance = _config.Distance,
                    Status = info.Status.ToString(),
                    Database = "Qdrant",
                    LastUpdated = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get statistics");
                return new
                {
                    Error = ex.Message,
                    Database = "Qdrant",
                    LastUpdated = DateTime.UtcNow
                };
            }
        }

        #region Private Helper Methods

        private async Task<bool> CollectionExistsAsync()
        {
            try
            {
                var collections = await _client.ListCollectionsAsync();
                return collections.Any(c => c == _config.CollectionName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check if collection exists");
                return false;
            }
        }

        private Distance ParseDistance(string distance)
        {
            return distance.ToLowerInvariant() switch
            {
                "cosine" => Distance.Cosine,
                "dot" => Distance.Dot,
                "euclidean" => Distance.Euclid,
                _ => Distance.Cosine
            };
        }

        private Dictionary<string, Value> CreatePayloadFromDocument(RagDocument document)
        {
            var payload = new Dictionary<string, Value>
            {
                ["id"] = document.Id.ToString(),
                ["file_name"] = document.FileName ?? "",
                ["content"] = document.Content ?? "",
                ["source"] = document.Source ?? "",
                ["category"] = document.Category ?? "",
                ["weight"] = document.Weight,
                ["access_level"] = document.AccessLevel,
                ["chunk_index"] = document.ChunkIndex,
                ["total_chunks"] = document.TotalChunks,
                ["previous_document_id"] = document.PreviousDocumentId?.ToString() ?? "",
                ["next_document_id"] = document.NextDocumentId?.ToString() ?? "",
                ["file_path"] = document.FilePath ?? "",
                ["file_size"] = document.FileSize,
                ["keywords"] = document.Keywords ?? "",
                ["embedding_model"] = document.EmbeddingModel ?? "",
                ["embedding_hash"] = document.EmbeddingHash ?? "",
                ["content_hash"] = document.ContentHash ?? "",
                ["version"] = document.Version,
                ["last_processed"] = document.LastProcessed.ToString("O"),
                ["custom_metadata"] = document.CustomMetadata ?? "",
            };

            return payload;
        }

        private RagDocument? CreateDocumentFromPayload(string pointId, Dictionary<string, Value> payload)
        {
            try
            {
                var document = new RagDocument
                {
                    Id = Guid.Parse(payload.GetValueOrDefault("id", pointId).StringValue),
                    FileName = payload.GetValueOrDefault("file_name", "").StringValue,
                    Content = payload.GetValueOrDefault("content", "").StringValue,
                    Source = payload.GetValueOrDefault("source", "").StringValue,
                    Category = payload.GetValueOrDefault("category", "").StringValue,
                    Weight = (int)payload.GetValueOrDefault("weight", 1).IntegerValue,
                    AccessLevel = (int)payload.GetValueOrDefault("access_level", 0).IntegerValue,
                    ChunkIndex = (int)payload.GetValueOrDefault("chunk_index", 0).IntegerValue,
                    TotalChunks = (int)payload.GetValueOrDefault("total_chunks", 1).IntegerValue,
                    FilePath = payload.GetValueOrDefault("file_path", "").StringValue,
                    FileSize = payload.GetValueOrDefault("file_size", 0).IntegerValue,
                    Keywords = payload.GetValueOrDefault("keywords", "").StringValue,
                    EmbeddingModel = payload.GetValueOrDefault("embedding_model", "").StringValue,
                    EmbeddingHash = payload.GetValueOrDefault("embedding_hash", "").StringValue,
                    ContentHash = payload.GetValueOrDefault("content_hash", "").StringValue,
                    Version = (int)payload.GetValueOrDefault("version", 1).IntegerValue,
                    CustomMetadata = payload.GetValueOrDefault("custom_metadata", "").StringValue,
                };

                // Parse navigation IDs
                var prevIdStr = payload.GetValueOrDefault("previous_document_id", "").StringValue;
                if (!string.IsNullOrEmpty(prevIdStr) && Guid.TryParse(prevIdStr, out var prevId))
                {
                    document.PreviousDocumentId = prevId;
                }

                var nextIdStr = payload.GetValueOrDefault("next_document_id", "").StringValue;
                if (!string.IsNullOrEmpty(nextIdStr) && Guid.TryParse(nextIdStr, out var nextId))
                {
                    document.NextDocumentId = nextId;
                }

                // Parse dates
                if (DateTime.TryParse(payload.GetValueOrDefault("last_processed", DateTime.UtcNow.ToString("O")).StringValue, out var lastProcessed))
                {
                    document.LastProcessed = lastProcessed;
                }

                return document;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create document from payload for point {PointId}", pointId);
                return null;
            }
        }

        private Filter CreateFilterFromDictionary(Dictionary<string, object> filters)
        {
            var filter = new Filter();

            foreach (var kvp in filters)
            {
                var condition = new Condition
                {
                    Field = new FieldCondition
                    {
                        Key = kvp.Key,
                        Match = new Match { Text = kvp.Value.ToString() }
                    }
                };
                filter.Must.Add(condition);
            }

            return filter;
        }

        #endregion
    }
}
