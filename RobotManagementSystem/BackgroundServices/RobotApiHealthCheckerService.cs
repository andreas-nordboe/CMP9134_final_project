using RobotManagementSystem.Services;

namespace RobotManagementSystem.BackgroundServices;

// This service checks the health of robot API every 2 seconds and sends a socket to clients
public class RobotApiHealthCheckerService : BackgroundService
{
    private readonly IServiceScopeFactory  _scopeFactory;

    public RobotApiHealthCheckerService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            
            var robotStatusService = scope.ServiceProvider.GetRequiredService<IRobotStatusService>();
            
            await robotStatusService.GetRobotStatusAsync();
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }
}