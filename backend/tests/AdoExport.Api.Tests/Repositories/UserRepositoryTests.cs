using AdoExport.Api.Repositories;
using AdoExport.Api.Services;
using AdoExport.Api.Services.Caching;
using AdoExport.Client.AdoApi;

using static AdoExport.Api.Tests.Repositories.RepositoryTestData;

namespace AdoExport.Api.Tests.Repositories;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class UserRepositoryTests
{
    private readonly Mock<ICreateAuditDataCache> _cache = new();

    private readonly ScopedAuditKeyBearer _bearer = new();

    private UserRepository CreateSut(RawAuditData? raw = null)
    {
        _cache
            .Setup(cache => cache.GetAsync(AuditKey))
            .ReturnsAsync(raw is null ? null : CreateAuditData(raw));

        return new UserRepository(_bearer, _cache.Object);
    }

    private Task<T> Run<T>(Func<Task<T>> action) => InScopeAsync(_bearer, action);

    [Test]
    public async Task GetUserByDescriptorAsync_MatchesIgnoringCase()
    {
        var user = User("aad.ABC");
        var sut = CreateSut(RawData(allUsers: [User("aad.other"), user]));

        var found = await Run(() => sut.GetUserByDescriptorAsync("AAD.abc"));

        found.Should().BeSameAs(user);
    }

    [Test]
    public async Task GetUserByDescriptorAsync_ReturnsNullForUnknownDescriptor()
    {
        var sut = CreateSut(RawData(allUsers: [User("aad.abc")]));

        var found = await Run(() => sut.GetUserByDescriptorAsync("aad.missing"));

        found.Should().BeNull();
    }

    [Test]
    public async Task GetAllUsersAsync_ReturnsEveryUserIncludingNonRealOnes()
    {
        var users = new[] { User("a"), User("b", metaType: "build") };
        var sut = CreateSut(RawData(allUsers: users));

        var all = await Run(sut.GetAllUsersAsync);

        all.Should().Equal(users);
    }

    [Test]
    public async Task GetAllUsersAsync_ReturnsEmptyWhenOrganizationHasNoUsers()
    {
        var sut = CreateSut(RawData());

        var all = await Run(sut.GetAllUsersAsync);

        all.Should().BeEmpty();
    }

    [Test]
    public async Task GetUserByUserIdAsync_FindsTheOwnerOfAPrivateToken()
    {
        var owner = User("owner");
        var sut = CreateSut(RawData(privateTokens: [TokenWithUser(Token("t1", "uid-1"), owner)]));

        var found = await Run(() => sut.GetUserByUserIdAsync("uid-1"));

        found.Should().BeSameAs(owner);
    }

    [Test]
    public async Task GetUserByUserIdAsync_FindsTheOwnerOfAPublicToken()
    {
        var owner = User("ssh-owner");
        var sut = CreateSut(RawData(
            privateTokens: [TokenWithUser(Token("t1", "uid-1"), User("someone-else"))],
            publicTokens: [TokenWithUser(Token("k1", "uid-2"), owner)]));

        var found = await Run(() => sut.GetUserByUserIdAsync("uid-2"));

        found.Should().BeSameAs(owner);
    }

    [Test]
    public async Task GetUserByUserIdAsync_ComparesIdsCaseSensitivelyAndReturnsNullWhenUnknown()
    {
        var sut = CreateSut(RawData(privateTokens: [TokenWithUser(Token("t1", "UID-1"), User("owner"))]));

        var found = await Run(() => sut.GetUserByUserIdAsync("uid-1"));

        found.Should().BeNull();
    }

    [Test]
    public async Task GetAccessLevelByUserDescriptorAsync_ReturnsTheLevelOfTheMatchingUser()
    {
        var expected = AccessLevel("active");
        var sut = CreateSut(RawData(entitlements:
        [
            Entitlement(User("aad.one"), AccessLevel("disabled")),
            Entitlement(User("AAD.TWO"), expected),
        ]));

        var level = await Run(() => sut.GetAccessLevelByUserDescriptorAsync("aad.two"));

        level.Should().BeSameAs(expected);
    }

    [Test]
    public async Task GetAccessLevelByUserDescriptorAsync_ReturnsNullForUserWithoutEntitlement()
    {
        var sut = CreateSut(RawData(entitlements: [Entitlement(User("aad.one"), AccessLevel("active"))]));

        var level = await Run(() => sut.GetAccessLevelByUserDescriptorAsync("aad.none"));

        level.Should().BeNull();
    }

    [Test]
    public async Task ThrowsWhenTheAuditDataIsNoLongerCached()
    {
        var sut = CreateSut(raw: null);

        var act = () => Run(sut.GetAllUsersAsync);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Audit data cannot be null.");
    }

    [Test]
    public async Task ThrowsWhenTheCachedAuditDataCarriesNoExport()
    {
        _cache.Setup(cache => cache.GetAsync(AuditKey)).ReturnsAsync(CreateAuditData(null));
        var sut = new UserRepository(_bearer, _cache.Object);

        var act = () => Run(sut.GetAllUsersAsync);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Raw audit data cannot be null.");
    }

    [Test]
    public async Task ReadsTheCacheOnlyOncePerInstance()
    {
        var sut = CreateSut(RawData(allUsers: [User("a")]));

        await Run(sut.GetAllUsersAsync);
        await Run(sut.GetAllUsersAsync);
        await Run(() => sut.GetUserByDescriptorAsync("a"));

        _cache.Verify(cache => cache.GetAsync(It.IsAny<string>()), Times.Once);
    }
}
