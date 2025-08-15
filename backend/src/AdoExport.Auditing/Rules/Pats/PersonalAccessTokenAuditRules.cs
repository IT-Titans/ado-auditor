using AdoExport.Client.AdoApi;
using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Auditing.Rules.Pats;

public class PersonalAccessTokenAuditRules : AuditRules<SessionTokenDto>
{
    public PersonalAccessTokenAuditRules(RawAuditData rawAuditData)
    {
        Rules =
        [
            new PersonalAccessTokenExpiryAuditRule(rawAuditData),
            new PersonalAccessTokenScopesAuditRule(rawAuditData),
            new PersonalAccessTokenValidForAllOrganizationsAuditRule(rawAuditData),
        ];
    }

}