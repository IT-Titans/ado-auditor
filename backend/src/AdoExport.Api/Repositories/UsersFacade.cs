using System.Collections.Concurrent;

using AdoExport.Api.Controllers.Models;
using AdoExport.Api.Errors;
using AdoExport.Api.Formatters;
using AdoExport.Api.Services;
using AdoExport.Auditing;
using AdoExport.Client.AdoApi.Models;

using FluentResults;

namespace AdoExport.Api.Repositories;

public class UsersFacade : IUsersFacade
{
    private readonly ILogger<UsersFacade> _logger;

    private readonly IAuditRepository _auditRepository;

    private readonly IUserRepository _userRepository;

    private readonly IRulesRepository _rulesRepository;

    private readonly ScopedAuditKeyBearer _auditKeyBearer;

    public UsersFacade(
        ILogger<UsersFacade> logger,
        IAuditRepository auditRepository,
        IUserRepository userRepository,
        IRulesRepository rulesRepository,
        ScopedAuditKeyBearer auditKeyBearer)
    {
        Guard.IsNotNull(logger);
        Guard.IsNotNull(auditRepository);
        Guard.IsNotNull(userRepository);
        Guard.IsNotNull(rulesRepository);
        Guard.IsNotNull(auditKeyBearer);

        _logger = logger;
        _auditRepository = auditRepository;
        _userRepository = userRepository;
        _rulesRepository = rulesRepository;
        _auditKeyBearer = auditKeyBearer;
    }

    /// <inheritdoc />
    public async Task<Result<GetUserEntries>> GetUserEntriesAsync()
    {
        var auditResult = await _auditRepository.GetAuditByIdAsync(_auditKeyBearer.AuditKey).ConfigureAwait(false);
        if (auditResult.IsFailed)
        {
            return auditResult.ToResult();
        }

        var audit = auditResult.Value;

        var userDtos = await _userRepository.GetAllUsersAsync().ConfigureAwait(false);

        var userEntries = new List<UserEntryDto>();
        foreach (var userDto in userDtos.Where(user => user.IsRealUserType()))
        {
            var userAuditResults = audit.UserAuditResults
                .Where(userResult => string.Equals(userResult.Auditee.Descriptor, userDto.Descriptor, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            var accessLevel = await _userRepository.GetAccessLevelByUserDescriptorAsync(userDto.Descriptor).ConfigureAwait(false);
            if (accessLevel is null)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                {
                    _logger.LogWarning("Could not find an access level for user '{descriptor}'", userDto.Descriptor);
                }

                continue;
            }

            userEntries.Add(new UserEntryDto()
            {
                Id = userDto.Descriptor,
                UserName = userDto.DisplayName,
                IsActive = accessLevel.IsActive(),
                MetaType = userDto.DisplayableMetaType(),
                AffectedUserRulesCounts = new TotalSeverityCountsDto()
                {
                    TotalCriticals = userAuditResults.GetTotalRuleResultsBySeverity(AdoExport.Auditing.Severities.Critical),
                    TotalWarnings = userAuditResults.GetTotalRuleResultsBySeverity(AdoExport.Auditing.Severities.Warning),
                    TotalOks = userAuditResults.GetTotalRuleResultsBySeverity(AdoExport.Auditing.Severities.Ok),
                },
            });
        }

        return Result.Ok(new GetUserEntries()
        {
            Audit = audit,
            UserEntries = userEntries.ToArray(),
        });
    }

    /// <inheritdoc />
    public async Task<Result<Dictionary<string, GetUserById>>> GetAllUsersByIdAsync()
    {
        var userDtos = await _userRepository.GetAllUsersAsync().ConfigureAwait(false);

        var usersById = new ConcurrentDictionary<string, GetUserById>();
        await Parallel.ForEachAsync(userDtos, async (userDto, cancellationToken) =>
        {
            var userByIdResult = await GetUserByIdAsync(userDto.Descriptor).ConfigureAwait(false);
            if (userByIdResult.IsFailed)
            {
                _logger.LogWarning("Failed to get user by id '{userId}'", userDto.Descriptor);
                return;
            }

            if (usersById.TryAdd(userByIdResult.Value.User.Id, userByIdResult.Value) == false)
            {
                _logger.LogWarning("Failed to add user by id result '{userId}'", userDto.Descriptor);
            }
        }).ConfigureAwait(false);

        return usersById.ToDictionary();
    }

    /// <inheritdoc />
    public async Task<Result<GetUserById>> GetUserByIdAsync(string userId)
    {
        var auditResult = await _auditRepository.GetAuditByIdAsync(_auditKeyBearer.AuditKey).ConfigureAwait(false);
        if (auditResult.IsFailed)
        {
            return auditResult.ToResult();
        }

        var audit = auditResult.Value;

        var userDto = await _userRepository.GetUserByDescriptorAsync(userId).ConfigureAwait(false);
        if (userDto is null)
        {
            return Result.Fail(new UserNotFoundError(userId));
        }

        var allPatAuditResults = (audit.PatAuditResults.Concat(audit.SshKeysAuditResults))
            .ToArray();

        var allPatAuditResultsForGivenUser = new List<AuditResult<SessionTokenDto>>();
        foreach (var patAuditResult in allPatAuditResults)
        {
            var patUser = await _userRepository.GetUserByUserIdAsync(patAuditResult.Auditee.UserId).ConfigureAwait(false);
            if (patUser is null || string.Equals(patUser.Descriptor, userDto.Descriptor, StringComparison.OrdinalIgnoreCase) == false)
            {
                continue;
            }

            allPatAuditResultsForGivenUser.Add(patAuditResult);
        }

        var accessLevel = await _userRepository.GetAccessLevelByUserDescriptorAsync(userDto.Descriptor).ConfigureAwait(false);
        if (accessLevel is null)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning("Could not find an access level for user '{descriptor}'", userDto.Descriptor);
            }

            return Result.Fail(new AccessLevelNotFoundError(userDto.Descriptor));
        }

