using RobotManagementSystem.Shared.Models.Robot;
using RobotManagementSystem.Shared.Models.SystemStatus;

namespace RobotManagementSystem.Services.System;

public interface ISystemStatusLogService
{
    Task LogConnectionChangedAsync(string newStatus);
    Task LogRobotStatusChangedAsync(string robotStatus);
    Task LogTelemetrySnapshotAsync(RobotStatusResponse status);
    Task LogBackendEventAsync(string eventType, string message);
    Task<PagedResult<SystemStatusLog>> GetSystemStatusLogsAsync(string? eventType, DateTime? fromDate, int page, int pageSize);
}