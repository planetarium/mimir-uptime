using MimirUptime.Client;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MimirUptime.BlockTests
{
    public class BlockTimestampTests : IClassFixture<HeadlessOptionsFixture>
    {
        private readonly HeadlessOptionsFixture _fixture;

        public BlockTimestampTests(HeadlessOptionsFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Block_Timestamp_Should_Be_Recent()
        {
            var tasks = Constants.MIMIR_URLS.Select(testCase => TestNodeAsync(testCase.Key, testCase.Value)).ToList();
            var results = await Task.WhenAll(tasks);
            
            var failures = results.Where(r => !string.IsNullOrEmpty(r)).ToList();

            if (failures.Count > 0)
            {
                Assert.True(false, string.Join("\n", failures));
            }
        }

        private async Task<string> TestNodeAsync(string headlessKey, string routingKey)
        {
            try
            {
                var headlessUrl = Constants.HEADLESS_URLS[headlessKey];
                var headlessOptions = _fixture.HeadlessOptions.Value;

                var client = new HeadlessGQLClient(
                    new Uri(headlessUrl),
                    headlessOptions.JwtIssuer,
                    headlessOptions.JwtSecretKey
                );

                var (response, _) = await client.GetTipAsync();

                if (
                    response?.ChainQuery?.BlockQuery?.Blocks != null
                    && response.ChainQuery.BlockQuery.Blocks.Count > 0
                    && !string.IsNullOrEmpty(response.ChainQuery.BlockQuery.Blocks[0].Timestamp)
                )
                {
                    var blockTimestamp = DateTime.Parse(
                        response.ChainQuery.BlockQuery.Blocks[0].Timestamp
                    ).ToUniversalTime();
                    var currentTime = DateTime.UtcNow;

                    var timeDifference = (currentTime - blockTimestamp).TotalSeconds;

                    var isValid = timeDifference < 300;

                    if (!isValid)
                    {
                        var errorMessage =
                            $"Block timestamp for {headlessKey} is too old. Difference: {timeDifference} seconds.";

                        await _fixture.PagerDutyService.SendAlertAsync(
                            headlessKey,
                            routingKey,
                            errorMessage
                        );

                        return errorMessage;
                    }
                    else
                    {
                        var resolveMessage =
                            $"Block timestamp for {headlessKey} is now valid. Difference: {timeDifference} seconds.";
                        await _fixture.PagerDutyService.ResolveAlertAsync(
                            headlessKey,
                            routingKey,
                            resolveMessage
                        );
                    }
                }
                else
                {
                    var errorMessage = $"No valid block data received for {headlessKey}";
                    await _fixture.PagerDutyService.SendAlertAsync(
                        headlessKey,
                        routingKey,
                        errorMessage
                    );
                    return errorMessage;
                }
            }
            catch (Exception ex)
            {
                await _fixture.PagerDutyService.SendAlertAsync(
                    headlessKey,
                    routingKey,
                    $"Error checking {headlessKey} block timestamp: {ex.Message}"
                );
                return $"Error checking {headlessKey} block timestamp: {ex.Message}";
            }
            
            return string.Empty;
        }
    }
}
