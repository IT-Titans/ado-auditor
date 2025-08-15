using AdoExport.Api.Configuration;
using AdoExport.Api.Services;
using AdoExport.Api.Services.Caching;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;

namespace AdoExport.Api.Tests.Services;

public class ScopedUserCredentialsBearerTests
{
    private const string ValidOrganizationName = "MyOrganization";

    private const string ValidPat = "my-secure-pat";

    [Test]
    public void GuardsAgainst_Null_OrganizationName()
    {
        var sut = CreateFixture().Create<ScopedUserCredentialsBearer>();

        Assert.ThrowsAsync<ArgumentNullException>(() => sut.ScopedAsync(null!, ValidPat, () => Task.FromResult(true)));
    }

    [TestCase("")]
    [TestCase(" ")]
    public void GuardsAgainst_Invalid_OrganizationName(string organizationName)
    {
        var sut = CreateFixture().Create<ScopedUserCredentialsBearer>();

        Assert.ThrowsAsync<ArgumentException>(() => sut.ScopedAsync(organizationName, ValidPat, () => Task.FromResult(true)));
    }

    [Test]
    public void GuardsAgainst_Null_Pat()
    {
        var sut = CreateFixture().Create<ScopedUserCredentialsBearer>();

        Assert.ThrowsAsync<ArgumentNullException>(() => sut.ScopedAsync(ValidOrganizationName, null!, () => Task.FromResult(true)));
    }

    [TestCase("")]
    [TestCase(" ")]
    public void GuardsAgainst_Invalid_Pat(string pat)
    {
        var sut = CreateFixture().Create<ScopedUserCredentialsBearer>();

        Assert.ThrowsAsync<ArgumentException>(() => sut.ScopedAsync(ValidOrganizationName, pat, () => Task.FromResult(true)));
    }

    [Test]
    public void GuardsAgainst_Null_ScopedAction()
    {
        var sut = CreateFixture().Create<ScopedUserCredentialsBearer>();

        Assert.ThrowsAsync<ArgumentNullException>(() => sut.ScopedAsync<bool>(ValidOrganizationName, ValidPat, null!));
    }

    [Test]
    public Task ReturnsExpectedResult()
    {
        var fixture = CreateFixture();

        var auditKeyBearer = new ScopedAuditKeyBearer();
        fixture.Inject(auditKeyBearer);

        fixture.Inject(CreateCacheOptions());

        var auditKey = fixture.Create<string>();

        return auditKeyBearer.ScopedAsync(auditKey, async () =>
        {
            var sut = fixture.Create<ScopedUserCredentialsBearer>();

            var expectedResult = fixture.Create<string>();

            var result = await sut.ScopedAsync(ValidOrganizationName, ValidPat, () => Task.FromResult(expectedResult));

            result.Should().Be(expectedResult);
        });
    }

    [Test]
    public Task SetsProtectedCredentialsToCache()
    {
        var fixture = CreateFixture();

        var dataProtectionProvider = new EphemeralDataProtectionProvider();
        fixture.Inject(dataProtectionProvider);

        var auditKeyBearer = new ScopedAuditKeyBearer();
        fixture.Inject(auditKeyBearer);

        var cacheOptions = CreateCacheOptions();
        fixture.Inject(cacheOptions);

        var userDataCacheMock = fixture.Freeze<Mock<IUserDataCache>>();

        var auditKey = fixture.Create<string>();

        return auditKeyBearer.ScopedAsync(auditKey, async () =>
        {
            var protector = dataProtectionProvider.CreateProtector(auditKey).ToTimeLimitedDataProtector();

            var sut = new ScopedUserCredentialsBearer(auditKeyBearer, dataProtectionProvider, userDataCacheMock.Object, cacheOptions);

            _ = await sut.ScopedAsync(ValidOrganizationName, ValidPat, () =>
            {
                userDataCacheMock
                    .Verify(m => m.SetAsync(
                            auditKey,
                            It.Is<UserCredentials>(credentials => string.Equals(credentials.Organization, ValidOrganizationName, StringComparison.Ordinal) &&
                                                                  string.Equals(protector.Unprotect(credentials.Token), ValidPat, StringComparison.Ordinal))),
                        Times.Once);
                return Task.FromResult(true);
            });
        });
    }

    [Test]
    public Task InvalidatesCache_WhenScopeCompleted()
    {
        var fixture = CreateFixture();

        var dataProtectionProvider = new EphemeralDataProtectionProvider();
        fixture.Inject(dataProtectionProvider);

        var auditKeyBearer = new ScopedAuditKeyBearer();
        fixture.Inject(auditKeyBearer);

        var cacheOptions = CreateCacheOptions();
        fixture.Inject(cacheOptions);

        var userDataCacheMock = fixture.Freeze<Mock<IUserDataCache>>();

        var auditKey = fixture.Create<string>();

        return auditKeyBearer.ScopedAsync(auditKey, async () =>
        {
            var sut = new ScopedUserCredentialsBearer(auditKeyBearer, dataProtectionProvider, userDataCacheMock.Object, cacheOptions);

            _ = await sut.ScopedAsync(ValidOrganizationName, ValidPat, () => Task.FromResult(true));

            userDataCacheMock.Verify(m => m.InvalidateAsync(auditKey), Times.Once);
        });
    }

    [Test]
    public Task InvalidatesCache_OnException()
    {
        var fixture = CreateFixture();

        var dataProtectionProvider = new EphemeralDataProtectionProvider();
        fixture.Inject(dataProtectionProvider);

        var auditKeyBearer = new ScopedAuditKeyBearer();
        fixture.Inject(auditKeyBearer);

        var cacheOptions = CreateCacheOptions();
        fixture.Inject(cacheOptions);

        var userDataCacheMock = fixture.Freeze<Mock<IUserDataCache>>();

        var auditKey = fixture.Create<string>();

        return auditKeyBearer.ScopedAsync(auditKey, () =>
        {
            var sut = new ScopedUserCredentialsBearer(auditKeyBearer, dataProtectionProvider, userDataCacheMock.Object, cacheOptions);

            Assert.ThrowsAsync<InvalidOperationException>(() => _ = sut.ScopedAsync<bool>(ValidOrganizationName, ValidPat, () => throw new InvalidOperationException("This is just a test")));

            userDataCacheMock.Verify(m => m.InvalidateAsync(auditKey), Times.Once);

            return Task.CompletedTask;
        });
    }

    private static IOptions<CacheConfiguration> CreateCacheOptions()
    {
        return new OptionsWrapper<CacheConfiguration>(new CacheConfiguration
        {
            CacheExpiry = TimeSpan.FromSeconds(10),
            CacheType = CacheTypes.Memory,
        });
    }

    private static IFixture CreateFixture()
    {
        return new Fixture()
            .Customize(new AutoMoqCustomization());
    }
}