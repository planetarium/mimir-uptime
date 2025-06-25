using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MimirUptime.Options;

namespace MimirUptime.Services
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

        public async Task<bool> SendAlertAsync(string headlessKey, string errorMessage)
        {
            if (!_options.Enabled)
            {
                return false;
            }

            try
            {
                var payload = new
                {
                    routing_key = _options.RoutingKey,
                    event_action = "trigger",
                    dedup_key = $"mimiruptime-{headlessKey}",
                    payload = new
                    {
                        summary = $"FAILURE for test {headlessKey}",
                        source = "MimirUptime Tests",
                        severity = "critical",
                        component = headlessKey,
                        custom_details = new
                        {
                            error_message = errorMessage,
                            timestamp = DateTime.UtcNow.ToString("o"),
                        },
                    },
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

        public async Task<bool> ResolveAlertAsync(string headlessKey, string resolveMessage)
        {
            if (!_options.Enabled)
            {
                return false;
            }

            try
            {
                var payload = new
                {
                    routing_key = _options.RoutingKey,
                    event_action = "resolve",
                    dedup_key = $"mimiruptime-{headlessKey}",
                    payload = new
                    {
                        summary = $"RESOLVED for test {headlessKey}",
                        source = "MimirUptime Tests",
                        component = headlessKey,
                        severity = "critical",
                        custom_details = new
                        {
                            message = resolveMessage,
                            timestamp = DateTime.UtcNow.ToString("o"),
                        },
                    },
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
