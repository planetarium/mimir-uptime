using System.Collections.Generic;
using System.Threading.Tasks;
using MimirUptime.Client;

namespace MimirUptime.TipTests
{
    public class TipTests : IClassFixture<HeadlessOptionsFixture>
    {
        private readonly HeadlessOptionsFixture _fixture;

        public TipTests(HeadlessOptionsFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task MimirTip_Should_Be_Recent()
        {
            var tasks = Constants
                .MIMIR_URLS.Select(testCase => TestNodeAsync(testCase.Key, testCase.Value))
                .ToList();
            var results = await Task.WhenAll(tasks);

            var failures = results.Where(r => !string.IsNullOrEmpty(r)).ToList();

            if (failures.Count > 0)
            {
                Assert.Fail(string.Join("\n", failures));
            }
        }

        private async Task<string> TestNodeAsync(string headlessKey, string routingKey)
        {
            try
            {
                var headlessUrl = Constants.HEADLESS_URLS[headlessKey];
                var mimirUrl = Constants.MIMIR_URLS[headlessKey];

                var headlessOptions = _fixture.HeadlessOptions.Value;
                var mimirOptions = _fixture.MimirOptions.Value;

                var headlessClient = new HeadlessGQLClient(
                    new Uri(headlessUrl),
                    headlessOptions.JwtIssuer,
                    headlessOptions.JwtSecretKey
                );
                var mimirClient = new MimirGQLClient(
                    new Uri(mimirUrl),
                    mimirOptions.JwtIssuer,
                    mimirOptions.JwtSecretKey
                );

                try
                {
                    var (response, _) = await headlessClient.GetTipAsync();
                    var blockIndexFromHeadless = response.NodeStatus.Tip.Index;

                    foreach (var collectionName in Constants.COLLECTION_NAMES)
                    {
                        try
                        {
                            var (metadataResponse, _) = await mimirClient.GetMetadataAsync(collectionName);
                            var blockIndexFromMimir = metadataResponse.Metadata.latestBlockIndex;

                            var blockDifference = blockIndexFromHeadless - blockIndexFromMimir;

                            Assert.True(
                                blockDifference <= 50,
                                $"Collection '{collectionName}' has a block difference of {blockDifference}. "
                                    + $"Mimir block index: {blockIndexFromMimir}, Headless block index: {blockIndexFromHeadless}"
                            );
                        }
                        catch (HttpRequestException ex) when (ex.Message.Contains("429"))
                        {
                            return string.Empty;
                        }
                        catch (Exception ex)
                        {
                            return $"Error checking collection '{collectionName}': {ex.Message}";
                        }
                    }

                    var resolveMessage =
                        $"Block timestamp for {headlessKey} is now valid.";
                    await _fixture.PagerDutyService.ResolveAlertAsync(
                        headlessKey,
                        resolveMessage
                    );
                }
                catch (HttpRequestException ex)
                {
                    return $"Headless request error for {headlessKey}: {ex.Message}";
                }
            }
            catch (Exception ex)
            {
                await _fixture.PagerDutyService.SendAlertAsync(
                    headlessKey,
                    $"Error checking {headlessKey} mimir block: {ex.Message}"
                );
                return $"Error checking {headlessKey} mimir block: {ex.Message}";
            }

            return string.Empty;
        }
    }
}
