using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Client.Tests.AdoApi.Models;

public class AccessLevelDtoExtensionsTests
{
    [TestCase("active", ExpectedResult = true)]
    [TestCase("inactive", ExpectedResult = false)]
    public bool IsActive_returns_expected(string status)
    {
        var user = new AccessLevelDto()
        {
            Status = status,
        };

        return user.IsActive();
    }
}