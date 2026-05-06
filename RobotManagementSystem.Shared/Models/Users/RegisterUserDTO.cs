namespace RobotManagementSystem.Shared.Models.Users;

public class RegisterUserDTO
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string RepeatPassword { get; set; }
}