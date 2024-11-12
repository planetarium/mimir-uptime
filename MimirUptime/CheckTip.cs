using HeadlessGQL;
using Lib9c;
using Microsoft.Extensions.DependencyInjection;
using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using MimirGQL;

namespace MimirUptime;

public class CheckTip : IClassFixture<GraphQLClientFixture>
{
    private readonly IMimirClient mimirClient;
    private readonly IHeadlessClient headlessClient;
    private List<string> excludeCollectionNames = new List<string>();

    public CheckTip(GraphQLClientFixture fixture)
    {
        mimirClient = fixture.ServiceProvider.GetRequiredService<IMimirClient>();
        headlessClient = fixture.ServiceProvider.GetRequiredService<IHeadlessClient>();

        excludeCollectionNames.Add(CollectionNames.GetCollectionName<InventoryDocument>());
        excludeCollectionNames.Add(CollectionNames.GetCollectionName<MetadataDocument>());
        excludeCollectionNames.Add(
            CollectionNames.GetCollectionName(
                CollectionNames.GetAccountAddress(Currencies.DailyRewardRune)
            )
        );
    }

    [Fact]
    public async Task CheckGapMimirAndHeadless()
    {
        var blockIndexFromHeadless = await GetHeadlessBlockIndex();

        foreach (
            var collectionName in CollectionNames.CollectionAndStateTypeMappings.Values.Concat(
                CollectionNames.CollectionAndAddressMappings.Values
            )
        )
        {
            if (excludeCollectionNames.Contains(collectionName))
            {
                continue;
            }

            var blockIndexFromMimir = await GetMimirMetadata(collectionName);

            Assert.True(blockIndexFromMimir + 10 >= blockIndexFromHeadless);
        }
    }

    public async Task<long> GetMimirMetadata(string collectionName)
    {
        var metadataResponse = await mimirClient.GetMetadata.ExecuteAsync(collectionName);
        var latestBlockIndex = metadataResponse.Data.Metadata.LatestBlockIndex;

        return latestBlockIndex;
    }

    public async Task<long> GetHeadlessBlockIndex()
    {
        var tipResponse = await headlessClient.GetTip.ExecuteAsync();
        return tipResponse.Data.NodeStatus.Tip.Index;
    }
}
