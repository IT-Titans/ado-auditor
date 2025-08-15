using AdoExport.Api.Configuration;
using AdoExport.Api.Errors;
using AdoExport.Api.Services;
using AdoExport.Api.Services.Caching;
using AdoExport.Auditing;
using AdoExport.Client.AdoApi;
using AdoExport.Client.AdoApi.Models;
using AdoExport.Common.Errors;

using FluentResults;

using Microsoft.Extensions.Options;

namespace AdoExport.Api.Tests.Services;

[TestFixture]
public static class AuditServiceTests
{
    [TestFixture]
    public class CreateAuditAsync
    {
        [Test]
        public Task ReturnsUnauthorizedWhenNoCredentialsCouldBeFound()
        {
            var fixture = CreateFixture();

            var auditId = fixture.Create<Guid>();
            var auditKey = auditId.ToString();

            var auditKeyBearer = new ScopedAuditKeyBearer();
            fixture.Inject(auditKeyBearer);

            return auditKeyBearer.ScopedAsync(auditKey, async () =>
            {
                var userDataCacheMock = fixture.Freeze<Mock<IUserDataCache>>();
                userDataCacheMock
                    .Setup(m => m.GetAsync(auditKeyBearer.AuditKey))
                    .ReturnsAsync((UserCredentials?)null);

                var sut = fixture.Create<AuditService>();

                var result = await sut.CreateAuditAsync(auditId, CancellationToken.None);

                result.IsFailed.Should().BeTrue();
                result.HasError<CreateAuditRequestUnauthorizedError>().Should().BeTrue();
            });
        }

        [Test]
        public Task ReturnsFailedResultWhenRetrievingDataFailed()
        {
            var fixture = CreateFixture();

            var auditId = fixture.Create<Guid>();
            var auditKey = auditId.ToString();

            fixture.Inject(CreateCacheConfiguration());

            var retrieveAuditDataServiceMock = fixture.Freeze<Mock<IRetrieveAuditDataService>>();
            retrieveAuditDataServiceMock
                .Setup(m => m.GetRawAuditDataAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Fail(new ExceptionalError(new Exception())));

            var createAuditDataCacheMock = fixture.Freeze<Mock<ICreateAuditDataCache>>();
            var auditCacheMock = fixture.Freeze<Mock<IAuditCache>>();

            var auditKeyBearer = new ScopedAuditKeyBearer();
            fixture.Inject(auditKeyBearer);

            var userDataCacheMock = fixture.Freeze<Mock<IUserDataCache>>();
            userDataCacheMock
                .Setup(m => m.GetAsync(auditKey))
                .ReturnsAsync(CreateUserCredentials());

            var sut = fixture.Create<AuditService>();

            return auditKeyBearer.ScopedAsync(auditKey, async () =>
            {
                var result = await sut.CreateAuditAsync(auditId, CancellationToken.None);

                result.IsFailed.Should().BeTrue();
                result.HasError<ExceptionalError>().Should().BeTrue();

                userDataCacheMock
                    .Verify(m =>
                            m.InvalidateAsync(It.IsAny<string>()),
                        Times.Once);

                auditCacheMock
                    .Verify(m =>
                            m.SetAsync(It.IsAny<string>(), It.IsAny<Audit>()),
                        Times.Never);

                createAuditDataCacheMock
                    .Verify(m =>
                            m.SetAsync(It.IsAny<string>(), It.IsAny<CreateAuditData>()),
                        Times.Never);
            });
        }

