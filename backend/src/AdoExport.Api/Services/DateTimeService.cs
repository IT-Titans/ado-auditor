using AdoExport.Common.Services;

namespace AdoExport.Api.Services;

internal class DateTimeService : IDateTimeService
{
    /// <inheritdoc />
    public DateTime UtcNow()
    {
        return DateTime.UtcNow;
    }
}