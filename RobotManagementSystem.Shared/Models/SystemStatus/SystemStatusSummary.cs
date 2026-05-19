namespace RobotManagementSystem.Shared.Models.SystemStatus;

public class SystemStatusSummary
{
    public string ConnectionStatus { get; set; } = "Unknown";
    public string SignalState { get; set; } = "Unknown";

    public double? AverageLatencyMs { get; set; }
    public double? LastLatencyMs { get; set; }
    public List<double> RecentLatenciesMs { get; set; } = [];

    public int RetryAttempts { get; set; }
    public DateTime? LastUpdated { get; set; }

    public double? Battery { get; set; }
    public int? RobotX { get; set; }
    public int? RobotY { get; set; }
    public string? RobotState { get; set; }
}