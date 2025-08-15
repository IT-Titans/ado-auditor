namespace AdoExport.Client.AdoApi.Models;

public static class AccessLevelDtoExtensions
{
    /// <summary>
    /// Determines whether the user that is associated with this <paramref name="accessLevel" /> is active (true) or not (false).
    /// </summary>
    public static bool IsActive(this AccessLevelDto accessLevel)
    {
        ArgumentNullException.ThrowIfNull(accessLevel);

        return string.Equals(accessLevel.Status, "active", StringComparison.OrdinalIgnoreCase);
    }
}