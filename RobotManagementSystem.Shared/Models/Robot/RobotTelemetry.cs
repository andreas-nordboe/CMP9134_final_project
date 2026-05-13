using RobotManagementSystem.Shared.Models.Components;

namespace RobotManagementSystem.Shared.Models.Robot;

public class RobotTelemetry
{
    public Vector2D Position { get; set; } = new Vector2D();
    public double Battery { get; set; }
    public string Status { get; set; } = string.Empty;
    public SensorData SensorData { get; set; } = new SensorData();
}