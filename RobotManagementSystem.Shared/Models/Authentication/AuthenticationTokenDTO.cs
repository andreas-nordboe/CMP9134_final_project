using RobotManagementSystem.Shared.Models.Users;

namespace RobotManagementSystem.Shared.Models.Authentication;

public class AuthenticationTokenDTO
{
    public string Username { get; set; }
    public string UserId { get; set; }
    public UserRole Role { get; set; }
}