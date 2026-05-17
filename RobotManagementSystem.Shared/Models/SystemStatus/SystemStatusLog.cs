namespace RobotManagementSystem.Shared.Models.SystemStatus;

public class SystemStatusLog
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }

    public SystemStatusEventType EventType { get; set; } = SystemStatusEventType.UNKNOWN;
    public string Message { get; set; } = "";

    public string? CurrentStatus { get; set; }

    public int? RobotX { get; set; }
    public int? RobotY { get; set; }
    public double? Battery { get; set; }

    public string? RobotState { get; set; }
}