namespace RobotManagementSystem.Shared.Models;

public class RegisterUserResponse
{
    public string Token { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    
    public bool IsSuccessful { get; set; }
    public int ErrorCode { get; set; }
}