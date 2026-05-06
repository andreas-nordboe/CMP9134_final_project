namespace RobotManagementSystem.Shared.Models.Authentication;

public class RegisterUserResponse
{
    public string UserId { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsSuccessful { get; set; }
    public int ErrorCode { get; set; }
}