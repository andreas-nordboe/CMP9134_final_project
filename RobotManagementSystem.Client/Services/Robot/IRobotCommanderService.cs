using RobotManagementSystem.Shared.Models.Robot;

namespace RobotManagementSystem.Client.Services.Robot;

public interface IRobotCommanderService
{
    Task<RobotCommandResponse?> MoveRobotAsync(RobotNavigationRequest request);
    Task<RobotCommandResponse?> ResetAsync();
}