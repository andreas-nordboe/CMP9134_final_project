namespace RobotManagementSystem.Services;

// This is a singleton that holds the current status of the robot API connection that is sent to connecting clients
public interface IRobotApiStatusStore
{
    public string CurrentApiStatus { get; set; }
    string LastRobotState { get; set; }
    DateTime LastSnapshotLoggedAt { get; set; }
    
    // Summary fields
    double? LastLatencyMs { get; set; }
    List<double> RecentLatenciesMs { get; }
    int RetryAttempts { get; set; }
}