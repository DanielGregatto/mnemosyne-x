namespace Domain.DTO.Responses
{
    /// <summary>
    /// Result of processing a markdown file
    /// </summary>
    public class ProcessFileResultDto
    {
        public string FileName { get; set; }
        public int TotalChunks { get; set; }
        public int ProcessedChunks { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
