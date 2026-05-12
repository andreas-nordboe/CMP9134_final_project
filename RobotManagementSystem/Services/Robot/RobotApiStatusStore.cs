using RobotManagementSystem.Shared.Models.Robot;

namespace RobotManagementSystem.Services;

public class RobotApiStatusStore : IRobotApiStatusStore
{
    public string CurrentApiStatus { get; set; } = RobotApiStatus.Disconnected.ToString();
}