using System.Collections.Generic;

namespace Domain.DTO.Responses
{
    /// <summary>
    /// RAG system statistics
    /// </summary>
    public class StatisticsDto
    {
        public long TotalDocuments { get; set; }
        public long TotalFiles { get; set; }
        public Dictionary<string, long> DocumentsByCategory { get; set; }
        public Dictionary<string, long> DocumentsBySource { get; set; }
        public Dictionary<int, long> DocumentsByAccessLevel { get; set; }
        public double AverageChunksPerFile { get; set; }
    }
}
