using Microsoft.AspNetCore.SignalR;
using RobotManagementSystem.Services;
using RobotManagementSystem.Shared.Models.Robot;

namespace RobotManagementSystem.Hubs;

public class RobotTelemetryHub : Hub
{
    private readonly ILogger<RobotTelemetryHub> _logger;
    private readonly IRobotApiStatusStore _robotApiStatusStore;
    
    public RobotTelemetryHub(ILogger<RobotTelemetryHub> logger, IRobotApiStatusStore robotApiStatusStore)
    {
        _logger = logger;
        _robotApiStatusStore = robotApiStatusStore;
    }
    
    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation($"Client connected: {Context.ConnectionId}");
        
        // This sends the current robot status to the client when they connect so that they can correctly display the robot status
        await Clients.Caller.SendCoreAsync(RobotApiStatus.StatusMethod, new object[] { _robotApiStatusStore });
        await base.OnConnectedAsync();
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");
        await base.OnDisconnectedAsync(exception);
    }
}