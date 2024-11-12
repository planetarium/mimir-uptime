using Bencodex.Types;
using HeadlessGQL;
using Lib9c.Models.States;
using Libplanet.Crypto;
using Microsoft.Extensions.DependencyInjection;
using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using MimirGQL;
using Nekoyume;

namespace MimirUptime.CompareTests;

public class DailyRewardTests : IClassFixture<GraphQLClientFixture>
{
    private readonly IMimirClient mimirClient;
    private readonly IHeadlessClient headlessClient;

    public DailyRewardTests(GraphQLClientFixture fixture)
    {
        mimirClient = fixture.ServiceProvider.GetRequiredService<IMimirClient>();
        headlessClient = fixture.ServiceProvider.GetRequiredService<IHeadlessClient>();
    }

    [Theory]
    [InlineData("0xAD509326770e7EB2a44ffe6Ef4116AdEA7877114")]
    public async Task CompareDailyRewardDataFromDifferentServices_ShouldMatch(string address)
    {
        var metadata = await mimirClient.GetMetadata.ExecuteAsync(
            CollectionNames.GetCollectionName<DailyRewardDocument>()
        );
        var dailyRewardDataFromMimir = await GetMimirDailyRewardData(new Address(address));
        var dailyRewardDataFromHeadless = await GetHeadlessDailyRewardData(
            metadata.Data.Metadata.LatestBlockIndex,
            new Address(address)
        );

        Assert.Equal(dailyRewardDataFromMimir, dailyRewardDataFromHeadless);
    }

    public async Task<long> GetMimirDailyRewardData(Address address)
    {
        var dailyRewardResponse = await mimirClient.GetDailyReward.ExecuteAsync(address.ToString());
        var dailyRewardData = dailyRewardResponse.Data.DailyRewardReceivedBlockIndex;

        return dailyRewardData;
    }

    public async Task<long> GetHeadlessDailyRewardData(long blockIndex, Address address)
    {
        try
        {
            var stateResponse = await headlessClient.GetState.ExecuteAsync(
                Addresses.DailyReward.ToString(),
                address.ToString(),
                blockIndex
            );
            var result = CodecUtil.DecodeState(stateResponse.Data.State);
            return (Integer)result;
        }
        catch (Exception)
        {
            Assert.Skip("Headless client is unresponsive; skipping test.");
            throw;
        }
    }
}