        var allPatDetails = new List<PatDetailsDto>();
        foreach (var pat in allPatAuditResultsForGivenUser)
        {
            var auditee = pat.Auditee;

            var totalAuditResultsForSessionToken = allPatAuditResultsForGivenUser
                .Where(result => result.Auditee == auditee)
                .ToArray();

            allPatDetails.Add(new PatDetailsDto()
            {
                Id = auditee.AuthorizationId,
                User = new PatDetailsUserDto()
                {
                    UserId = userDto.Descriptor,
                    Name = userDto.DisplayName,
                },
                Name = auditee.DisplayName,
                CreatedAt = DateTime.Parse(auditee.ValidFrom).FormatForFrontend(),
                ExpiresIn = DateTime.Parse(auditee.ValidTo).FormatForFrontend(),
                Scopes = auditee.Scope.Split(' '),
                AffectedPatRulesCounts = new TotalSeverityCountsDto()
                {
                    TotalCriticals = totalAuditResultsForSessionToken.GetTotalRuleResultsBySeverity(AdoExport.Auditing.Severities.Critical),
                    TotalWarnings = totalAuditResultsForSessionToken.GetTotalRuleResultsBySeverity(AdoExport.Auditing.Severities.Warning),
                    TotalOks = totalAuditResultsForSessionToken.GetTotalRuleResultsBySeverity(AdoExport.Auditing.Severities.Ok),
                },
            });
        }

        var userAuditResults = audit
            .UserAuditResults
            .Where(userAuditResult => string.Equals(userAuditResult.Auditee.Descriptor, userDto.Descriptor, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        var rules = await _rulesRepository
            .GetRuleDtosForAsync([..userAuditResults.SelectMany(userAuditResult => userAuditResult.RuleResults)])
            .ConfigureAwait(false);

        return Result.Ok(new GetUserById()
        {
            Audit = audit,
            User = new UserDetailsDto()
            {
                Id = userDto.Descriptor,
                UserName = userDto.DisplayName,
                IsActive = accessLevel.IsActive(),
                MetaType = userDto.DisplayableMetaType(),
            },
            PatsSummary = new TotalSeverityCountsDto()
            {
                TotalCriticals = allPatAuditResultsForGivenUser.GetTotalRuleResultsBySeverity(AdoExport.Auditing.Severities.Critical),
                TotalWarnings = allPatAuditResultsForGivenUser.GetTotalRuleResultsBySeverity(AdoExport.Auditing.Severities.Warning),
                TotalOks = allPatAuditResultsForGivenUser.GetTotalRuleResultsBySeverity(AdoExport.Auditing.Severities.Ok),
            },
            Pats = allPatDetails.ToArray(),
            Rules = rules.ToArray(),
        });
    }
}