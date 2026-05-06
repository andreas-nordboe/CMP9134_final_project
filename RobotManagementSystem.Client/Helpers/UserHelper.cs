using RobotManagementSystem.Shared.Models.Authentication;
using RobotManagementSystem.Shared.Models.Users;

namespace RobotManagementSystem.Client.Helpers;

public static class UserHelper
{
    public static User ToUser(AuthenticationResponse authenticationResponse)
    {
        return new User
        {
            UserId = authenticationResponse.UserId,
            Username = authenticationResponse.Username,
            Role = Enum.Parse<UserRole>(authenticationResponse.Role),
            IsLoggedIn = !string.IsNullOrEmpty(authenticationResponse.AccessToken) && authenticationResponse.Expires > DateTime.UtcNow
        };
    }
}