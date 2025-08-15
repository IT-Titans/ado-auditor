using AdoExport.Api.Configuration;
using AdoExport.Api.Errors;
using AdoExport.Api.Services.Caching;
using AdoExport.Auditing;
using AdoExport.Client.AdoApi;
using AdoExport.Common.Errors;
using AdoExport.Common.Services;

using FluentResults;

using Microsoft.Extensions.Options;

namespace AdoExport.Api.Services;

public class AuditService : IAuditService
{
    private readonly ILogger<AuditService> _logger;

    private readonly IAuditCache _auditCache;

    private readonly IAuditor _auditor;

    private readonly IRetrieveAuditDataService _retrieveAuditDataService;

    private readonly IUserDataCache _userDataCache;

    private readonly ScopedAuditKeyBearer _auditKeyBearer;

    private readonly ICreateAuditDataCache _createAuditDataCache;

    private readonly IDateTimeService _dateTimeService;

    private readonly IOptions<CacheConfiguration> _cacheOptions;

    public AuditService(
        ILogger<AuditService> logger,
        IAuditCache auditCache,
        IAuditor auditor,
        IRetrieveAuditDataService retrieveAuditDataService,
        IUserDataCache userDataCache,
        ScopedAuditKeyBearer auditKeyBearer,
        ICreateAuditDataCache createAuditDataCache,
        IDateTimeService dateTimeService,
        IOptions<CacheConfiguration> cacheOptions)
    {
        Guard.IsNotNull(logger);
        Guard.IsNotNull(auditCache);
        Guard.IsNotNull(auditor);
        Guard.IsNotNull(retrieveAuditDataService);
        Guard.IsNotNull(userDataCache);
        Guard.IsNotNull(auditKeyBearer);
        Guard.IsNotNull(createAuditDataCache);
        Guard.IsNotNull(dateTimeService);
        Guard.IsNotNull(cacheOptions);

        _logger = logger;
        _auditCache = auditCache;
        _auditor = auditor;
        _retrieveAuditDataService = retrieveAuditDataService;
        _userDataCache = userDataCache;
        _auditKeyBearer = auditKeyBearer;
        _createAuditDataCache = createAuditDataCache;
        _dateTimeService = dateTimeService;
        _cacheOptions = cacheOptions;
    }

    /// <inheritdoc />
    public async Task<Result<Audit>> CreateAuditAsync(Guid auditId, CancellationToken cancellationToken)
    {
        var organizationData = await _userDataCache.GetAsync(_auditKeyBearer.AuditKey).ConfigureAwait(false);
        if (organizationData is null)
        {
            return Result.Fail(new CreateAuditRequestUnauthorizedError(auditId));
        }

        var createAuditData = new CreateAuditData()
        {
            Metadata = new AuditMetadata
            {
                AuditId = auditId,
                OrganizationName = organizationData.Organization,
                ExpiresAfter = _cacheOptions.Value.CacheExpiry,
                RequestedAuditAt = _dateTimeService.UtcNow(),
            }
        };

        createAuditData.Metadata.StartRequestAuditDataAt = _dateTimeService.UtcNow();

        Result<RawAuditData> gatherDataResult;
        try
        {
            gatherDataResult = await _retrieveAuditDataService.GetRawAuditDataAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            // invalidate the credentials as we don't need it anymore
            await _userDataCache.InvalidateAsync(_auditKeyBearer.AuditKey).ConfigureAwait(false);
        }

        if (gatherDataResult.IsFailed)
        {
            return gatherDataResult.ToResult();
        }

        createAuditData.ExportedData = gatherDataResult.Value;
        createAuditData.Metadata.FinishRequestAuditDataAt = _dateTimeService.UtcNow();

        if (cancellationToken.IsCancellationRequested)
        {
            return Result.Fail(new RequestWasCanceledError());
        }

        var auditResult = await _auditor.AuditOrganizationAsync(createAuditData).ConfigureAwait(false);
        if (auditResult.IsFailed)
        {
            return auditResult.ToResult();
        }

        createAuditData.Metadata.CreatedAuditAt = _dateTimeService.UtcNow();

        await _createAuditDataCache.SetAsync(_auditKeyBearer.AuditKey, createAuditData).ConfigureAwait(false);
        await _auditCache.SetAsync(_auditKeyBearer.AuditKey, auditResult.Value).ConfigureAwait(false);

        return auditResult;
    }
}