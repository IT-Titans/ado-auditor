using AdoExport.Api.Controllers.Transformers;
using AdoExport.Api.Services;
using AdoExport.Auditing;
using AdoExport.Auditing.Rules;
using AdoExport.Auditing.Rules.Pats.Reasons;
using AdoExport.Auditing.Rules.SshKeys.Reasons;
using AdoExport.Auditing.Rules.Users.Reasons;
using AdoExport.Client.AdoApi.Models;

using AutoMapper;

namespace AdoExport.Api.Tests;

public class AuditControllerTransformerTests
{
    private Mapper _mapper;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        var autoMapperConfiguration = new MapperConfiguration(cfg => { cfg.AddMaps(typeof(Program)); });
        _mapper = new Mapper(autoMapperConfiguration);
    }

    [Test]
    public void CountsTotalIssues()
    {
        var auditResult = CreateSampleAudit();

        var sut = new AuditControllerTransformer(new AuditSummarizerService(new NotesOnRulesService()), _mapper);

        var auditResultDto = sut.TransformAuditToAuditResultDto(auditResult);

        auditResultDto
            .TotalIssuesCount
            .Should()
            .Be(9);
        auditResultDto
            .CriticalIssuesCount
            .Should()
            .Be(3);
        auditResultDto
            .WarningIssuesCount
            .Should()
            .Be(3);
        auditResultDto
            .OkIssuesCount
            .Should()
            .Be(3);
    }

    [Test]
    public void GroupsByCategories()
    {
        var auditResult = CreateSampleAudit();

        var sut = new AuditControllerTransformer(new AuditSummarizerService(new NotesOnRulesService()), _mapper);

        var auditResultDto = sut.TransformAuditToAuditResultDto(auditResult);
        auditResultDto
            .Summaries
            .Count(summary => summary.Category == Api.Controllers.Models.Categories.Pats)
            .Should()
            .Be(1);

        auditResultDto
            .Summaries
            .Count(summary => summary.Category == Api.Controllers.Models.Categories.Users)
            .Should()
            .Be(1);

        auditResultDto
            .Summaries
            .Count(summary => summary.Category == Api.Controllers.Models.Categories.Projects)
            .Should()
            .Be(0);

        auditResultDto
            .Summaries
            .Count(summary => summary.Category == Api.Controllers.Models.Categories.Groups)
            .Should()
            .Be(0);
    }

    private static Audit CreateSampleAudit()
    {
        var createAuditData = new CreateAuditData()
        {
            Metadata = new AuditMetadata()
            {
                AuditId = Guid.NewGuid(),
                OrganizationName = "ITTitans",
                ExpiresAfter = TimeSpan.FromMinutes(30),
            }
        };

        var auditResult = new Audit(createAuditData.Metadata)
        {
            PatAuditResults =
            [
                CreateAuditResult(
                    CreatePatModel(),
                    auditee =>
                    [
                        new PatExpiresInOverOneQuarterReason(auditee),
                        new PatHasAppScopeReason(auditee),
                        new PatExpiresInOverHalfAYearReason(auditee),
                    ]),
                CreateAuditResult(
                    CreatePatModel(),
                    auditee =>
                    [
                        new PatExpiresWithinShortTimeRangeReason(auditee),
                    ]),
            ],
            SshKeysAuditResults =
            [
                CreateAuditResult(
                    CreateSshKeyModel(),
                    auditee =>
                    [
                        new SshKeyExpiresWithinShortTimeRangeReason(auditee),
                    ]),
                CreateAuditResult(
                    CreateSshKeyModel(),
                    auditee =>
                    [
                        new SshKeyIsValidForAllOrganizationsReason(auditee),
                        new SshKeyExpiresWithinShortTimeRangeReason(auditee),
                    ]),
            ],
            UserAuditResults =
            [
                CreateAuditResult(
                    CreateUserModel(),
                    auditee =>
                    [
                        new UserExceedsOverallPatLimitReason(auditee),
                        new UserIsOrganizationAdminReason(auditee),
                    ]),
                CreateAuditResult(
                    CreateUserModel(),
                    auditee =>
                    [
                        new UserIsNotOrganizationAdminReason(auditee),
                        new UserIsOrganizationAdminReason(auditee),
                    ]),
            ],
        };

        return auditResult;
    }

    private static SessionTokenDto CreatePatModel() => new SessionTokenDto();

    private static SessionTokenDto CreateSshKeyModel() => new SessionTokenDto();

    private static UserDto CreateUserModel() => new UserDto();

    private static AuditResult<TAuditee> CreateAuditResult<TAuditee>(TAuditee auditee,
        Func<TAuditee, AuditRuleReason[]> withReasons)
    {
        return new AuditResult<TAuditee>()
        {
            RuleResults = withReasons(auditee).Select(reason => new RuleResult()
            {
                Reason = reason,
            }).ToArray(),
            Auditee = auditee,
        };
    }
}