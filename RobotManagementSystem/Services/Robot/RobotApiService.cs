using RobotManagementSystem.Shared.Models.Robot;

namespace RobotManagementSystem.Services;

public class RobotApiService : IRobotApiService
{
    public Task<RobotStatusResponse?> GetRobotStatusAsync()
    {
        throw new NotImplementedException();
    }

    public Task<RobotCommandResponse?> MoveRobotAsync(RobotNavigationRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<RobotCommandResponse?> ResetAsync()
    {
        throw new NotImplementedException();
    }
}