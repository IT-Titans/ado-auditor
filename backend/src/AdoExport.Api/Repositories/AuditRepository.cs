using AdoExport.Api.Errors;
using AdoExport.Api.Services.Caching;
using AdoExport.Auditing;

using FluentResults;

namespace AdoExport.Api.Repositories;

public class AuditRepository : IAuditRepository
{
    private readonly ILogger<AuditRepository> _logger;

    private readonly IAuditCache _auditCache;

    private readonly IAuditor _auditor;

    private readonly ICreateAuditDataCache _createAuditDataCache;
    
    public AuditRepository(
        ILogger<AuditRepository> logger,
        IAuditCache auditCache,
        IAuditor auditor,
        ICreateAuditDataCache createAuditDataCache)
    {
        Guard.IsNotNull(logger);
        Guard.IsNotNull(auditCache);
        Guard.IsNotNull(auditor);
        Guard.IsNotNull(createAuditDataCache);

        _logger = logger;
        _auditCache = auditCache;
        _auditor = auditor;
        _createAuditDataCache = createAuditDataCache;
    }

    /// <inheritdoc />
    public Task<Result<Audit>> GetAuditByIdAsync(Guid auditId)
    {
        return GetAuditByIdAsync(auditId.ToString());
    }

    /// <inheritdoc />
    public async Task<Result<Audit>> GetAuditByIdAsync(string auditKey)
    {
        var cachedAudit = await _auditCache.GetAsync(auditKey).ConfigureAwait(false);
        if (cachedAudit is not null)
        {
            return Result.Ok(cachedAudit);
        }

        _logger.LogInformation("Audit for key '{auditId}' not found in cache.", auditKey);

        var cachedData = await _createAuditDataCache.GetAsync(auditKey).ConfigureAwait(false);
        if (cachedData is null)
        {
            _logger.LogInformation("Raw audit data for key '{auditId}' not found in cache.", auditKey);

            return Result.Fail(new AuditExpiredError(auditKey));
        }

        var auditResult = await _auditor.AuditOrganizationAsync(cachedData).ConfigureAwait(false);
        if (auditResult.IsFailed)
        {
            return auditResult.ToResult();
        }

        await _auditCache.SetAsync(auditKey, auditResult.Value).ConfigureAwait(false);

        return auditResult.Value;
    }
}