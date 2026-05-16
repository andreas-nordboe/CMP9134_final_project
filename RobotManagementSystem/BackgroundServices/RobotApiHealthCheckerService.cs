using RobotManagementSystem.Services;

namespace RobotManagementSystem.BackgroundServices;

// This service checks the health of robot API every 2 seconds and sends a socket to clients
public class RobotApiHealthCheckerService : BackgroundService
{
    private readonly IRobotStatusService _robotStatusService;

    public RobotApiHealthCheckerService(IRobotStatusService robotStatusService)
    {
        _robotStatusService = robotStatusService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _robotStatusService.GetRobotStatusAsync();
            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }
}