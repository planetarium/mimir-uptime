using System;
using System.ComponentModel.DataAnnotations;

namespace MimirUptime.Options
{
    public class HeadlessOption
    {
        public const string SectionName = "Headless";

        [Required]
        public string JwtIssuer { get; set; } = string.Empty;

        [Required]
        public string JwtSecretKey { get; set; } = string.Empty;
    }
}
