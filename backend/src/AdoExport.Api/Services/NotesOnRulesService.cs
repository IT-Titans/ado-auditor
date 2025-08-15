using AdoExport.Api.Auditing;
using AdoExport.Api.Controllers;
using AdoExport.Auditing;
using AdoExport.Auditing.Rules.Users;

namespace AdoExport.Api.Services;

public class NotesOnRulesService : INotesOnRulesService
{
    /// <summary>
    /// Todo: This needs to be moved into a configuration that is used in the related rules as well (<see cref="UserPatsAuditRule" />).
    /// </summary>
    private const int NumberOfAllowedPats = 3;

    private const string FollowingBestPracticesDisplayableString = "You're following best practices. Nothing to do here!";

    private readonly Dictionary<Guid, RuleHint> _ruleHints = new Dictionary<Guid, RuleHint>()
    {
        {
            Guid.Parse("05f06a6b-5f3d-4f16-bb63-7e035c962f48"), new RuleHint()
            {
                Name = "PAT expires in over half a year",
                Description = "Personal Access Tokens (PATs) with expiry dates longer than half a year increase the risk of unauthorized access if they are ever leaked or compromised. Since these tokens often provide broad permissions, a long lifespan gives attackers more time to exploit them without detection. Shorter-lived tokens reduce this risk by limiting the duration of access and encouraging regular token rotation, which is a key security best practice.",
                Recommendations = "To improve security in your Azure DevOps Service instance, we recommend setting a shorter expiration period for Personal Access Tokens—ideally 90 days or less. You can do this by navigating to User Settings > Personal Access Tokens, revoking any long-lived tokens, and creating new ones with shorter expiry periods and the minimum required scopes.",
            }
        },
        {
            Guid.Parse("27bd636c-6a92-4c77-9023-d91d3a0dc129"), new RuleHint()
            {
                Name = "PAT expires in over 90 days",
                Description = "Personal Access Tokens (PATs) with expiry dates longer than 90 days increase the risk of unauthorized access if they are ever leaked or compromised. Since these tokens often provide broad permissions, a long lifespan gives attackers more time to exploit them without detection. Shorter-lived tokens reduce this risk by limiting the duration of access and encouraging regular token rotation, which is a key security best practice.",
                Recommendations = "To improve security in your Azure DevOps Service instance, we recommend setting a shorter expiration period for Personal Access Tokens—ideally 90 days or less. You can do this by navigating to User Settings > Personal Access Tokens, revoking any long-lived tokens, and creating new ones with shorter expiry periods and the minimum required scopes.",
            }
        },
        {
            Guid.Parse("1529f5ce-6264-4296-8d54-605e68836317"), new RuleHint()
            {
                Name = "PAT expires in a short time",
                Description = "Using short-lived Personal Access Tokens (PATs) is a best practice because it limits the time a token can be used if it becomes compromised. By reducing the lifespan of tokens, you minimize the window of opportunity for unauthorized access, enforce more frequent access reviews, and encourage regular credential rotation. This approach strengthens your overall security posture by ensuring that access credentials are refreshed more often and less likely to be misused or forgotten.",
                Recommendations = FollowingBestPracticesDisplayableString,
            }
        },
        {
            Guid.Parse("e6a47ea8-34f1-43c5-b50c-df0ebda16cfc"), new RuleHint()
            {
                Name = "PAT is valid for all organizations",
                Description = "Allowing a Personal Access Token (PAT) to access multiple Azure DevOps organizations increases the attack surface and risk of cross-organization compromise. If the token is exposed or misused, it could grant unauthorized access to several environments, potentially affecting different teams, projects, or clients. Isolating PATs to a single organization ensures tighter access control and limits the potential impact of a security incident.",
                Recommendations = "To reduce risk in your Azure DevOps Service instance, create separate Personal Access Tokens for each organization you need to access. You can do this by going to User Settings > Personal Access Tokens within each organization and generating a new token scoped only to that organization. Revoke any existing tokens that span multiple organizations to enforce proper access isolation and minimize potential exposure.",
            }
        },
        {
            Guid.Parse("CF7DCC01-8DB3-41A6-B6B9-A6B856D72C76"), new RuleHint()
            {
                Name = "PAT is valid for more than one organization",
                Description = "Allowing a Personal Access Token (PAT) to access multiple Azure DevOps organizations increases the attack surface and risk of cross-organization compromise. If the token is exposed or misused, it could grant unauthorized access to several environments, potentially affecting different teams, projects, or clients. Isolating PATs to a single organization ensures tighter access control and limits the potential impact of a security incident.",
                Recommendations = "To reduce risk in your Azure DevOps Service instance, create separate Personal Access Tokens for each organization you need to access. You can do this by going to User Settings > Personal Access Tokens within each organization and generating a new token scoped only to that organization. Revoke any existing tokens that span multiple organizations to enforce proper access isolation and minimize potential exposure.",
            }
        },
        {
            Guid.Parse("F7084C80-0444-4525-BF8C-F8C58C07B026"), new RuleHint()
            {
                Name = "PAT is valid for only one organization",
                Description = "Restricting a Personal Access Token (PAT) to a single Azure DevOps organization is a good security practice because it limits the token’s scope and reduces the potential impact if it is ever compromised. This ensures that access is contained within one environment, making it easier to manage, audit, and revoke if necessary. By isolating access in this way, you minimize the risk of unauthorized actions affecting multiple organizations and maintain stronger access control across your Azure DevOps instances.",
                Recommendations = FollowingBestPracticesDisplayableString,
            }
        },
        {
            Guid.Parse("5a5adb71-e8ac-40d3-8956-15e6465be028"), new RuleHint()
            {
                Name = "PAT has app_token scope",
                Description = "Personal Access Tokens (PATs) with the app_token scope grant broad and powerful permissions intended for applications, often beyond what is necessary for typical user tasks. This elevated level of access increases the risk of misuse, especially if the token is leaked or used in insecure environments. Over-privileged tokens violate the principle of least privilege, making them a high-value target for attackers and a significant risk to the security of your Azure DevOps environment.",
                Recommendations = "To reduce risk, avoid using PATs with the app_token scope unless absolutely required. Review your existing tokens under User Settings > Personal Access Tokens, and revoke any that include the app_token scope unnecessarily. Instead, create new tokens with only the minimum scopes required for the specific task or workflow. If you need application-level access, consider using Azure AD service principals or OAuth-based authentication with scoped permissions for more secure and manageable access.",
            }
        },
        {
            Guid.Parse("0b705033-2e32-4ce3-b606-7dceb31aee96"), new RuleHint()
            {
                Name = "PAT has limited scope",
                Description = "Personal Access Tokens (PATs) with the minimum required scopes are a strong security practice because they follow the principle of least privilege—granting only the specific permissions needed for a task and nothing more. This limits the potential damage if a token is ever compromised, reduces the risk of accidental misuse, and makes it easier to audit and manage access. Using minimally scoped tokens helps maintain tighter control over your Azure DevOps environment and supports a more secure and compliant development workflow.",
                Recommendations = FollowingBestPracticesDisplayableString,
            }
        },
        {
            Guid.Parse("cb5666b5-fc99-4994-9ce3-b464bff65cf7"), new RuleHint()
            {
                Name = "SSH key expires in over a year",
                Description = "SSH keys with expiration dates set more than a year in the future pose a long-term security risk. If the key is ever compromised or the associated access is no longer needed, the key could remain active and undetected for an extended period. This extended validity undermines periodic access reviews and increases the risk of unauthorized access to your Azure DevOps environment. Shorter expiration periods help enforce regular key rotation and ensure access remains intentional and up-to-date.",
                Recommendations = "To improve security in your Azure DevOps Service instance, configure SSH keys with shorter expiration periods—preferably no more than 6 to 12 months. You can manage SSH keys by navigating to User Settings > SSH Public Keys, removing keys with long expiration periods, and uploading new ones with a more appropriate lifespan. This encourages periodic review and helps maintain tighter control over who can access your repositories.",
            }
        },
        {
            Guid.Parse("20922ad1-012e-4c6a-8b1d-4c712a819679"), new RuleHint()
            {
                Name = "SSH key expire in over half a year",
            }
        },
        {
            Guid.Parse("de414e9c-ae7e-4498-a12f-68ce12ec4efc"), new RuleHint()
            {
                Name = "SSH key expire in a short time",
            }
        },
        {
            Guid.Parse("3bd706aa-f35d-4c68-8a76-90398a0c5860"), new RuleHint()
            {
                Name = "SSH key is valid for all organizations",
                Description = "An SSH key that is valid across all organizations significantly increases security risks by granting broad access beyond what is necessary. If the key is compromised, it could allow unauthorized access to multiple organizations, potentially exposing sensitive code, pipelines, and resources. Limiting SSH keys to specific organizations helps contain access, reducing the impact of any security breach.",
                Recommendations = "For better security, create and use separate SSH keys for each Azure DevOps organization you need to access. In your Azure DevOps Service instance, manage SSH keys under User Settings > SSH Public Keys, and ensure each key is scoped only to the relevant organization. Revoke any keys that provide blanket access across multiple organizations to enforce stricter access controls and minimize the risk of cross-organization compromise.",
            }
        },
        {
            Guid.Parse("0919cce4-52f0-429e-804d-72227583d3c9"), new RuleHint()
            {
                Name = "SSH key is valid for more than one organization",
                Description = "An SSH key that is valid across all organizations significantly increases security risks by granting broad access beyond what is necessary. If the key is compromised, it could allow unauthorized access to multiple organizations, potentially exposing sensitive code, pipelines, and resources. Limiting SSH keys to specific organizations helps contain access, reducing the impact of any security breach.",
                Recommendations = "For better security, create and use separate SSH keys for each Azure DevOps organization you need to access. In your Azure DevOps Service instance, manage SSH keys under User Settings > SSH Public Keys, and ensure each key is scoped only to the relevant organization. Revoke any keys that provide blanket access across multiple organizations to enforce stricter access controls and minimize the risk of cross-organization compromise.",
            }
        },
        {
            Guid.Parse("41616316-59e1-4198-98de-90247be45073"), new RuleHint()
            {
                Name = "SSH key is valid for only one organization",
                Description = "Limiting an SSH key’s validity to a single Azure DevOps organization is a strong security practice because it confines access to just that specific environment. This reduces the risk of widespread impact if the key is compromised, as it cannot be used to access other organizations. By isolating access in this way, you improve control, simplify auditing, and minimize potential damage, helping to keep your Azure DevOps resources safer.",
                Recommendations = FollowingBestPracticesDisplayableString,
            }
        },
        {
            Guid.Parse("498a578a-110e-4cd6-ba80-ba641171420a"), new RuleHint()
            {
                Name = "User is normal member",
                Description = "Is the standard organization user type. Grants access and permissions typical for standard users. Users with this rule can interact with regular features of the system according to their membership level.",
                Recommendations = FollowingBestPracticesDisplayableString,
            }
        },
        {
            Guid.Parse("459ddf63-91b0-4722-89f7-548f19b36799"), new RuleHint()
            {
                Name = "User is of type guest",
                Description = "Is an external user which get access to the organization. Guest users can typically view public content but cannot perform actions that require authentication, such as editing data or accessing restricted areas.",
                Recommendations = "Use strict role assignment, enforce MFA, apply access reviews, and avoid adding to high-privilege groups.",
            }
        },
        {
            Guid.Parse("962a4349-6dea-4e0f-ac10-8b6ebd8a9be6"), new RuleHint()
            {
                Name = "User is build agent member",
                Description = "Is a non-human user in the organization. Grants permissions to users who are members of the Build Agent group, allowing them to perform tasks related to build automation and deployment processes.",
                Recommendations = "Review and restrict to automated systems only. Remove from human users.",
            }
        },
        {
            Guid.Parse("4b5d1e5f-49f6-4151-a50e-92ce326b79c3"), new RuleHint()
            {
                Name = "User is agent pool member",
                Description = "Is a non-human user in the organization. Grants permissions to users who are members of an agent pool, allowing them to manage and use build agents within that pool for pipeline execution.",
                Recommendations = "Avoid assigning to real users. Reserve for CI/CD systems. Treat with high privilege caution.",
            }
        },
        {
            Guid.Parse("d1ce9fd4-50c1-4899-8a5d-4120acbf3d54"), new RuleHint()
            {
                Name = "User is unknown member type",
                Description = "Users with an unknown member type introduce security uncertainty and should not retain access until their identity source is verified and properly classified.",
                Recommendations = "Review and remove or properly reassign all users with an unknown member type to ensure secure and traceable access",
            }
        },
        {
            Guid.Parse("4B327B8A-8F83-44F7-85BD-E05B98B5FEDD"), new RuleHint()
            {
                Name = $"User has more than {NumberOfAllowedPats} PATs",
                Description = "A user with an excessive number of Personal Access Tokens (PATs), poses a significant security and management risk. Each PAT represents a potential entry point into your Azure DevOps environment, and the more tokens that exist, the harder it becomes to track, audit, and revoke them when necessary. This increases the likelihood of forgotten, misused, or compromised tokens remaining active and unnoticed, violating the principle of least privilege.",
                Recommendations = "Users should regularly review and consolidate their Personal Access Tokens by navigating to User Settings > Personal Access Tokens in Azure DevOps. Revoke any unused or duplicate tokens, and ensure that each active token serves a specific purpose with the minimum required scopes.",
            }
        },
        {
            Guid.Parse("37c18e21-67ed-4abf-9471-73cbcf743138"), new RuleHint()
            {
                Name = $"User has less than {NumberOfAllowedPats} PATs",
                Description = "Having a small and manageable number of Personal Access Tokens (PATs) reduces the likelihood of forgotten, misused, or compromised tokens remaining active and unnoticed, violating the principle of least privilege.",
                Recommendations = FollowingBestPracticesDisplayableString,
            }
        },
        {
            Guid.Parse("d9883edd-8181-4acb-9e13-b44e04d2ca7d"), new RuleHint()
            {
                Name = "User is organization-wide admin",
                Description = "Having too many organization admins is a security risk because admins have full control over settings, permissions, users, and resources. The more individuals with admin privileges, the greater the chance of accidental misconfiguration, unauthorized changes, or insider threats. It also makes it harder to maintain clear accountability. Limiting admin access to only those who absolutely need it helps enforce the principle of least privilege and strengthens the overall security of your organization.",
                Recommendations = "Grant admin privileges to as little users as possible."
            }
        },
        {
            Guid.Parse("14cf0a21-aa67-4a11-8cea-abf0bfaec4b1"), new RuleHint()
            {
                Name = "User is not organization-wide admin",
                Description = "Admins have full control over settings, permissions, users, and resources. Limiting admin access to only those who absolutely need it helps enforce the principle of least privilege and strengthens the overall security of your organization.",
                Recommendations = FollowingBestPracticesDisplayableString,
            }
        },
    };

    /// <inheritdoc />
    public RuleHint GetByRuleReasonId(Guid ruleReasonId)
    {
        if (ruleReasonId == Guid.Empty)
        {
            throw new ArgumentException("rule reason id cannot be empty", nameof(ruleReasonId));
        }

        if (_ruleHints.TryGetValue(ruleReasonId, out var hint) == false)
        {
            throw new ArgumentOutOfRangeException(nameof(ruleReasonId), "rule reason id not found");
        }

        return hint;
    }
}