using RobotManagementSystem.Shared.Models.RBAC;

namespace RobotManagementSystem.Shared.Models.Users;

public class User
{
    public string? UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Username { get; set; }
    public UserRole Role { get; set; }
    public bool IsLoggedIn { get; set; }
}