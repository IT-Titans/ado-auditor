using AdoExport.Client.AdoApi.Helpers;

namespace AdoExport.Client.Tests.AdoApi.Helpers;

public class QueryHelpersTests
{
    [Test]
    public void AppendsQueryParametersProperlyFormatted()
    {
        QueryHelpers.BuildUrlWithQueryString("https://localhost", new()
        {
            { "continuationToken", null },
            { "someOther-Value", "value with spacing" },
            { "apiVersion", "v1.7" }
        })
            .Should()
            .Be("https://localhost?continuationToken=&someOther-Value=value%20with%20spacing&apiVersion=v1.7");
    }
}