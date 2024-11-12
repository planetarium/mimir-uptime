using HeadlessGQL;
using Lib9c.Models.States;
using Libplanet.Crypto;
using Microsoft.Extensions.DependencyInjection;
using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using MimirGQL;
using Nekoyume;

namespace MimirUptime.CompareTests;

public class AvatarTests : IClassFixture<GraphQLClientFixture>
{
    private readonly IMimirClient mimirClient;
    private readonly IHeadlessClient headlessClient;

    public AvatarTests(GraphQLClientFixture fixture)
    {
        mimirClient = fixture.ServiceProvider.GetRequiredService<IMimirClient>();
        headlessClient = fixture.ServiceProvider.GetRequiredService<IHeadlessClient>();
    }

    [Theory]
    [InlineData("0xD292eA111A72cCB0c95aE9D122fD95b16e1142B8")]
    public async Task CompareAvatarDataFromDifferentServices_ShouldMatch(string address)
    {
        var metadata = await mimirClient.GetMetadata.ExecuteAsync(
            CollectionNames.GetCollectionName<AvatarDocument>()
        );
        var avatarDataFromMimir = await GetMimirAvatarData(new Address(address));
        var avatarDataFromHeadless = await GetHeadlessAvatarData(
            metadata.Data.Metadata.LatestBlockIndex,
            new Address(address)
        );

        Assert.Equal(avatarDataFromMimir.Address, avatarDataFromHeadless.Address.ToString());
        Assert.Equal(avatarDataFromMimir.Name, avatarDataFromHeadless.Name);
        Assert.Equal(avatarDataFromMimir.Level, avatarDataFromHeadless.Level);
        Assert.Equal(
            avatarDataFromMimir.AgentAddress,
            avatarDataFromHeadless.AgentAddress.ToString()
        );
    }

    public async Task<IGetAvatar_Avatar> GetMimirAvatarData(Address address)
    {
        var avatarResponse = await mimirClient.GetAvatar.ExecuteAsync(address.ToString());
        var avatarData = avatarResponse.Data.Avatar;

        return avatarData;
    }

    public async Task<AvatarState> GetHeadlessAvatarData(long blockIndex, Address address)
    {
        var stateResponse = await headlessClient.GetState.ExecuteAsync(
            Addresses.Avatar.ToString(),
            address.ToString(),
            blockIndex
        );
        var result = CodecUtil.DecodeState(stateResponse.Data.State);
        return new AvatarState(result);
    }
}
