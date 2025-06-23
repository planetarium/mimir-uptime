namespace MimirUptime.Client
{
    public static class Queries
    {
        public const string ChainTipQuery = @"
    query chainTip {
        chainTip {
            blockNumber
            hash
        }
    }";

        public const string NodeStateQuery = @"
    query nodeState {
        nodeState {
            isStaking
            isSyncing
            peers
            publicKey
            txMempool
        }
    }";

        public const string ActionPointQuery = @"
    query actionPoint($avatarAddress: Address!) {
        actionPoint(avatarAddress: $avatarAddress) {
            ap
            maxAp
            chargedAp
            lastChargedApUpdatedBlockIndex
            stageClearCount
            dailyStageClearCount
            maxApRefillWeeklyCount
            apRefillWeeklyCount
        }
    }";

        public const string AgentQuery = @"
    query agent($agentAddress: Address!) {
        agent(agentAddress: $agentAddress) {
            address
            name
        }
    }";

        public const string AvatarQuery = @"
    query avatar($avatarAddress: Address!) {
        avatar(avatarAddress: $avatarAddress) {
            name
            level
            combination
            grade
            core
            costumeAddress
            titleId
        }
    }";

        public const string AvatarsQuery = @"
    query avatars($agentAddress: Address!) {
        avatars(agentAddress: $agentAddress) {
            name
            level
            combination
            grade
            core
            costumeAddress
            titleId
        }
    }";

        public const string DailyRewardStateQuery = @"
    query dailyRewardState($agentAddress: Address!) {
        dailyRewardState(agentAddress: $agentAddress) {
            claimedDays
            claimableDays
        }
    }";

        public const string InventoryQuery = @"
    query inventory($agentAddress: Address!) {
        inventory(agentAddress: $agentAddress) {
            balance(currency: NCG)
            items {
                count
                item {
                    id
                    itemType
                    elementalType
                    name
                }
            }
        }
    }";

        public const string StakeStateQuery = @"
    query stakeState($staker: Address!) {
        stakeState(staker: $staker) {
            id
            staker
            amount
            rewardType
            startedAt
            endsAt
        }
    }";

        public const string WorldInformationQuery = @"
    query worldInformation {
        worldInformation {
            worldId
            worldName
        }
    }";

        public const string NextArenaTicketChargeOffsetQuery = @"
    query nextArenaTicketChargeOffset {
        nextArenaTicketChargeOffset
    }";

        public const string ArenaStateQuery = @"
    query arenaState($avatarAddress: Address!) {
        arenaState(avatarAddress: $avatarAddress) {
            ticket
            ticketChargedAt
            ticketResetCount
            purchasedTicketCount
            winCount
            loseCount
            consecutiveWinCount
            bestConsecutiveWinCount
            rank
            season
            championScore
            revengeTargetAvatarAddress
        }
    }";
    }
}
