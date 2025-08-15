using AdoExport.Api.Auditing;
using AdoExport.Auditing;

namespace AdoExport.Api.Services.Caching;

public interface IAuditCache
{
    Task SetAsync(string key, Audit data);

    Task<Audit?> GetAsync(string key);
}