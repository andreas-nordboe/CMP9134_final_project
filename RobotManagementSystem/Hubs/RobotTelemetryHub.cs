using Microsoft.AspNetCore.SignalR;

namespace RobotManagementSystem.Hubs;

public class RobotTelemetryHub : Hub
{
    private readonly ILogger<RobotTelemetryHub> _logger;
    
    public RobotTelemetryHub(ILogger<RobotTelemetryHub> logger)
    {
        _logger = logger;
    }
    
    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation($"Client connected: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");
        await base.OnDisconnectedAsync(exception);
    }
}