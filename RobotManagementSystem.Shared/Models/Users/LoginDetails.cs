namespace RobotManagementSystem.Shared.Models.Users;

// TODO Code smell - Refactor to use login request class instead to avoid duplication 
public class LoginDetails
{
    public string Username { get; set; }
    public string Password { get; set; }
}