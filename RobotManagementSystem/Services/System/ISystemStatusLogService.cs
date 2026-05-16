using RobotManagementSystem.Shared.Models.Robot;

namespace RobotManagementSystem.Services.System;

public interface ISystemStatusLogService
{
    Task LogConnectionChangedAsync(string newStatus);
    Task LogRobotStatusChangedAsync(string robotStatus);
    Task LogTelemetrySnapshotAsync(RobotStatusResponse status);
    Task LogBackendEventAsync(string eventType, string message);
}