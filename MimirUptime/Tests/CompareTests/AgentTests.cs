// using HeadlessGQL;
// using Lib9c.Models.States;
// using Libplanet.Crypto;
// using Microsoft.Extensions.DependencyInjection;
// using Mimir.MongoDB;
// using Mimir.MongoDB.Bson;
// using MimirGQL;
// using Nekoyume;

// namespace MimirUptime.CompareTests;

// public class AgentTests : IClassFixture<GraphQLClientFixture>
// {
//     private readonly IMimirClient mimirClient;
//     private readonly IHeadlessClient headlessClient;

//     public AgentTests(GraphQLClientFixture fixture)
//     {
//         mimirClient = fixture.ServiceProvider.GetRequiredService<IMimirClient>();
//         headlessClient = fixture.ServiceProvider.GetRequiredService<IHeadlessClient>();
//     }

//     [Theory]
//     [InlineData("0x6880c5B8EFccce10E955aA5F9aa102eE90c658e8")]
//     public async Task CompareAgentData(string address)
//     {
//         var metadata = await mimirClient.GetMetadata.ExecuteAsync(
//             CollectionNames.GetCollectionName<AgentDocument>()
//         );
//         var agentDataFromMimir = await GetMimirAgentData(new Address(address));
//         var agentDataFromHeadless = await GetHeadlessAgentData(
//             metadata.Data.Metadata.LatestBlockIndex,
//             new Address(address)
//         );

//         Assert.Equal(agentDataFromMimir.Address, agentDataFromHeadless.Address.ToString());
//         Assert.Equal(
//             agentDataFromMimir.MonsterCollectionRound,
//             agentDataFromHeadless.MonsterCollectionRound
//         );

//         var mimirAvatarAddresses = agentDataFromMimir.AvatarAddresses;
//         var headlessAvatarAddresses = agentDataFromHeadless.AvatarAddresses;
//         foreach (var mimirAddress in mimirAvatarAddresses)
//         {
//             if (headlessAvatarAddresses.TryGetValue(mimirAddress.Key, out var headlessValue))
//             {
//                 Assert.Equal(mimirAddress.Value, headlessValue.ToString());
//             }
//             else
//             {
//                 Assert.Fail();
//             }
//         }
//     }

//     public async Task<IGetAgent_Agent> GetMimirAgentData(Address address)
//     {
//         var agentResponse = await mimirClient.GetAgent.ExecuteAsync(address.ToString());
//         var agentData = agentResponse.Data.Agent;

//         return agentData;
//     }

//     public async Task<AgentState> GetHeadlessAgentData(long blockIndex, Address address)
//     {
//         try
//         {
//             var stateResponse = await headlessClient.GetState.ExecuteAsync(
//                 Addresses.Agent.ToString(),
//                 address.ToString(),
//                 blockIndex
//             );
//             var result = CodecUtil.DecodeState(stateResponse.Data.State);
//             return new AgentState(result);
//         }
//         catch (Exception)
//         {
//             Assert.Skip("Headless client is unresponsive; skipping test.");
//             throw;
//         }
//     }
// }
