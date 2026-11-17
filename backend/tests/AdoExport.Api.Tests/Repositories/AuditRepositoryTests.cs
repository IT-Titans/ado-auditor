using AdoExport.Api.Errors;
using AdoExport.Api.Repositories;
using AdoExport.Api.Services.Caching;
using AdoExport.Auditing;

using FluentResults;

using Microsoft.Extensions.Logging.Abstractions;

using static AdoExport.Api.Tests.Repositories.RepositoryTestData;

namespace AdoExport.Api.Tests.Repositories;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class AuditRepositoryTests
{
    private readonly Mock<IAuditCache> _auditCache = new();

    private readonly Mock<IAuditor> _auditor = new();

    private readonly Mock<ICreateAuditDataCache> _createAuditDataCache = new();

    private AuditRepository CreateSut()
        => new(NullLogger<AuditRepository>.Instance, _auditCache.Object, _auditor.Object, _createAuditDataCache.Object);

    [Test]
    public async Task ReturnsTheCachedAuditWithoutAuditingAgain()
    {
        var audit = Audit();
        _auditCache.Setup(cache => cache.GetAsync("key")).ReturnsAsync(audit);

        var result = await CreateSut().GetAuditByIdAsync("key");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(audit);
        _createAuditDataCache.Verify(cache => cache.GetAsync(It.IsAny<string>()), Times.Never);
        _auditor.Verify(auditor => auditor.AuditOrganizationAsync(It.IsAny<CreateAuditData>()), Times.Never);
    }

    [Test]
    public async Task FailsWithAuditExpiredWhenNeitherTheAuditNorItsRawDataIsCached()
    {
        var result = await CreateSut().GetAuditByIdAsync("key");

        result.IsFailed.Should().BeTrue();
        var error = result.Errors.Should().ContainSingle().Which.Should().BeOfType<AuditExpiredError>().Subject;
        error.Metadata["auditKey"].Should().Be("key");
        _auditor.Verify(auditor => auditor.AuditOrganizationAsync(It.IsAny<CreateAuditData>()), Times.Never);
    }

    [Test]
    public async Task AuditsTheRawDataAndCachesTheResultWhenOnlyTheRawDataIsCached()
    {
        var data = CreateAuditData(RawData());
        var audit = Audit();
        _createAuditDataCache.Setup(cache => cache.GetAsync("key")).ReturnsAsync(data);
        _auditor.Setup(auditor => auditor.AuditOrganizationAsync(data)).ReturnsAsync(Result.Ok(audit));

        var result = await CreateSut().GetAuditByIdAsync("key");

        result.Value.Should().BeSameAs(audit);
        _auditCache.Verify(cache => cache.SetAsync("key", audit), Times.Once);
    }

    [Test]
    public async Task PropagatesAnAuditorFailureAndDoesNotCacheAnything()
    {
        var data = CreateAuditData(RawData());
        _createAuditDataCache.Setup(cache => cache.GetAsync("key")).ReturnsAsync(data);
        _auditor.Setup(auditor => auditor.AuditOrganizationAsync(data)).ReturnsAsync(Result.Fail("rules exploded"));

        var result = await CreateSut().GetAuditByIdAsync("key");

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Message.Should().Be("rules exploded");
        _auditCache.Verify(cache => cache.SetAsync(It.IsAny<string>(), It.IsAny<Audit>()), Times.Never);
    }

    [Test]
    public async Task GuidOverloadLooksUpTheAuditByTheGuidsStringForm()
    {
        var id = Guid.NewGuid();
        var audit = Audit();
        _auditCache.Setup(cache => cache.GetAsync(id.ToString())).ReturnsAsync(audit);

        var result = await CreateSut().GetAuditByIdAsync(id);

        result.Value.Should().BeSameAs(audit);
    }
}
