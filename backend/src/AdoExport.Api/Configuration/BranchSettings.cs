using System.Text.RegularExpressions;

using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.FileProviders;

namespace AdoExport.Api.Configuration;

/// <summary>
/// Settings that apply to images built from one particular branch.
/// </summary>
/// <remarks>
/// The development and the production App Services run the same image definition, and
/// nothing in this repository tells them apart at run time: whether
/// <c>ASPNETCORE_ENVIRONMENT</c> differs between them is an App Service setting that is not
/// kept here. What is known is where an image came from. The pipeline deploys to
/// production from <c>main</c> and from nowhere else, and it passes the branch into the
/// image build, which stores it as <c>ADOAUDITOR__BUILDBRANCH</c>.
/// <para>
/// When a file <c>appsettings.branch.{branch}.json</c> exists for that branch it is
/// loaded on top of <c>appsettings.json</c>. It is placed below the environment
/// variables, so a setting made on the App Service still wins.
/// </para>
/// </remarks>
public static partial class BranchSettings
{
    /// <summary>
    /// The configuration key holding the branch the running image was built from.
    /// </summary>
    public const string BranchKey = "AdoAuditor:BuildBranch";

    /// <summary>
    /// Loads the settings file of the branch named in <see cref="BranchKey" />, if there
    /// is one.
    /// </summary>
    /// <returns>What was found, for the start-up log.</returns>
    public static BranchSettingsOutcome AddBranchSettings(this WebApplicationBuilder builder)
    {
        Guard.IsNotNull(builder);

        return Apply(builder.Configuration, builder.Environment.ContentRootFileProvider);
    }

    internal static BranchSettingsOutcome Apply(IConfigurationManager configuration, IFileProvider fileProvider)
    {
        Guard.IsNotNull(configuration);
        Guard.IsNotNull(fileProvider);

        var branch = configuration[BranchKey];

        if (string.IsNullOrWhiteSpace(branch))
        {
            return new BranchSettingsOutcome(Branch: null, FileName: null, Applied: false);
        }

        // The value ends up in a file name. Anything that is not a plain branch name is
        // ignored instead of being looked up.
        if (BranchName().IsMatch(branch) == false)
        {
            return new BranchSettingsOutcome(branch, FileName: null, Applied: false);
        }

        var fileName = $"appsettings.branch.{branch}.json";

        if (fileProvider.GetFileInfo(fileName).Exists == false)
        {
            return new BranchSettingsOutcome(branch, fileName, Applied: false);
        }

        var source = new JsonConfigurationSource
        {
            FileProvider = fileProvider,
            Path = fileName,
            Optional = false,
            ReloadOnChange = false,
        };

        // Directly after the last settings file and therefore before environment
        // variables and command line. Appending it would put it above them, and an App
        // Service setting could no longer override what the file says.
        var sources = configuration.Sources;
        var lastSettingsFile = -1;
        for (var index = 0; index < sources.Count; index++)
        {
            if (sources[index] is JsonConfigurationSource)
            {
                lastSettingsFile = index;
            }
        }

        sources.Insert(lastSettingsFile + 1, source);

        return new BranchSettingsOutcome(branch, fileName, Applied: true);
    }

    [GeneratedRegex("^[a-z0-9-]{1,64}$")]
    private static partial Regex BranchName();
}

/// <summary>
/// What <see cref="BranchSettings" /> did at start.
/// </summary>
/// <param name="Branch">The branch the image says it was built from, if it says so.</param>
/// <param name="FileName">The settings file that belongs to that branch.</param>
/// <param name="Applied">Whether that file exists and was loaded.</param>
public sealed record BranchSettingsOutcome(string? Branch, string? FileName, bool Applied);
