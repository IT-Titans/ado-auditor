using AdoExport.Api.Auditing;
using AdoExport.Api.Controllers.Models;
using AdoExport.Api.Services;
using AdoExport.Auditing;

using AdoExport.Api.Controllers.Mappings;

namespace AdoExport.Api.Controllers.Transformers;

public class AuditControllerTransformer
{
    private readonly IAuditSummarizerService _auditSummarizer;

    private readonly IAuditMapper _mapper;

    public AuditControllerTransformer(
        IAuditSummarizerService auditSummarizer,
        IAuditMapper mapper)
    {
        Guard.IsNotNull(auditSummarizer);
        _auditSummarizer = auditSummarizer;

        Guard.IsNotNull(mapper);
        _mapper = mapper;
    }

    public AuditResultDto TransformAuditToAuditResultDto(Audit audit)
    {
        var auditSummaries = _auditSummarizer
            .Summarize(audit)
            .ToArray();

        return new AuditResultDto()
        {
            OrganizationName = audit.OrganizationName,
            RequestedAtDateString = audit.RequestedAtFormatted(),
            CreatedAtDateString = audit.CreatedAtFormatted(),
            ValidUntilDateString = audit.ValidUntilFormatted(),
            TotalIssuesCount = auditSummaries.Length,
            CriticalIssuesCount = auditSummaries.Count(summary => summary.Severity == AdoExport.Auditing.Severities.Critical),
            WarningIssuesCount = auditSummaries.Count(summary => summary.Severity == AdoExport.Auditing.Severities.Warning),
            OkIssuesCount = auditSummaries.Count(summary => summary.Severity == AdoExport.Auditing.Severities.Ok),
            Summaries = auditSummaries
                .GroupBy(item => item.Category)
                .OrderByDescending(item => item.Key)
                .Select(group => new AuditSummaryCategoryDto()
                {
                    Category = _mapper.MapCategoryToDto(group.Key),
                    TotalCount = group.Count(),
                    CriticalIssuesCount = group.Count(item => item.Severity == AdoExport.Auditing.Severities.Critical),
                    WarningIssuesCount = group.Count(item => item.Severity == AdoExport.Auditing.Severities.Warning),
                    OkIssuesCount = group.Count(item => item.Severity == AdoExport.Auditing.Severities.Ok),
                    Items = group
                        .OrderBy(item => item.Severity)
                        .Select(_mapper.MapSummaryItemToDto)
                        .ToArray()
                })
                .ToArray()
        };
    }
}