namespace RobotManagementSystem.Shared.Models.Authentication;

public class APIError
{
    public int ErrorCode { get; set; } = 0;
    public string ErrorMessage { get; set; } = string.Empty;

    public List<string> Metadata { get; set; } =
        new List<string>(); // This may become useful later for providing list of errors or exceptions
}