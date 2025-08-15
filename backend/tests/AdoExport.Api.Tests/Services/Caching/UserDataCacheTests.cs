using AdoExport.Api.Configuration;
using AdoExport.Api.Services;
using AdoExport.Api.Services.Caching;

using Microsoft.Extensions.Options;

namespace AdoExport.Api.Tests.Services.Caching;

[TestFixture]
public static class UserDataCacheTests
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

            var userCredentials = fixture.Create<UserCredentials>();
            
            var sut = fixture.Create<UserDataCache>();

            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await sut.SetAsync(null!, userCredentials);
            });
        }

        [TestCaseSource(typeof(UserDataCacheTests), nameof(InvalidKeysTestCases))]
        public void ThrowsWhenKeyIsInvalid(string key)
        {
            var fixture = CreateFixture();

            var userCredentials = fixture.Create<UserCredentials>();
            
            var sut = fixture.Create<UserDataCache>();

            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await sut.SetAsync(key, userCredentials);
            });
        }

        [Test]
        public void ThrowsWhenDataIsNull()
        {
            var fixture = CreateFixture();

            var key = fixture.Create<string>();
            
            var sut = fixture.Create<UserDataCache>();

            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await sut.SetAsync(key, null!);
            });
        }

        [Test]
        public async Task SetsExpectedData()
        {
            var fixture = CreateFixture();

            var key = fixture.Create<string>();
            var data = CreateUserCredentials();
            
            var cachingServiceMock = fixture.Freeze<Mock<IInMemoryCachingService>>();

            var cacheConfiguration = CreateCacheConfiguration();
            fixture.Inject(cacheConfiguration);

            var sut = fixture.Create<UserDataCache>();

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
            var fixture = CreateFixture();

            var sut = fixture.Create<UserDataCache>();

            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await sut.GetAsync(null!);
            });
        }

        [TestCaseSource(typeof(UserDataCacheTests), nameof(InvalidKeysTestCases))]
        public void ThrowsWhenKeyIsInvalid(string key)
        {
            var fixture = CreateFixture();

            var sut = fixture.Create<UserDataCache>();

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
                .Setup(m => m.GetAsync<UserCredentials?>(It.IsAny<string>()))
                .ReturnsAsync((UserCredentials?)null);

            var cacheConfiguration = CreateCacheConfiguration();
            fixture.Inject(cacheConfiguration);

            var sut = fixture.Create<UserDataCache>();

            (await sut.GetAsync(key))
                .Should()
                .BeNull();
        }

        [Test]
        public async Task GetsExpectedData()
        {
            var fixture = CreateFixture();

            var key = fixture.Create<string>();
            var data = CreateUserCredentials();
            
            var cachingServiceMock = fixture.Freeze<Mock<IInMemoryCachingService>>();
            cachingServiceMock
                .Setup(m => m.GetAsync<UserCredentials?>(It.IsAny<string>()))
                .ReturnsAsync(data);

            var cacheConfiguration = CreateCacheConfiguration();
            fixture.Inject(cacheConfiguration);

            var sut = fixture.Create<UserDataCache>();

            (await sut.GetAsync(key))
                .Should()
                .BeSameAs(data);
        }
    }

    [TestFixture]
    public class InvalidateAsync
    {
        [Test]
        public void ThrowsWhenKeyIsNull()
        {
            var fixture = CreateFixture();

            var sut = fixture.Create<UserDataCache>();

            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await sut.InvalidateAsync(null!);
            });
        }

        [TestCaseSource(typeof(UserDataCacheTests), nameof(InvalidKeysTestCases))]
        public void ThrowsWhenKeyIsInvalid(string key)
        {
            var fixture = CreateFixture();

            var sut = fixture.Create<UserDataCache>();

            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await sut.InvalidateAsync(key);
            });
        }

        [Test]
        public async Task InvalidatesData()
        {
            var fixture = CreateFixture();

            var key = fixture.Create<string>();
            var data = CreateUserCredentials();
            
            var cachingServiceMock = fixture.Freeze<Mock<IInMemoryCachingService>>();
            cachingServiceMock
                .Setup(m => m.GetAsync<UserCredentials?>(It.IsAny<string>()))
                .ReturnsAsync(data);

            var cacheConfiguration = CreateCacheConfiguration();
            fixture.Inject(cacheConfiguration);

            var sut = fixture.Create<UserDataCache>();

            await sut.InvalidateAsync(key);

            cachingServiceMock
                .Verify(m =>
                        m.InvalidateAsync(It.Is<string>(v => v.Contains(key))),
                    Times.Once);
        }
    }

    private static UserCredentials CreateUserCredentials(string? organizationName = null, string? pat = null)
    {
        return new UserCredentials(organizationName!, pat!);
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