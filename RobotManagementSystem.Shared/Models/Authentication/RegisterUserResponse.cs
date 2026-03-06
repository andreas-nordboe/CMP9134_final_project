namespace RobotManagementSystem.Shared.Models.Authentication;

public class RegisterUserResponse
{
    public string UserId { get; set; }
    public string AccessToken { get; set; }
    public string Username { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    
    public bool IsSuccessful { get; set; }
    public int ErrorCode { get; set; }
}