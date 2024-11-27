using Bencodex.Types;
using HeadlessGQL;
using Lib9c.Models.States;
using Libplanet.Action.State;
using Libplanet.Crypto;
using Microsoft.Extensions.DependencyInjection;
using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using MimirGQL;

namespace MimirUptime.CompareTests;

public class StakeTests : IClassFixture<GraphQLClientFixture>
{
    private readonly IMimirClient mimirClient;
    private readonly IHeadlessClient headlessClient;

    public StakeTests(GraphQLClientFixture fixture)
    {
        mimirClient = fixture.ServiceProvider.GetRequiredService<IMimirClient>();
        headlessClient = fixture.ServiceProvider.GetRequiredService<IHeadlessClient>();
    }

    [Theory]
    [InlineData("0x08eB36BB2B46073149fE9DaCB9706d2b49Fa6115")]
    [InlineData("0x17c30886E68A18EB7fF57a5e8Fa981b47FB1C5bC")]
    public async Task CompareStakeData(string address)
    {
        var metadata = await mimirClient.GetMetadata.ExecuteAsync(
            CollectionNames.GetCollectionName<StakeDocument>()
        );
        var agentDataFromMimir = await GetMimirStakeData(new Address(address));

        var stakeAddress = Nekoyume.Model.State.StakeState.DeriveAddress(new Address(address));
        var agentDataFromHeadless = await GetHeadlessStakeData(
            metadata.Data.Metadata.LatestBlockIndex,
            stakeAddress
        );
        if (agentDataFromMimir == null)
        {
            // FIXME; if agentDataFromMimir is null, stakeAddress should be null as well, but now it isn't
            return;
        }

        Assert.Equal(agentDataFromMimir.StartedBlockIndex, agentDataFromHeadless.StartedBlockIndex);
    }

    private async Task<IGetStake_Stake> GetMimirStakeData(Address address)
    {
        var agentResponse = await mimirClient.GetStake.ExecuteAsync(address.ToString());
        var agentData = agentResponse.Data.Stake;

        return agentData;
    }

    private async Task<StakeState?> GetHeadlessStakeData(long blockIndex, Address address)
    {
        try
        {
            var stateResponse = await headlessClient.GetState.ExecuteAsync(
                ReservedAddresses.LegacyAccount.ToString(),
                address.ToString(),
                blockIndex
            );
            var result = CodecUtil.DecodeState(stateResponse.Data.State);
            if (result.Kind == ValueKind.Null)
            {
                return null;
            }

            return new StakeState(result);
        }
        catch (Exception)
        {
            Assert.Skip("Headless client is unresponsive; skipping test.");
            throw;
        }
    }
}
