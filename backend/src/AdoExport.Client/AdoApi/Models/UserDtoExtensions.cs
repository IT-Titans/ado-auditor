namespace AdoExport.Client.AdoApi.Models;

public static class UserDtoExtensions
{
    /// <summary>
    /// Converts the <see cref="UserDto.MetaType" /> of the given <paramref name="userDto" /> to a displayable string.
    /// </summary>
    public static string DisplayableMetaType(this UserDto userDto)
    {
        Guard.IsNotNull(userDto);

        return userDto.MetaType ?? "unknown";
    }

    /// <summary>
    /// Determines whether the given <paramref name="userDto" /> represents a user identity (member, guest).
    /// </summary>
    /// <param name="userDto">The user to check.</param>
    /// <returns>true, when the given <paramref name="userDto" /> represents a user identity; otherwise false.</returns>
    public static bool IsRealUserType(this UserDto userDto)
    {
        Guard.IsNotNull(userDto);

        return string.Equals(userDto.MetaType, UserDto.MemberUserMetaType, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(userDto.MetaType, UserDto.GuestUserMetaType, StringComparison.OrdinalIgnoreCase);
    }
}