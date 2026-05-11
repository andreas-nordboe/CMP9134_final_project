using RobotManagementSystem.Shared.Models.Robot;

namespace RobotManagementSystem.Services;

public interface IRobotApiService
{
    Task<RobotStatusResponse?> GetRobotStatusAsync();
    Task<RobotCommandResponse?> MoveRobotAsync(RobotNavigationRequest request);
    Task<RobotCommandResponse?> ResetAsync();
}