        [Test]
        public Task ReturnsFailedResultWhenRetrievingDataWasCanceled()
        {
            var fixture = CreateFixture();

            var auditId = fixture.Create<Guid>();
            var auditKey = auditId.ToString();

            fixture.Inject(CreateCacheConfiguration());

            var retrieveAuditDataServiceMock = fixture.Freeze<Mock<IRetrieveAuditDataService>>();
            retrieveAuditDataServiceMock
                .Setup(m => m.GetRawAuditDataAsync(It.Is<CancellationToken>(token => token.IsCancellationRequested)))
                .ReturnsAsync(Result.Fail(new RequestWasCanceledError()));

            var createAuditDataCacheMock = fixture.Freeze<Mock<ICreateAuditDataCache>>();
            var auditCacheMock = fixture.Freeze<Mock<IAuditCache>>();

            var auditKeyBearer = new ScopedAuditKeyBearer();
            fixture.Inject(auditKeyBearer);

            var userDataCacheMock = fixture.Freeze<Mock<IUserDataCache>>();
            userDataCacheMock
                .Setup(m => m.GetAsync(auditKey))
                .ReturnsAsync(CreateUserCredentials());

            var sut = fixture.Create<AuditService>();

            return auditKeyBearer.ScopedAsync(auditKey, async () =>
            {
                var result = await sut.CreateAuditAsync(auditId, new CancellationToken(canceled: true));

                result.IsFailed.Should().BeTrue();
                result.RequestWasCanceled().Should().BeTrue();

                userDataCacheMock
                    .Verify(m =>
                            m.InvalidateAsync(It.IsAny<string>()),
                        Times.Once);

                auditCacheMock
                    .Verify(m =>
                            m.SetAsync(It.IsAny<string>(), It.IsAny<Audit>()),
                        Times.Never);

                createAuditDataCacheMock
                    .Verify(m =>
                            m.SetAsync(It.IsAny<string>(), It.IsAny<CreateAuditData>()),
                        Times.Never);
            });
        }

        [Test]
        public Task ReturnsFailedResultWhenAuditingOrganizationFailed()
        {
            var fixture = CreateFixture();

            var auditId = fixture.Create<Guid>();
            var auditKey = auditId.ToString();

            fixture.Inject(CreateCacheConfiguration());

            var retrieveAuditDataServiceMock = fixture.Freeze<Mock<IRetrieveAuditDataService>>();
            retrieveAuditDataServiceMock
                .Setup(m => m.GetRawAuditDataAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Ok(CreateRawAuditData()));

            var auditorMock = fixture.Freeze<Mock<IAuditor>>();
            auditorMock
                .Setup(m => m.AuditOrganizationAsync(It.IsAny<CreateAuditData>()))
                .ReturnsAsync(Result.Fail(new ExceptionalError(new Exception())));

            var createAuditDataCacheMock = fixture.Freeze<Mock<ICreateAuditDataCache>>();
            var auditCacheMock = fixture.Freeze<Mock<IAuditCache>>();

            var auditKeyBearer = new ScopedAuditKeyBearer();
            fixture.Inject(auditKeyBearer);

            var userDataCacheMock = fixture.Freeze<Mock<IUserDataCache>>();
            userDataCacheMock
                .Setup(m => m.GetAsync(auditKey))
                .ReturnsAsync(CreateUserCredentials());

            var sut = fixture.Create<AuditService>();

            return auditKeyBearer.ScopedAsync(auditKey, async () =>
            {
                var result = await sut.CreateAuditAsync(auditId, CancellationToken.None);

                result.IsFailed.Should().BeTrue();
                result.HasError<ExceptionalError>().Should().BeTrue();

                userDataCacheMock
                    .Verify(m =>
                            m.InvalidateAsync(It.IsAny<string>()),
                        Times.Once);

                auditCacheMock
                    .Verify(m =>
                            m.SetAsync(It.IsAny<string>(), It.IsAny<Audit>()),
                        Times.Never);

                createAuditDataCacheMock
                    .Verify(m =>
                            m.SetAsync(It.IsAny<string>(), It.IsAny<CreateAuditData>()),
                        Times.Never);
            });
        }

