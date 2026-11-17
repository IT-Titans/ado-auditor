using AdoExport.Api.Repositories;
using AdoExport.Api.Services;
using AdoExport.Api.Services.Caching;
using AdoExport.Client.AdoApi;

using static AdoExport.Api.Tests.Repositories.RepositoryTestData;

namespace AdoExport.Api.Tests.Repositories;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class SessionTokenRepositoryTests
{
    private readonly Mock<ICreateAuditDataCache> _cache = new();

    private readonly ScopedAuditKeyBearer _bearer = new();

    private SessionTokenRepository CreateSut(RawAuditData? raw)
    {
        _cache
            .Setup(cache => cache.GetAsync(AuditKey))
            .ReturnsAsync(raw is null ? null : CreateAuditData(raw));

        return new SessionTokenRepository(_bearer, _cache.Object);
    }

    private Task<T> Run<T>(Func<Task<T>> action) => InScopeAsync(_bearer, action);

    private static RawAuditData TwoUsersWithPatsAndKeys() => RawData(
        privateTokens:
        [
            TokenWithUser(Token("pat-a1", "u-a"), User("aad.A")),
            TokenWithUser(Token("pat-a2", "u-a"), User("aad.A")),
            TokenWithUser(Token("pat-b1", "u-b"), User("aad.B")),
        ],
        publicTokens:
        [
            TokenWithUser(Token("ssh-a1", "u-a"), User("aad.A")),
            TokenWithUser(Token("ssh-b1", "u-b"), User("aad.B")),
        ]);

    [Test]
    public async Task GetPatsByUserDescriptorAsync_ReturnsOnlyPrivateTokensOfThatUser()
    {
        var sut = CreateSut(TwoUsersWithPatsAndKeys());

        var tokens = await Run(() => sut.GetPatsByUserDescriptorAsync("aad.a"));

        tokens.Select(t => t.AuthorizationId).Should().BeEquivalentTo(["pat-a1", "pat-a2"]);
    }

    [Test]
    public async Task GetSshKeysByUserDescriptorAsync_ReturnsOnlyPublicTokensOfThatUser()
    {
        var sut = CreateSut(TwoUsersWithPatsAndKeys());

        var tokens = await Run(() => sut.GetSshKeysByUserDescriptorAsync("AAD.B"));

        tokens.Select(t => t.AuthorizationId).Should().BeEquivalentTo(["ssh-b1"]);
    }

    [Test]
    public async Task GetAllSessionTokensByUserDescriptorAsync_CombinesPatsAndSshKeys()
    {
        var sut = CreateSut(TwoUsersWithPatsAndKeys());

        var tokens = await Run(() => sut.GetAllSessionTokensByUserDescriptorAsync("aad.a"));

        tokens.Select(t => t.AuthorizationId).Should().BeEquivalentTo(["pat-a1", "pat-a2", "ssh-a1"]);
    }

    [Test]
    public async Task GetAllSessionTokensByUserDescriptorAsync_ReturnsEmptyForUserWithoutTokens()
    {
        var sut = CreateSut(TwoUsersWithPatsAndKeys());

        var tokens = await Run(() => sut.GetAllSessionTokensByUserDescriptorAsync("aad.nobody"));

        tokens.Should().BeEmpty();
    }

    [Test]
    public async Task GetAllSessionTokensByUserDescriptorAsync_ReturnsEmptyForEmptyOrganization()
    {
        var sut = CreateSut(RawData());

        var tokens = await Run(() => sut.GetAllSessionTokensByUserDescriptorAsync("aad.a"));

        tokens.Should().BeEmpty();
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("  ")]
    public async Task RejectsBlankDescriptors(string? descriptor)
    {
        var sut = CreateSut(RawData());

        await FluentActions.Awaiting(() => sut.GetPatsByUserDescriptorAsync(descriptor!)).Should().ThrowAsync<ArgumentException>();
        await FluentActions.Awaiting(() => sut.GetSshKeysByUserDescriptorAsync(descriptor!)).Should().ThrowAsync<ArgumentException>();
        await FluentActions.Awaiting(() => sut.GetAllSessionTokensByUserDescriptorAsync(descriptor!)).Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public async Task ThrowsWhenTheAuditDataIsNoLongerCached()
    {
        var sut = CreateSut(raw: null);

        var act = () => Run(() => sut.GetPatsByUserDescriptorAsync("aad.a"));

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Audit data cannot be null.");
    }

    [Test]
    public async Task ThrowsWhenTheCachedAuditDataCarriesNoExport()
    {
        _cache.Setup(cache => cache.GetAsync(AuditKey)).ReturnsAsync(CreateAuditData(null));
        var sut = new SessionTokenRepository(_bearer, _cache.Object);

        var act = () => Run(() => sut.GetSshKeysByUserDescriptorAsync("aad.a"));

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Raw audit data cannot be null.");
    }
}
