using AdoExport.Api.Configuration;
using AdoExport.Api.Services.Caching;
using AdoExport.Auditing;

using Microsoft.Extensions.Options;

namespace AdoExport.Api.Tests.Services.Caching;

[TestFixture]
public static class AuditCacheTests
{
    private static IEnumerable<TestCaseData> InvalidKeysTestCases()
    {
        yield return new TestCaseData("");
        yield return new TestCaseData(" ");
    }

    [TestFixture]
    public class SetAsync
    {
        [Test]
        public void ThrowsWhenKeyIsNull()
        {
            var sut = CreateFixture().Create<AuditCache>();

            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await sut.SetAsync(null!, CreateAudit());
            });
        }

        [TestCaseSource(typeof(AuditCacheTests), nameof(InvalidKeysTestCases))]
        public void ThrowsWhenKeyIsInvalid(string key)
        {
            var sut = CreateFixture().Create<AuditCache>();

            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await sut.SetAsync(key, CreateAudit());
            });
        }

        [Test]
        public void ThrowsWhenDataIsNull()
        {
            var sut = CreateFixture().Create<AuditCache>();

            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await sut.SetAsync("my-key", null!);
            });
        }

        [Test]
        public async Task SetsExpectedData()
        {
            var fixture = CreateFixture();

            var cachingServiceMock = fixture.Freeze<Mock<IInMemoryCachingService>>();

            var cacheConfiguration = CreateCacheConfiguration();
            fixture.Inject(cacheConfiguration);

            var key = fixture.Create<string>();
            var data = CreateAudit();

            var sut = fixture.Create<AuditCache>();

            await sut.SetAsync(key, data);

            cachingServiceMock
                .Verify(m =>
                        m.SetAsync(
                            It.Is<string>(v => v.Contains(key)),
                            data,
                            cacheConfiguration.Value.CacheExpiry),
                    Times.Once);
        }
    }

    [TestFixture]
    public class GetAsync
    {
        [Test]
        public void ThrowsWhenKeyIsNull()
        {
            var sut = CreateFixture().Create<AuditCache>();

            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await sut.GetAsync(null!);
            });
        }

        [TestCaseSource(typeof(AuditCacheTests), nameof(InvalidKeysTestCases))]
        public void ThrowsWhenKeyIsInvalid(string key)
        {
            var sut = CreateFixture().Create<AuditCache>();

            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await sut.GetAsync(key);
            });
        }

        [Test]
        public async Task GetsNullData()
        {
            var fixture = CreateFixture();

            var key = fixture.Create<string>();

            var cachingServiceMock = fixture.Freeze<Mock<IInMemoryCachingService>>();
            cachingServiceMock
                .Setup(m => m.GetAsync<Audit?>(It.Is<string>(v => v.Contains(key))))
                .ReturnsAsync((Audit?)null);

            fixture.Inject(CreateCacheConfiguration());
            
            var sut = fixture.Create<AuditCache>();

            (await sut.GetAsync(key))
                .Should()
                .BeNull();
        }

        [Test]
        public async Task GetsExpectedData()
        {
            var fixture = CreateFixture();

            var key = fixture.Create<string>();
            var data = CreateAudit();

            var cachingServiceMock = fixture.Freeze<Mock<IInMemoryCachingService>>();
            cachingServiceMock
                .Setup(m => m.GetAsync<Audit?>(It.Is<string>(v => v.Contains(key))))
                .ReturnsAsync(data);

            fixture.Inject(CreateCacheConfiguration());
            
            var sut = fixture.Create<AuditCache>();

            (await sut.GetAsync(key))
                .Should()
                .BeSameAs(data);
        }
    }

    private static Audit CreateAudit()
    {
        return new Audit(new AuditMetadata()
        {
            AuditId = Guid.NewGuid(),
            ExpiresAfter = TimeSpan.FromMinutes(30),
            OrganizationName = "my-organization",
        })
        {
            UserAuditResults = [],
            PatAuditResults = [],
            SshKeysAuditResults = [],
        };
    }

    private static IOptions<CacheConfiguration> CreateCacheConfiguration()
    {
        return new OptionsWrapper<CacheConfiguration>(new CacheConfiguration
        {
            CacheExpiry = TimeSpan.FromMinutes(30),
            CacheType = CacheTypes.Memory,
        });
    }

    private static IFixture CreateFixture()
    {
        return new Fixture()
            .Customize(new AutoMoqCustomization());
    }
}