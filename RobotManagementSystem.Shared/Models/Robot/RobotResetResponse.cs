namespace RobotManagementSystem.Shared.Models.Robot;

// I made this a class even though it only contains a single boolean
// as I might want to add more data to robot reset responses in the future
public class RobotResetResponse
{
    public bool ResetSuccessful { get; set; }
}