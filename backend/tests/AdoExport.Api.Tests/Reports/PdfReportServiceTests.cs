using AdoExport.Api.Reports;
using AdoExport.Api.Reports.DataSources;
using AdoExport.Api.Reports.Models;
using AdoExport.Api.Reports.Templates;
using AdoExport.Api.Services;
using AdoExport.Common.Errors;

using FluentResults;

namespace AdoExport.Api.Tests.Reports;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class PdfReportServiceTests
{
    private ScopedAuditKeyBearer _keyBearer = null!;

    private Mock<IAuditReportDataSource> _dataSource = null!;

    private int _documentsCreated;

    private PdfReportService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _keyBearer = new ScopedAuditKeyBearer();
        _dataSource = new Mock<IAuditReportDataSource>();
        _documentsCreated = 0;

        _sut = new PdfReportService(
            _keyBearer,
            _dataSource.Object,
            model =>
            {
                _documentsCreated++;
                return ReportRenderer.CreateDocument(model);
            });
    }

    [Test]
    public void GuardsAgainstNullDependencies()
    {
        Func<AuditPdfModel, AuditPdfReportDocument> factory = model => ReportRenderer.CreateDocument(model);

        Assert.Throws<ArgumentNullException>(() => new PdfReportService(null!, _dataSource.Object, factory));
        Assert.Throws<ArgumentNullException>(() => new PdfReportService(_keyBearer, null!, factory));
        Assert.Throws<ArgumentNullException>(() => new PdfReportService(_keyBearer, _dataSource.Object, null!));
    }

    [Test]
    public void RejectsAnEmptyAuditId()
    {
        Assert.ThrowsAsync<ArgumentException>(() => _sut.GetAuditReportPdfFileAsync(Guid.Empty, CancellationToken.None));
    }

    [Test]
    public async Task ReturnsAPdfFileNamedAfterTheAuditedOrganization()
    {
        _dataSource
            .Setup(source => source.GetAuditDataAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(ReportTestData.CreateFullModel("Contoso")));

        var result = await _sut.GetAuditReportPdfFileAsync(Guid.NewGuid(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.MimeType.Should().Be("application/pdf");
        result.Value.FileName.Should().StartWith("Security Audit Contoso").And.EndWith(".pdf");
        PdfInspector.LooksLikePdf(result.Value.Bytes).Should().BeTrue();
        PdfInspector.Title(result.Value.Bytes).Should().Be(result.Value.FileName[..^".pdf".Length]);
    }

    [Test]
    public async Task MakesTheAuditKeyAvailableToTheDataSourceOnlyWhileItRuns()
    {
        var auditId = Guid.NewGuid();
        string? keySeenByDataSource = null;
        _dataSource
            .Setup(source => source.GetAuditDataAsync(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                keySeenByDataSource = _keyBearer.AuditKey;
                return Task.FromResult(Result.Ok(ReportTestData.CreateEmptyModel()));
            });

        await _sut.GetAuditReportPdfFileAsync(auditId, CancellationToken.None);

        keySeenByDataSource.Should().Be(auditId.ToString());
        Assert.Throws<InvalidOperationException>(() => _ = _keyBearer.AuditKey);
    }

    [Test]
    public async Task ForwardsTheFailureOfTheDataSourceWithoutBuildingADocument()
    {
        _dataSource
            .Setup(source => source.GetAuditDataAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<AuditPdfModel>("audit not found"));

        var result = await _sut.GetAuditReportPdfFileAsync(Guid.NewGuid(), CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(error => error.Message == "audit not found");
        _documentsCreated.Should().Be(0);
    }

    [Test]
    public async Task StopsWithACancellationErrorWhenTheRequestWasCanceledWhileLoadingData()
    {
        using var cancellation = new CancellationTokenSource();
        _dataSource
            .Setup(source => source.GetAuditDataAsync(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                cancellation.Cancel();
                return Task.FromResult(Result.Ok(ReportTestData.CreateFullModel()));
            });

        var result = await _sut.GetAuditReportPdfFileAsync(Guid.NewGuid(), cancellation.Token);

        result.IsFailed.Should().BeTrue();
        result.HasError<RequestWasCanceledError>().Should().BeTrue();
        _documentsCreated.Should().Be(0);
    }

    [Test]
    public async Task ClearsTheAuditKeyWhenTheDataSourceFails()
    {
        _dataSource
            .Setup(source => source.GetAuditDataAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<AuditPdfModel>("boom"));

        await _sut.GetAuditReportPdfFileAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Throws<InvalidOperationException>(() => _ = _keyBearer.AuditKey);
    }
}
