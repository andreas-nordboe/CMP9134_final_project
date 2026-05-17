using RobotManagementSystem.Shared.Models.Robot;

namespace RobotManagementSystem.Services;

public class RobotApiStatusStore : IRobotApiStatusStore
{
    public string CurrentApiStatus { get; set; } = RobotApiStatus.Disconnected.ToString();
    public string LastRobotState { get; set; } = string.Empty;
    public DateTime LastSnapshotLoggedAt { get; set; } = DateTime.MinValue;
    
    public double? LastLatencyMs { get; set; }
    public List<double> RecentLatenciesMs { get; } = [];
    public int RetryAttempts { get; set; }
}