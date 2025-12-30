using Domain.Configs;
using Domain.DTO.OpenAI;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Services.Infrastructure
{
    /// <summary>
    /// OpenAI API service for generating text embeddings
    /// </summary>
    public class OpenAIService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OpenAIService> _logger;
        private readonly OpenAIConfig _config;

        public OpenAIService(
            HttpClient httpClient,
            IOptions<OpenAIConfig> config,
            ILogger<OpenAIService> logger)
        {
            _httpClient = httpClient;
            _config = config.Value;
            _logger = logger;

            // Ensure base address and authentication are set
            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri("https://api.openai.com/v1/");
            }

            // Authorization header should be set via IHttpClientFactory configuration
            // but we can ensure it's set here as fallback
            if (!string.IsNullOrEmpty(_config.Token) &&
                _httpClient.DefaultRequestHeaders.Authorization == null)
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.Token}");
            }
        }

        /// <summary>
        /// Generate embedding vector for the given text using OpenAI's API
        /// </summary>
        /// <param name="text">Text to embed</param>
        /// <returns>Embedding vector (1536 dimensions for ada-002)</returns>
        public async Task<float[]> GenerateEmbeddingAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                _logger.LogWarning("Empty text provided for embedding generation");
                return Array.Empty<float>();
            }

            try
            {
                var request = new EmbeddingRequest
                {
                    Input = text,
                    Model = _config.EmbeddingModel ?? "text-embedding-ada-002",
                    EncodingFormat = "float"
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogDebug("Generating embedding for text of length {Length} using model {Model}",
                    text.Length, request.Model);

                var response = await _httpClient.PostAsync("embeddings", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to generate embedding. Status: {StatusCode}, Error: {ErrorMessage}",
                        response.StatusCode, errorMessage);

                    // Return empty array on failure
                    return Array.Empty<float>();
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var embeddingResponse = JsonSerializer.Deserialize<EmbeddingResponse>(responseJson);

                if (embeddingResponse?.Data?.Count > 0)
                {
                    var embedding = embeddingResponse.Data[0].Embedding;
                    _logger.LogDebug("Successfully generated embedding with {Dimensions} dimensions. Tokens used: {Tokens}",
                        embedding?.Length ?? 0, embeddingResponse.Usage?.TotalTokens ?? 0);

                    return embedding ?? Array.Empty<float>();
                }

                _logger.LogWarning("No embedding data received from OpenAI API");
                return Array.Empty<float>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error generating embedding: {Message}", ex.Message);
                return Array.Empty<float>();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization error: {Message}", ex.Message);
                return Array.Empty<float>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error generating embedding: {Message}", ex.Message);
                return Array.Empty<float>();
            }
        }
    }
}