        [Test]
        public async Task ReturnsFailedResultWhenAuditingOrganizationWasCanceled()
        {
            var fixture = CreateFixture();

            var auditId = fixture.Create<Guid>();
            var auditKey = auditId.ToString();

            fixture.Inject(CreateCacheConfiguration());

            using var cancellationTokenSource = new CancellationTokenSource();

            var retrieveAuditDataServiceMock = fixture.Freeze<Mock<IRetrieveAuditDataService>>();
            retrieveAuditDataServiceMock
                .Setup(m => m.GetRawAuditDataAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Ok(CreateRawAuditData()))
                // ReSharper disable once AccessToDisposedClosure
                .Callback(() => cancellationTokenSource.Cancel());

            var createAuditDataCacheMock = fixture.Freeze<Mock<ICreateAuditDataCache>>();
            var auditCacheMock = fixture.Freeze<Mock<IAuditCache>>();

            var auditKeyBearer = new ScopedAuditKeyBearer();
            fixture.Inject(auditKeyBearer);

            var userDataCacheMock = fixture.Freeze<Mock<IUserDataCache>>();
            userDataCacheMock
                .Setup(m => m.GetAsync(auditKey))
                .ReturnsAsync(CreateUserCredentials());

            var sut = fixture.Create<AuditService>();

            await auditKeyBearer.ScopedAsync(auditKey, async () =>
            {
                var result = await sut.CreateAuditAsync(auditId, cancellationTokenSource.Token);

                result.IsFailed.Should().BeTrue();
                result.RequestWasCanceled().Should().BeTrue();

                userDataCacheMock
                    .Verify(m =>
                            m.InvalidateAsync(It.IsAny<string>()),
                        Times.Once);

                auditCacheMock
                    .Verify(m =>
                            m.SetAsync(It.IsAny<string>(), It.IsAny<Audit>()),
                        Times.Never);

                createAuditDataCacheMock
                    .Verify(m =>
                            m.SetAsync(It.IsAny<string>(), It.IsAny<CreateAuditData>()),
                        Times.Never);
            });
        }

        [Test]
        public Task PersistsAuditDataOnSuccess()
        {
            var fixture = CreateFixture();

            var auditId = fixture.Create<Guid>();
            var auditKey = auditId.ToString();
            var audit = CreateAudit();

            fixture.Inject(CreateCacheConfiguration());

            var retrieveAuditDataServiceMock = fixture.Freeze<Mock<IRetrieveAuditDataService>>();
            retrieveAuditDataServiceMock
                .Setup(m => m.GetRawAuditDataAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Ok(CreateRawAuditData()));

            var auditorMock = fixture.Freeze<Mock<IAuditor>>();
            auditorMock
                .Setup(m => m.AuditOrganizationAsync(It.IsAny<CreateAuditData>()))
                .ReturnsAsync(Result.Ok(audit));

            var createAuditDataCacheMock = fixture.Freeze<Mock<ICreateAuditDataCache>>();
            var auditCacheMock = fixture.Freeze<Mock<IAuditCache>>();

            var auditKeyBearer = new ScopedAuditKeyBearer();
            fixture.Inject(auditKeyBearer);

            var userDataCacheMock = fixture.Freeze<Mock<IUserDataCache>>();
            userDataCacheMock
                .Setup(m => m.GetAsync(auditKey))
                .ReturnsAsync(CreateUserCredentials());

            var sut = fixture.Create<AuditService>();

            return auditKeyBearer.ScopedAsync(auditKey, async () =>
            {
                var result = await sut.CreateAuditAsync(auditId, CancellationToken.None);

                result.IsSuccess.Should().BeTrue();
                result.Value.Should().Be(audit);

                userDataCacheMock
                    .Verify(m =>
                            m.InvalidateAsync(It.IsAny<string>()),
                        Times.Once);

                auditCacheMock
                    .Verify(m =>
                            m.SetAsync(It.IsAny<string>(), It.IsAny<Audit>()),
                        Times.Once);

                createAuditDataCacheMock
                    .Verify(m =>
                            m.SetAsync(It.IsAny<string>(), It.IsAny<CreateAuditData>()),
                        Times.Once);
            });
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

    private static RawAuditData CreateRawAuditData()
    {
        return new RawAuditData()
        {
            AllUsers = [],
            PrivateSessionTokenDetailsWithUser = [],
            PublicSessionTokenDetailsWithUser = [],
            UserGroupMemberships = new Dictionary<string, GroupDto[]>(),
            UsersWithProjectEntitlements = [],
        };
    }

    private static UserCredentials CreateUserCredentials()
    {
        return new UserCredentials("My-Organization", "my-pat");
    }

    private static Audit CreateAudit()
    {
        return new Audit(new AuditMetadata()
        {
            AuditId = Guid.NewGuid(),
            OrganizationName = "my-organization",
            ExpiresAfter = TimeSpan.FromMinutes(30),
        })
        {
            PatAuditResults = [],
            UserAuditResults = [],
            SshKeysAuditResults = [],
        };
    }

    private static IFixture CreateFixture()
    {
        return new Fixture()
            .Customize(new AutoMoqCustomization());
    }
}