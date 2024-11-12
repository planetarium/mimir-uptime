// using HeadlessGQL;
// using Lib9c.Models.Items;
// using Lib9c.Models.States;
// using Libplanet.Crypto;
// using Microsoft.Extensions.DependencyInjection;
// using Mimir.MongoDB;
// using Mimir.MongoDB.Bson;
// using MimirGQL;
// using Nekoyume;

// namespace MimirUptime;

// public class InventoryTests : IClassFixture<GraphQLClientFixture>
// {
//     private readonly IMimirClient mimirClient;
//     private readonly IHeadlessClient headlessClient;

//     public InventoryTests(GraphQLClientFixture fixture)
//     {
//         mimirClient = fixture.ServiceProvider.GetRequiredService<IMimirClient>();
//         headlessClient = fixture.ServiceProvider.GetRequiredService<IHeadlessClient>();
//     }

//     [Theory]
//     [InlineData("0xb2EEa20efA1040EF75cfB8dfA93b9ef5de62e4d9")]
//     public async Task CompareInventoryData(string address)
//     {
//         var metadata = await mimirClient.GetMetadata.ExecuteAsync(
//             CollectionNames.GetCollectionName<InventoryDocument>()
//         );
//         var inventoryDataFromMimir = await GetMimirInventoryData(new Address(address));
//         var inventoryDataFromHeadless = await GetHeadlessInventoryData(
//             metadata.Data.Metadata.LatestBlockIndex,
//             new Address(address)
//         );

//         Assert.Equal(inventoryDataFromMimir.Items.Count, inventoryDataFromHeadless.Items.Count);
//     }

//     public async Task<IGetInventory_Inventory> GetMimirInventoryData(Address address)
//     {
//         var inventoryResponse = await mimirClient.GetInventory.ExecuteAsync(address.ToString());
//         var inventoryData = inventoryResponse.Data.Inventory;

//         return inventoryData;
//     }

//     public async Task<Inventory> GetHeadlessInventoryData(long blockIndex, Address address)
//     {
//         var stateResponse = await headlessClient.GetState.ExecuteAsync(
//             Addresses.Inventory.ToString(),
//             address.ToString(),
//             blockIndex
//         );
//         var result = CodecUtil.DecodeState(stateResponse.Data.State);
//         return new Inventory(result);
//     }
// }
