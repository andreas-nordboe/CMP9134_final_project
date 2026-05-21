namespace RobotManagementSystem.Shared.Models.Users;

// This is used for external clients/requests as PasswordHash from UserAccount should not be externally exposed  
public class UserAccountDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime? LastLoggedIn { get; set; }
    public DateTime CreatedAt { get; set; }
}