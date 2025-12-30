using System.Collections.Generic;

namespace Domain.DTO.Infrastructure.API
{
    public class TurnstileValidationDto
    {
        public bool success { get; set; }
        public string challenge_ts { get; set; }
        public string hostname { get; set; }
        public List<string> errorCodes { get; set; }
    }
}
