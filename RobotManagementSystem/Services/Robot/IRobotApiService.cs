using RobotManagementSystem.Shared.Models.Map;
using RobotManagementSystem.Shared.Models.Robot;
using RobotManagementSystem.Shared.Models.Users;

namespace RobotManagementSystem.Services;

public interface IRobotApiService
{
    Task<RobotStatusResponse?> GetRobotStatusAsync();
    Task<MapResponse?> GetMapAsync();
    Task<RobotCommandResponse?> MoveRobotAsync(RobotNavigationRequest request, int userId, UserRole userRole);
    Task<RobotCommandResponse?> ResetAsync();
}