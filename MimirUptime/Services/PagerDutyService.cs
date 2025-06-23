using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NodeUptime.Options;

namespace NodeUptime.Services
{
    public class PagerDutyService
    {
        private readonly HttpClient _httpClient;
        private readonly PagerDutyOption _options;
        private const string PagerDutyEventsApiUrl = "https://events.pagerduty.com/v2/enqueue";

        public PagerDutyService(IOptions<PagerDutyOption> options)
        {
            _httpClient = new HttpClient();
            _options = options.Value;
        }

        public async Task<bool> SendAlertAsync(
            string headlessKey,
            string routingKey,
            string errorMessage
        )
        {
            if (!_options.Enabled || !_options.RoutingKeys.ContainsKey(routingKey))
            {
                return false;
            }

            try
            {
                var pagerdutyRoutingKey = _options.RoutingKeys[routingKey];
                var payload = new
                {
                    routing_key = pagerdutyRoutingKey,
                    event_action = "trigger",
                    dedup_key = $"nodeuptime-{headlessKey}",
                    payload = new
                    {
                        summary = $"FAILURE for test {headlessKey}",
                        source = "NodeUptime Tests",
                        severity = "critical",
                        component = headlessKey,
                        custom_details = new
                        {
                            error_message = errorMessage,
                            timestamp = DateTime.UtcNow.ToString("o")
                        }
                    }
                };

                var jsonContent = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(PagerDutyEventsApiUrl, content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ResolveAlertAsync(
            string headlessKey,
            string routingKey,
            string resolveMessage
        )
        {
            if (!_options.Enabled || !_options.RoutingKeys.ContainsKey(routingKey))
            {
                return false;
            }

            try
            {
                var pagerdutyRoutingKey = _options.RoutingKeys[routingKey];
                var payload = new
                {
                    routing_key = pagerdutyRoutingKey,
                    event_action = "resolve",
                    dedup_key = $"nodeuptime-{headlessKey}",
                    payload = new
                    {
                        summary = $"RESOLVED for test {headlessKey}",
                        source = "NodeUptime Tests",
                        component = headlessKey,
                        severity = "critical",
                        custom_details = new
                        {
                            message = resolveMessage,
                            timestamp = DateTime.UtcNow.ToString("o")
                        }
                    }
                };

                var jsonContent = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(PagerDutyEventsApiUrl, content);
                var jsonResponse = await response.Content.ReadAsStringAsync();
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
