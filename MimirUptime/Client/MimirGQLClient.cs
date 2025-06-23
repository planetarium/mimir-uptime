using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using MimirUptime.Client.Models;
using Polly;
using Serilog;

namespace MimirUptime.Client
{
    public class MimirGQLClient
    {
        private readonly GraphQLHttpClient _client;

        public MimirGQLClient(string url)
        {
            _client = new GraphQLHttpClient(url, new NewtonsoftJsonSerializer());
        }

        public async Task<T> Request<T>(GraphQLRequest request, CancellationToken token)
        {
            var response = await Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(5,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        Log.Warning(
                            "Request failed with {Message}. Retrying in {TimeSpan}. {RetryCount} / 5",
                            exception.Message, timeSpan, retryCount);
                    })
                .ExecuteAsync(async () => await _client.SendQueryAsync<T>(request, token));

            if (response.Errors != null && response.Errors.Any())
            {
                var errors = string.Join(", ", response.Errors.Select(e => e.Message));
                throw new Exception($"GQL error: {errors}");
            }

            if (response.Data == null)
            {
                throw new Exception("GQL error: response data is null.");
            }

            return response.Data;
        }

        public async Task<ActionPoint> GetActionPoint(string avatarAddress, CancellationToken token)
        {
            var request = new GraphQLRequest
            {
                Query = Queries.ActionPointQuery,
                OperationName = "actionPoint",
                Variables = new { avatarAddress }
            };

            var response = await Request<ActionPointResponse>(request, token);
            return response.ActionPoint;
        }

        public async Task<Agent> GetAgent(string agentAddress, CancellationToken token)
        {
            var request = new GraphQLRequest
            {
                Query = Queries.AgentQuery,
                OperationName = "agent",
                Variables = new { agentAddress }
            };

            var response = await Request<AgentResponse>(request, token);
            return response.Agent;
        }

        public async Task<Avatar> GetAvatar(string avatarAddress, CancellationToken token)
        {
            var request = new GraphQLRequest
            {
                Query = Queries.AvatarQuery,
                OperationName = "avatar",
                Variables = new { avatarAddress }
            };

            var response = await Request<AvatarResponse>(request, token);
            return response.Avatar;
        }

        public async Task<List<Avatar>> GetAvatars(string agentAddress, CancellationToken token)
        {
            var request = new GraphQLRequest
            {
                Query = Queries.AvatarsQuery,
                OperationName = "avatars",
                Variables = new { agentAddress }
            };

            var response = await Request<AvatarsResponse>(request, token);
            return response.Avatars;
        }

        public async Task<DailyRewardState> GetDailyRewardState(string agentAddress, CancellationToken token)
        {
            var request = new GraphQLRequest
            {
                Query = Queries.DailyRewardStateQuery,
                OperationName = "dailyRewardState",
                Variables = new { agentAddress }
            };
            
            var response = await Request<DailyRewardStateResponse>(request, token);
            return response.DailyRewardState;
        }

        public async Task<Inventory> GetInventory(string agentAddress, CancellationToken token)
        {
            var request = new GraphQLRequest
            {
                Query = Queries.InventoryQuery,
                OperationName = "inventory",
                Variables = new { agentAddress }
            };
            
            var response = await Request<InventoryResponse>(request, token);
            return response.Inventory;
        }
        
        public async Task<StakeInfo> GetStakeState(string staker, CancellationToken token)
        {
            var request = new GraphQLRequest
            {
                Query = Queries.StakeStateQuery,
                OperationName = "stakeState",
                Variables = new { staker }
            };
            
            var response = await Request<StakeStateResponse>(request, token);
            return response.StakeState;
        }
        
        public async Task<WorldInformation> GetWorldInformation(CancellationToken token)
        {
            var request = new GraphQLRequest
            {
                Query = Queries.WorldInformationQuery,
                OperationName = "worldInformation"
            };
            
            var response = await Request<WorldInformationResponse>(request, token);
            return response.WorldInformation;
        }
        
        public async Task<long> GetNextArenaTicketChargeOffset(CancellationToken token)
        {
            var request = new GraphQLRequest
            {
                Query = Queries.NextArenaTicketChargeOffsetQuery,
                OperationName = "nextArenaTicketChargeOffset"
            };
            
            var response = await Request<NextArenaTicketChargeOffsetResponse>(request, token);
            return response.NextArenaTicketChargeOffset;
        }
        
        public async Task<ArenaState> GetArenaState(string avatarAddress, CancellationToken token)
        {
            var request = new GraphQLRequest
            {
                Query = Queries.ArenaStateQuery,
                OperationName = "arenaState",
                Variables = new { avatarAddress }
            };
            
            var response = await Request<ArenaStateResponse>(request, token);
            return response.ArenaState;
        }
    }

    public class ActionPointResponse
    {
        public ActionPoint ActionPoint { get; set; }
    }

    public class AgentResponse
    {
        public Agent Agent { get; set; }
    }

    public class AvatarResponse
    {
        public Avatar Avatar { get; set; }
    }

    public class AvatarsResponse
    {
        public List<Avatar> Avatars { get; set; }
    }

    public class DailyRewardStateResponse
    {
        public DailyRewardState DailyRewardState { get; set; }
    }
    
    public class InventoryResponse
    {
        public Inventory Inventory { get; set; }
    }
    
    public class StakeStateResponse
    {
        public StakeInfo StakeState { get; set; }
    }
    
    public class WorldInformationResponse
    {
        public WorldInformation WorldInformation { get; set; }
    }

    public class NextArenaTicketChargeOffsetResponse
    {
        public long NextArenaTicketChargeOffset { get; set; }
    }
    
    public class ArenaStateResponse
    {
        public ArenaState ArenaState { get; set; }
    }
}
