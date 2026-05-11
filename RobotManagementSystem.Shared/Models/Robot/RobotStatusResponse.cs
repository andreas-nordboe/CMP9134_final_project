using RobotManagementSystem.Shared.Models.Components;

namespace RobotManagementSystem.Shared.Models.Robot;

public class RobotStatusResponse
{
    public string Id { get; set; } = string.Empty;
    public Vector2D Position { get; set; } = new Vector2D();
    public double Battery { get; set; }
    public string Status { get; set; }  = string.Empty;
}