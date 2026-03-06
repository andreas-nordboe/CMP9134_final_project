namespace RobotManagementSystem.Shared.Models.Authentication;

public class AuthenticationResponse
{
    public string UserId { get; set; }
    public string Username { get; set; }
    public string AccessToken { get; set; }
    public DateTime Expires { get; set; }
    public string Role { get; set; }
    public APIError? Error { get; set; }
}