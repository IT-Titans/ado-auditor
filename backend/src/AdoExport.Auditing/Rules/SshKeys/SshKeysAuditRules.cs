using AdoExport.Client.AdoApi;
using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Rules.SshKeys;

public class SshKeysAuditRules : AuditRules<SessionTokenDto>
{
    public SshKeysAuditRules(RawAuditData rawAuditData)
    {
        Rules =
        [
            new SshKeyExpiryAuditRule(rawAuditData),
            new SshKeyValidForAllOrganizationsAuditRule(rawAuditData),
        ];
    }
}