namespace RobotManagementSystem.Shared.Models.Authentication;

public class AuthenticationResponse
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public DateTime Expires { get; set; }
    public string Role { get; set; } = string.Empty;
}