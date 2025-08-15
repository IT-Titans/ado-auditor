using AdoExport.Auditing.Rules.Pats;
using AdoExport.Auditing.Rules.SshKeys;
using AdoExport.Auditing.Rules.Users;
using AdoExport.Client.AdoApi.Models;
using AdoExport.Common.Services;

using FluentResults;

namespace AdoExport.Auditing;

public class Auditor : IAuditor
{
    private readonly IDateTimeService _dateTimeService;

    public Auditor(IDateTimeService dateTimeService)
    {
        Guard.IsNotNull(dateTimeService);

        _dateTimeService = dateTimeService;
    }

    /// <inheritdoc />
    public Task<Result<Audit>> AuditOrganizationAsync(CreateAuditData createAuditData)
    {
        Guard.IsNotNull(createAuditData);

        createAuditData.Metadata.StartCalculatingAuditAt = _dateTimeService.UtcNow();

        var data = createAuditData.ExportedData!;

        var patAuditRules = new PersonalAccessTokenAuditRules(data);
        var patResults = data
            .PrivateSessionTokenDetailsWithUser
            .Select(tokenDetails => patAuditRules.Audit(tokenDetails.SessionToken))
            .ToArray();

        var sshKeysAuditRules = new SshKeysAuditRules(data);
        var sshKeysResults = data
            .PublicSessionTokenDetailsWithUser
            .Select(tokenDetails => sshKeysAuditRules.Audit(tokenDetails.SessionToken))
            .ToArray();

        var userAuditRules = new UserAuditRules(data);
        var userResults = data
            .AllUsers
            .Where(user => user.IsRealUserType())
            .Select(token => userAuditRules.Audit(token))
            .ToArray();

        createAuditData.Metadata.FinishCalculatingAuditAt = _dateTimeService.UtcNow();

        var audit = new Audit(createAuditData.Metadata)
        {
            PatAuditResults = patResults,
            SshKeysAuditResults = sshKeysResults,
            UserAuditResults = userResults,
        };

        return Task.FromResult(Result.Ok(audit));
    }
}