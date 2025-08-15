using AdoExport.Api.Auditing;
using AdoExport.Auditing;
using AdoExport.Client.AdoApi;

namespace AdoExport.Api.Services.Caching;

public interface ICreateAuditDataCache
{
    Task SetAsync(string key, CreateAuditData data);

    Task<CreateAuditData?> GetAsync(string key);
}