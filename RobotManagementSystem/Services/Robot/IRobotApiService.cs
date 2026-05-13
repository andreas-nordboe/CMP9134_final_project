using RobotManagementSystem.Shared.Models.Map;
using RobotManagementSystem.Shared.Models.Robot;

namespace RobotManagementSystem.Services;

public interface IRobotApiService
{
    Task<RobotStatusResponse?> GetRobotStatusAsync();
    Task<MapResponse?> GetMapAsync();
    Task<SensorData?> GetSensorDataAsync();
    Task<RobotCommandResponse?> MoveRobotAsync(RobotNavigationRequest request);
    Task<RobotCommandResponse?> ResetAsync();
}