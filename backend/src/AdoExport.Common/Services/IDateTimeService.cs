namespace AdoExport.Common.Services;

public interface IDateTimeService
{
    /// <summary>
    /// Gets a <see cref="DateTime" /> object that is set to the current date and time according to the implementor, expressed as the Coordinated Universal Time (UTC).
    /// </summary>
    DateTime UtcNow();
}