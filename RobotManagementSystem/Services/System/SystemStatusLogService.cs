using RobotManagementSystem.Data;
using RobotManagementSystem.Shared.Models.Robot;
using RobotManagementSystem.Shared.Models.SystemStatus;

namespace RobotManagementSystem.Services.System;

// TODO: Backend Monitoring
// Robot API Disconnnected
// Robot API Reconnected
// Robot Entered LOW_BATTERY state
// ROBOT became STUCK
// Backend
public class SystemStatusLogService : ISystemStatusLogService
{
    private readonly RobotApiDbContext _dbContext;
    private readonly ILogger<SystemStatusLogService> _logger;

    public SystemStatusLogService(RobotApiDbContext dbContext, ILogger<SystemStatusLogService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task LogConnectionChangedAsync(string newStatus)
    {
        var log = new SystemStatusLog
        {
            Timestamp = DateTime.UtcNow,
            EventType = "CONNECTION_CHANGED",
            Message = $"Robot API connection changed to {newStatus}",
            CurrentStatus = newStatus
        };

        _dbContext.SystemStatusLogs.Add(log);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Robot API connection changed to {Status}", newStatus);
    }

    public async Task LogRobotStatusChangedAsync(string robotStatus)
    {
        var log = new SystemStatusLog
        {
            Timestamp = DateTime.UtcNow,
            EventType = "ROBOT_STATUS_CHANGED",
            Message = $"Robot status changed to {robotStatus}",
            CurrentStatus = robotStatus,
            RobotState = robotStatus
        };

        _dbContext.SystemStatusLogs.Add(log);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Robot status changed to {Status}", robotStatus);
    }

    public async Task LogTelemetrySnapshotAsync(RobotStatusResponse status)
    {
        var log = new SystemStatusLog
        {
            Timestamp = DateTime.UtcNow,
            EventType = "TELEMETRY_SNAPSHOT",
            Message = "Periodic robot telemetry snapshot",
            RobotX = status.Position.X,
            RobotY = status.Position.Y,
            Battery = status.Battery,
            RobotState = status.Status
        };

        _dbContext.SystemStatusLogs.Add(log);
        await _dbContext.SaveChangesAsync();
    }

    public async Task LogBackendEventAsync(string eventType, string message)
    {
        var log = new SystemStatusLog
        {
            Timestamp = DateTime.UtcNow,
            EventType = eventType,
            Message = message
        };

        _dbContext.SystemStatusLogs.Add(log);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("{EventType}: {Message}", eventType, message);
    }
}