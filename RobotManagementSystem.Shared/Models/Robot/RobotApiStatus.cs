namespace RobotManagementSystem.Shared.Models.Robot;

public static class RobotApiStatus
{
    public const string Disconnected = "Disconnected";
    public const string Connected = "Connected";
    public const string Reconnecting = "Reconnecting";
    
    public const string StatusMethod  = "ConnectionStatus";
}