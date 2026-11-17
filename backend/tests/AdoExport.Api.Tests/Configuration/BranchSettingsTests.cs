using AdoExport.Api.Configuration;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace AdoExport.Api.Tests.Configuration;

/// <summary>
/// The mechanism on its own, against a directory the test controls.
/// </summary>
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class BranchSettingsTests : IDisposable
{
    private readonly DirectoryInfo _directory = Directory.CreateTempSubdirectory("branch-settings-");

    public void Dispose() => _directory.Delete(recursive: true);

    private void WriteFile(string name, string content) => File.WriteAllText(Path.Combine(_directory.FullName, name), content);

    /// <summary>
    /// Builds the configuration the way the host does: the settings file first, then a
    /// source standing in for the environment variables that come after it.
    /// </summary>
    private (ConfigurationManager Configuration, BranchSettingsOutcome Outcome) Start(
        string? branch,
        Dictionary<string, string?>? environment = null)
    {
        WriteFile("appsettings.json", """{ "RateLimiting": { "AuditPermitLimit": 5, "MailPermitLimit": 3 } }""");

        using var fileProvider = new PhysicalFileProvider(_directory.FullName);

        var configuration = new ConfigurationManager();
        configuration.SetBasePath(_directory.FullName);
        configuration.AddJsonFile("appsettings.json");

        environment ??= [];
        environment[BranchSettings.BranchKey] = branch;
        configuration.AddInMemoryCollection(environment);

        var outcome = BranchSettings.Apply(configuration, fileProvider);

        return (configuration, outcome);
    }

    [Test]
    public void LoadsTheFileOfTheBranchOnTopOfTheDefaults()
    {
        WriteFile("appsettings.branch.main.json", """{ "RateLimiting": { "AuditPermitLimit": 60 } }""");

        var (configuration, outcome) = Start("main");

        configuration["RateLimiting:AuditPermitLimit"].Should().Be("60");
        configuration["RateLimiting:MailPermitLimit"].Should().Be("3", "what the branch file does not mention keeps its default");
        outcome.Should().Be(new BranchSettingsOutcome("main", "appsettings.branch.main.json", Applied: true));
    }

    [Test]
    public void LetsASettingMadeOnTheAppServiceWin()
    {
        // An operator who sets a limit on the App Service means it. The file must sit
        // below the environment, not above it.
        WriteFile("appsettings.branch.main.json", """{ "RateLimiting": { "AuditPermitLimit": 60 } }""");

        var (configuration, _) = Start("main", new Dictionary<string, string?> { ["RateLimiting:AuditPermitLimit"] = "7" });

        configuration["RateLimiting:AuditPermitLimit"].Should().Be("7");
    }

    [Test]
    public void ReadsCommentsInTheFile()
    {
        // The production file explains itself in comments. If they stopped being
        // accepted, production would not start.
        WriteFile(
            "appsettings.branch.main.json",
            """
            {
              // why this value is what it is
              "RateLimiting": { "AuditPermitLimit": 60 }
            }
            """);

        var (configuration, _) = Start("main");

        configuration["RateLimiting:AuditPermitLimit"].Should().Be("60");
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void DoesNothingWhenTheImageDoesNotSayWhereItCameFrom(string? branch)
    {
        WriteFile("appsettings.branch.main.json", """{ "RateLimiting": { "AuditPermitLimit": 60 } }""");

        var (configuration, outcome) = Start(branch);

        configuration["RateLimiting:AuditPermitLimit"].Should().Be("5");
        outcome.Applied.Should().BeFalse();
        outcome.FileName.Should().BeNull();
    }

    [TestCase("dev")]
    [TestCase("merge")]
    public void DoesNothingForABranchWithoutAFile(string branch)
    {
        WriteFile("appsettings.branch.main.json", """{ "RateLimiting": { "AuditPermitLimit": 60 } }""");

        var (configuration, outcome) = Start(branch);

        configuration["RateLimiting:AuditPermitLimit"].Should().Be("5");
        outcome.Should().Be(new BranchSettingsOutcome(branch, $"appsettings.branch.{branch}.json", Applied: false));
    }

    [TestCase("../main")]
    [TestCase("main/../../secrets")]
    [TestCase("Main")]
    [TestCase("main.json")]
    public void IgnoresAValueThatIsNotAPlainBranchName(string branch)
    {
        WriteFile("appsettings.branch.main.json", """{ "RateLimiting": { "AuditPermitLimit": 60 } }""");

        var (configuration, outcome) = Start(branch);

        configuration["RateLimiting:AuditPermitLimit"].Should().Be("5");
        outcome.Should().Be(new BranchSettingsOutcome(branch, FileName: null, Applied: false));
    }

    [Test]
    public void RefusesAMissingConfiguration()
    {
        using var fileProvider = new PhysicalFileProvider(_directory.FullName);

        var act = () => BranchSettings.Apply(null!, fileProvider);

        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void RefusesAMissingFileProvider()
    {
        var act = () => BranchSettings.Apply(new ConfigurationManager(), null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void RefusesAMissingBuilder()
    {
        var act = () => BranchSettings.AddBranchSettings(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
