using System.ComponentModel.DataAnnotations;

namespace RobotManagementSystem.Shared.Models.Users;

// This model is used only internally for database persistence (see UserAccountDto for the model that is exposed through API endpoints)
public class UserAccount
{
    [Key] public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime? LastLoggedIn { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}