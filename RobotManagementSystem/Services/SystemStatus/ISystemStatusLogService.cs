using RobotManagementSystem.Shared.Models.Robot;
using RobotManagementSystem.Shared.Models.SystemStatus;

namespace RobotManagementSystem.Services.SystemStatus;

public interface ISystemStatusLogService
{
    Task LogConnectionChangedAsync(string newStatus);
    Task LogRobotStatusChangedAsync(RobotStatusResponse robotStatus);
    Task LogTelemetrySnapshotAsync(RobotStatusResponse status);
    Task LogBackendEventAsync(SystemStatusEventType eventType, string message);
    Task<PagedResult<SystemStatusLog>> GetSystemStatusLogsAsync(SystemStatusEventType? eventType, DateTime? fromDate, int page, int pageSize);
}