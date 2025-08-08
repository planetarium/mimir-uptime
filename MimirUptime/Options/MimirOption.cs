using System;
using System.ComponentModel.DataAnnotations;

namespace MimirUptime.Options
{
    public class MimirOption
    {
        public const string SectionName = "Mimir";

        [Required]
        public string JwtIssuer { get; set; } = string.Empty;

        [Required]
        public string JwtSecretKey { get; set; } = string.Empty;
    }
}
