using RobotManagementSystem.Shared.Models.Components;

namespace RobotManagementSystem.Shared.Models.Robot;

public class RobotCommandResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Vector2D RobotPosition { get; set; } = new Vector2D();
}