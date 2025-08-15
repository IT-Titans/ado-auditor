using FluentResults;

namespace AdoExport.Client.AdoApi;

public interface IRetrieveAuditDataService
{
    /// <summary>
    /// Gets the raw data that is needed to perform an audit. 
    /// </summary>
    /// <param name="cancellationToken" />
    Task<Result<RawAuditData>> GetRawAuditDataAsync(CancellationToken cancellationToken);
}