using RobotManagementSystem.Shared.Models.Robot;

namespace RobotManagementSystem.Services;

public interface IRobotStatusService
{
    Task<RobotStatusResponse?> GetRobotStatusAsync();
}