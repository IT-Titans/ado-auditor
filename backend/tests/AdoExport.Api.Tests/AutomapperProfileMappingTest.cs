using AutoMapper;

namespace AdoExport.Api.Tests;

public class AutomapperProfileMappingTest
{
    [Test]
    public void AutoMapper_Configuration_IsValid()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(typeof(Program));
        });

        configuration.AssertConfigurationIsValid();
    }
}