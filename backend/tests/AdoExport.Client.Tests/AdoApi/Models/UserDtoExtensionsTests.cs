using AdoExport.Client.AdoApi.Models;

namespace AdoExport.Client.Tests.AdoApi.Models;

public class UserDtoExtensionsTests
{
    [TestCase("member", ExpectedResult = "member")]
    [TestCase("agent", ExpectedResult = "agent")]
    [TestCase(null, ExpectedResult = "unknown")]
    public string DisplayableMetaType_returns_expected(string? metaType)
    {
        var user = new UserDto()
        {
            MetaType = metaType,
        };

        return user.DisplayableMetaType();
    }
}