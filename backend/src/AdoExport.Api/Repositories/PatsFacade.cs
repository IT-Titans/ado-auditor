using System.Collections.Concurrent;

using AdoExport.Api.Controllers.Models;
using AdoExport.Api.Errors;
using AdoExport.Api.Formatters;
using AdoExport.Api.Services;
using AdoExport.Auditing;

using FluentResults;

namespace AdoExport.Api.Repositories;

public class PatsFacade : IPatsFacade
{
    private readonly ILogger<PatsFacade> _logger;

    private readonly IAuditRepository _auditRepository;

    private readonly IUserRepository _userRepository;

    private readonly IRulesRepository _rulesRepository;

    private readonly ScopedAuditKeyBearer _auditKeyBearer;

    public PatsFacade(
        ILogger<PatsFacade> logger,
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
    public async Task<Result<GetAllPats>> GetAllPatsAsync()
    {
        var auditResult = await _auditRepository.GetAuditByIdAsync(_auditKeyBearer.AuditKey).ConfigureAwait(false);
        if (auditResult.IsFailed)
        {
            return auditResult.ToResult();
        }

        var audit = auditResult.Value;

        // todo: contains both pat and ssh key results. Needs to be split once ssh keys have their own workflow
        var auditResults = audit.PatAuditResults
            .Concat(audit.SshKeysAuditResults).ToArray();

        var auditees = auditResults
            .Select(patResult => patResult.Auditee)
            .ToArray();

        var items = new List<PatDetailsDto>();
        foreach (var auditee in auditees)
        {
            var totalAuditeeResults = auditResults
                .Where(result => result.Auditee == auditee)
                .ToArray();

            var user = await _userRepository
                .GetUserByUserIdAsync(auditee.UserId)
                .ConfigureAwait(false);
            PatDetailsUserDto? patUser = user is null
                ? null
                : new PatDetailsUserDto()
                {
                    UserId = user.Descriptor,
                    Name = user.DisplayName,
                };

            items.Add(new PatDetailsDto()
            {
                Id = auditee.AuthorizationId,
                User = patUser,
                Name = auditee.DisplayName,
                CreatedAt = DateTime.Parse(auditee.ValidFrom).FormatForFrontend(),
                ExpiresIn = DateTime.Parse(auditee.ValidTo).FormatForFrontend(),
                Scopes = auditee.Scope.Split(' '),
                AffectedPatRulesCounts = new TotalSeverityCountsDto()
                {
                    TotalCriticals = totalAuditeeResults.GetTotalRuleResultsBySeverity(AdoExport.Auditing.Severities.Critical),
                    TotalWarnings = totalAuditeeResults.GetTotalRuleResultsBySeverity(AdoExport.Auditing.Severities.Warning),
                    TotalOks = totalAuditeeResults.GetTotalRuleResultsBySeverity(AdoExport.Auditing.Severities.Ok),
                },
            });
        }

        return Result.Ok(new GetAllPats()
        {
            Audit = audit,
            Pats = items.ToArray(),
        });
    }

    /// <inheritdoc />
    public async Task<Result<Dictionary<string, GetPatById>>> GetAllPatsByIdAsync()
    {
        var allPatsResult = await GetAllPatsAsync().ConfigureAwait(false);
        if (allPatsResult.IsFailed)
        {
            return allPatsResult.ToResult();
        }

        var patsById = new ConcurrentDictionary<string, GetPatById>();
        await Parallel.ForEachAsync(allPatsResult.Value.Pats, async (patDto, cancellationToken) =>
        {
            var patByIdResult = await GetPatByIdAsync(patDto.Id).ConfigureAwait(false);
            if (patByIdResult.IsFailed)
            {
                _logger.LogWarning("Failed to get pat by id '{userId}'", patDto.Id);
                return;
            }

            if (patsById.TryAdd(patByIdResult.Value.Pat.Id, patByIdResult.Value) == false)
            {
                _logger.LogWarning("Failed to add pat by id result '{userId}'", patDto.Id);
            }
        }).ConfigureAwait(false);

        return patsById.ToDictionary();
    }

    /// <inheritdoc />
    public async Task<Result<GetPatById>> GetPatByIdAsync(string patId)
    {
        Guard.IsNotNullOrWhiteSpace(patId);

        var auditResult = await _auditRepository.GetAuditByIdAsync(_auditKeyBearer.AuditKey).ConfigureAwait(false);
        if (auditResult.IsFailed)
        {
            return auditResult.ToResult();
        }

        var audit = auditResult.Value;

        var allAuditResults = audit.PatAuditResults
            .Concat(audit.SshKeysAuditResults)
            .ToArray();
        var auditResults = allAuditResults
            .Where(patResult => patResult.Auditee.AuthorizationId == patId)
            .ToArray();

        if (auditResults.Length == 0)
        {
            return Result.Fail(new PatNotFoundError(patId));
        }

        var auditee = auditResults.First().Auditee;

        var user = await _userRepository
            .GetUserByUserIdAsync(auditee.UserId)
            .ConfigureAwait(false);
        PatDetailsUserDto? patUser = user is null
            ? null
            : new PatDetailsUserDto()
            {
                UserId = user.Descriptor,
                Name = user.DisplayName,
            };

        var rules = await _rulesRepository
            .GetRuleDtosForAsync([..auditResults], [..allAuditResults])
            .ConfigureAwait(false);

        var totalAuditeeResults = auditResults
            .Where(result => result.Auditee == auditee)
            .ToArray();

        return new GetPatById
        {
            Audit = audit,
            Pat = new PatDetailsDto()
            {
                Id = auditee.AuthorizationId,
                User = patUser,
                Name = auditee.DisplayName,
                CreatedAt = DateTime.Parse(auditee.ValidFrom).FormatForFrontend(),
                ExpiresIn = DateTime.Parse(auditee.ValidTo).FormatForFrontend(),
                Scopes = auditee.Scope.Split(' '),
                AffectedPatRulesCounts = new TotalSeverityCountsDto()
                {
                    TotalCriticals = totalAuditeeResults.GetTotalRuleResultsBySeverity(AdoExport.Auditing.Severities.Critical),
                    TotalWarnings = totalAuditeeResults.GetTotalRuleResultsBySeverity(AdoExport.Auditing.Severities.Warning),
                    TotalOks = totalAuditeeResults.GetTotalRuleResultsBySeverity(AdoExport.Auditing.Severities.Ok),
                },
            },
            Rules = rules.ToArray(),
        };
    }
}