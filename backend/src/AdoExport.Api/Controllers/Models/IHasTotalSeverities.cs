namespace AdoExport.Api.Controllers.Models;

public interface IHasTotalSeverities
{
    /// <summary>
    /// Gets the total count of rules affected with severity <see cref="Severities.Critical" />.
    /// </summary>
    public int TotalCriticals { get; }

    /// <summary>
    /// Gets the total count of rules affected with severity <see cref="Severities.Warning" />.
    /// </summary>
    public int TotalWarnings { get; }

    /// <summary>
    /// Gets the total count of rules affected with severity <see cref="Severities.Ok" />.
    /// </summary>
    public int TotalOks { get; }
}