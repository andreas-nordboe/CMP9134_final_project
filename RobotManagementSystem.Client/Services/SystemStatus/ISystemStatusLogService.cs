using RobotManagementSystem.Shared.Models.SystemStatus;

namespace RobotManagementSystem.Client.Services.SystemStatus;

public interface ISystemStatusLogService
{
    Task<PagedResult<SystemStatusLog>> GetSystemStatusLogsAsync(SystemStatusEventType? eventType, DateTime? fromDate, int page, int pageSize);
    Task<SystemStatusSummary?> GetSystemStatusSummaryAsync();
}