using System.Collections.Generic;

namespace MimirUptime.Options
{
    public class PagerDutyOption
    {
        public const string SectionName = "PagerDuty";

        public string RoutingKey { get; set; } = string.Empty;

        public bool Enabled { get; set; } = false;
    }
}
