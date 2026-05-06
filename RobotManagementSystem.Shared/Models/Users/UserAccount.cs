using System.ComponentModel.DataAnnotations;

namespace RobotManagementSystem.Shared.Models.Users;

public class UserAccount
{
    [Key] public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}