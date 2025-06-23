using System;

namespace NodeUptime.Options
{
    public class HeadlessOption
    {
        public const string SectionName = "Headless";

        public string JwtIssuer { get; init; }

        public string JwtSecretKey { get; init; }
    }
}
