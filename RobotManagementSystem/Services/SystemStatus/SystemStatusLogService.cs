using Microsoft.EntityFrameworkCore;
using RobotManagementSystem.Data;
using RobotManagementSystem.Services.SystemStatus;
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
    private readonly IRobotApiStatusStore _robotApiStatusStore;

    public SystemStatusLogService(RobotApiDbContext dbContext, ILogger<SystemStatusLogService> logger, IRobotApiStatusStore robotApiStatusStore)
    {
        _dbContext = dbContext;
        _logger = logger;
        _robotApiStatusStore = robotApiStatusStore;
    }

    public async Task LogConnectionChangedAsync(string newStatus)
    {
        var log = new SystemStatusLog
        {
            Timestamp = DateTime.UtcNow,
            EventType = SystemStatusEventType.CONNECTION_CHANGED,
            Message = $"Robot API connection changed to {newStatus}",
            CurrentStatus = newStatus
        };

        _dbContext.SystemStatusLogs.Add(log);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Robot API connection changed to {Status}", newStatus);
    }

    public async Task LogRobotStatusChangedAsync(RobotStatusResponse robotStatus)
    {
        var log = new SystemStatusLog
        {
            Timestamp = DateTime.UtcNow,
            EventType = SystemStatusEventType.ROBOT_STATUS_CHANGED,
            Message = $"Robot status changed to {robotStatus.Status}",
            CurrentStatus = robotStatus.Status,
            RobotState = robotStatus.Status,
            RobotX = robotStatus.Position.X,
            RobotY = robotStatus.Position.Y,
            Battery = robotStatus.Battery
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
            EventType = SystemStatusEventType.TELEMETRY_SNAPSHOT,
            Message = "Periodic robot telemetry snapshot",
            RobotX = status.Position.X,
            RobotY = status.Position.Y,
            Battery = status.Battery,
            RobotState = status.Status
        };

        _dbContext.SystemStatusLogs.Add(log);
        await _dbContext.SaveChangesAsync();
    }

    public async Task LogBackendEventAsync(SystemStatusEventType eventType, string message)
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

    public async Task<PagedResult<SystemStatusLog>> GetSystemStatusLogsAsync(SystemStatusEventType? eventType, DateTime? fromDate, int page, int pageSize)
    {
        var query = _dbContext.SystemStatusLogs.AsQueryable();

        if (eventType != null)
        {
            query = query.Where(x => x.EventType == eventType);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(x => x.Timestamp >= fromDate.Value);
        }

        var totalCount = await query.CountAsync();
        
        var logs = await query
            .OrderByDescending(x => x.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new SystemStatusLog
            {
                Id = x.Id,
                Timestamp = x.Timestamp,
                EventType = x.EventType,
                Message = x.Message,
                CurrentStatus = x.CurrentStatus,
                RobotX = x.RobotX,
                RobotY = x.RobotY,
                Battery = x.Battery,
                RobotState = x.RobotState
            })
            .ToListAsync();

        return new PagedResult<SystemStatusLog>
        {
            Items = logs,
            TotalCount = totalCount,
            PageIndex = page,
            PageSize = pageSize
        };

    }

    public async Task<SystemStatusSummary> GetSystemStatusSummaryAsync()
    {
        var latestTelemetry = await _dbContext.SystemStatusLogs
            .Where(x => x.EventType == SystemStatusEventType.TELEMETRY_SNAPSHOT)
            .OrderByDescending(x => x.Timestamp)
            .FirstOrDefaultAsync();

        var latestLog = await _dbContext.SystemStatusLogs
            .OrderByDescending(x => x.Timestamp)
            .FirstOrDefaultAsync();

        double? averageLatency = _robotApiStatusStore.RecentLatenciesMs.Any()
            ? _robotApiStatusStore.RecentLatenciesMs.Average()
            : null;

        return new SystemStatusSummary
        {
            ConnectionStatus = _robotApiStatusStore.CurrentApiStatus,
            SignalState = _robotApiStatusStore.CurrentApiStatus == RobotApiStatus.Connected
                ? "Stable"
                : "Reconnecting",

            AverageLatencyMs = averageLatency,
            LastLatencyMs = _robotApiStatusStore.LastLatencyMs,
            RetryAttempts = _robotApiStatusStore.RetryAttempts,
            LastUpdated = latestLog?.Timestamp,

            Battery = latestTelemetry?.Battery,
            RobotX = latestTelemetry?.RobotX,
            RobotY = latestTelemetry?.RobotY,
            RobotState = latestTelemetry?.RobotState
        };
    }
}