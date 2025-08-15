using Microsoft.AspNetCore.Http;

namespace AdoExport.Client.AdoApi.Helpers;

public static class QueryHelpers
{
    public static string BuildUrlWithQueryString(string basePath, Dictionary<string, string?> queryParams)
    {
        return basePath + QueryString.Create(queryParams);
    }
}