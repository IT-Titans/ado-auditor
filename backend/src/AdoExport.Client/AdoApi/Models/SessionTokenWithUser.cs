namespace AdoExport.Client.AdoApi.Models;

#nullable disable

public class SessionTokenWithUser
{
    public required SessionTokenDto SessionToken { get; init; }

    public required UserDto User { get; init; }
}