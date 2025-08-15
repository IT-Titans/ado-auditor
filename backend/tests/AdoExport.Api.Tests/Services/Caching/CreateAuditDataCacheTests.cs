using AdoExport.Api.Configuration;
using AdoExport.Api.Services.Caching;
using AdoExport.Auditing;

using Microsoft.Extensions.Options;

namespace AdoExport.Api.Tests.Services.Caching;

[TestFixture]
public static class CreateAuditDataCacheTests
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
            var fixture = CreateFixture();

            var sut = fixture.Create<CreateAuditDataCache>();

            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await sut.SetAsync(null!, fixture.Create<CreateAuditData>());
            });
        }

        [TestCaseSource(typeof(CreateAuditDataCacheTests), nameof(InvalidKeysTestCases))]
        public void ThrowsWhenKeyIsInvalid(string key)
        {
            var fixture = CreateFixture();
            
            var data = fixture.Create<CreateAuditData>();

            var sut = fixture.Create<CreateAuditDataCache>();

            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await sut.SetAsync(key, data);
            });
        }

        [Test]
        public void ThrowsWhenDataIsNull()
        {
            var fixture = CreateFixture();

            var sut = fixture.Create<CreateAuditDataCache>();

            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await sut.SetAsync(fixture.Create<string>(), null!);
            });
        }

        [Test]
        public async Task SetsExpectedData()
        {
            var fixture = CreateFixture();

            var cachingServiceMock = fixture.Freeze<Mock<ICachingService>>();

            var cacheConfiguration = CreateCacheConfiguration();
            fixture.Inject(cacheConfiguration);

            var key = fixture.Create<string>();
            var data = fixture.Create<CreateAuditData>();

            var sut = fixture.Create<CreateAuditDataCache>();

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
            var sut = CreateFixture().Create<CreateAuditDataCache>();
    
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await sut.GetAsync(null!);
            });
        }
    
        [TestCaseSource(typeof(CreateAuditDataCacheTests), nameof(InvalidKeysTestCases))]
        public void ThrowsWhenKeyIsInvalid(string key)
        {
            var sut = CreateFixture().Create<CreateAuditDataCache>();
    
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
    
            var cachingServiceMock = fixture.Freeze<Mock<ICachingService>>();
            cachingServiceMock
                .Setup(m => m.GetAsync<CreateAuditData?>(It.IsAny<string>()))
                .ReturnsAsync((CreateAuditData?)null);
    
            var cacheConfiguration = CreateCacheConfiguration();
            fixture.Inject(cacheConfiguration);

            var sut = fixture.Create<CreateAuditDataCache>();
    
            (await sut.GetAsync(key))
                .Should()
                .BeNull();
        }
    
        [Test]
        public async Task GetsExpectedData()
        {var fixture = CreateFixture();
            
            var key = fixture.Create<string>();
            var data = fixture.Create<CreateAuditData>();
    
            var cachingServiceMock = fixture.Freeze<Mock<ICachingService>>();
            cachingServiceMock
                .Setup(m => m.GetAsync<CreateAuditData?>(It.Is<string>(v => v.Contains(key))))
                .ReturnsAsync(data);
    
            var cacheConfiguration = CreateCacheConfiguration();
            fixture.Inject(cacheConfiguration);

            var sut = fixture.Create<CreateAuditDataCache>();
    
            (await sut.GetAsync(key))
                .Should()
                .BeSameAs(data);
        }
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