using RobotManagementSystem.Shared.Models.Components;

namespace RobotManagementSystem.Shared.Models.Robot;

public class RobotTelemetry
{
    public Vector2D Position { get; set; }
    public int Battery { get; set; }
    public string Status { get; set; }
    public Sensors Sensors { get; set; }
}