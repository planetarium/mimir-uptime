using HeadlessGQL;
using Lib9c.Models.States;
using Libplanet.Crypto;
using Microsoft.Extensions.DependencyInjection;
using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using MimirGQL;
using Nekoyume;

namespace MimirUptime;

public class WorldInformationTests : IClassFixture<GraphQLClientFixture>
{
    private readonly IMimirClient mimirClient;
    private readonly IHeadlessClient headlessClient;

    public WorldInformationTests(GraphQLClientFixture fixture)
    {
        mimirClient = fixture.ServiceProvider.GetRequiredService<IMimirClient>();
        headlessClient = fixture.ServiceProvider.GetRequiredService<IHeadlessClient>();
    }

    public async Task<WorldInformationState> GetHeadlessWorldInformationData(
        long blockIndex,
        Address address
    )
    {
        var stateResponse = await headlessClient.GetState.ExecuteAsync(
            Addresses.WorldInformation.ToString(),
            address.ToString(),
            blockIndex
        );
        var result = CodecUtil.DecodeState(stateResponse.Data.State);
        return new WorldInformationState(result);
    }

    public async Task<IGetWorldInformation_WorldInformation> GetMimirWorldInformationData(
        Address address
    )
    {
        var agentResponse = await mimirClient.GetWorldInformation.ExecuteAsync(address.ToString());
        return agentResponse.Data.WorldInformation;
    }

    [Theory]
    [InlineData("4AB43b2d1d0e41DdF99449086dC70dC79513B0F1")]
    public async Task CompareWorldInformationData(string address)
    {
        var metadata = await mimirClient.GetMetadata.ExecuteAsync(
            CollectionNames.GetCollectionName<AgentDocument>()
        );
        var agentDataFromMimir = (
            await GetMimirWorldInformationData(new Address(address))
        ).WorldDictionary;
        var agentDataFromHeadless = (
            await GetHeadlessWorldInformationData(
                metadata.Data.Metadata.LatestBlockIndex,
                new Address(address)
            )
        ).WorldDictionary;

        foreach (var dictionary in agentDataFromMimir)
        {
            var mimirKey = dictionary.Key;
            var mimirValue = dictionary.Value;

            if (agentDataFromHeadless.TryGetValue(mimirKey, out var headlessValue))
            {
                // FIXME mimir에서 가져오는 id 값 부정확.
                // Assert.Equal(mimirValue.Id, headlessValue.Id);
                Assert.Equal(mimirValue.Name, headlessValue.Name);
                Assert.Equal(mimirValue.IsUnlocked, headlessValue.IsUnlocked);
                Assert.Equal(mimirValue.StageBegin, headlessValue.StageBegin);
                Assert.Equal(mimirValue.StageEnd, headlessValue.StageEnd);
                Assert.Equal(mimirValue.IsStageCleared, headlessValue.IsStageCleared);
                Assert.Equal(mimirValue.StageClearedId, headlessValue.StageClearedId);
                Assert.Equal(mimirValue.UnlockedBlockIndex, headlessValue.UnlockedBlockIndex);
                Assert.Equal(
                    mimirValue.StageClearedBlockIndex,
                    headlessValue.StageClearedBlockIndex
                );
            }
            else
            {
                Assert.Fail();
            }
        }
    }
}
