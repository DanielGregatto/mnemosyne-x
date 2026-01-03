using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Services.Infrastructure
{
    public class RagIngestionUtilities : IRagIngestionUtilities
    {
        public List<string> ChunkContent(string content, int chunkSize, int overlap)
        {
            var chunks = new List<string>();
            var position = 0;

            while (position < content.Length)
            {
                var remainingLength = content.Length - position;
                var currentChunkSize = Math.Min(chunkSize, remainingLength);

                var chunk = content.Substring(position, currentChunkSize);
                chunks.Add(chunk);

                position += chunkSize - overlap;

                if (position >= content.Length)
                    break;
            }

            return chunks;
        }

        public float[] GenerateFallbackEmbedding(string content)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(content));
            var embedding = new float[1536];

            for (int i = 0; i < embedding.Length; i++)
            {
                var byteIndex = i % hash.Length;
                embedding[i] = (hash[byteIndex] / 128f) - 1f;
            }

            return embedding;
        }

        public string ComputeHash(string content)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(content));
            return Convert.ToBase64String(hash);
        }

        public string ComputeHash(float[] embedding)
        {
            var bytes = new byte[embedding.Length * sizeof(float)];
            Buffer.BlockCopy(embedding, 0, bytes, 0, bytes.Length);
            var hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
