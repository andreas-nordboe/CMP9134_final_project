using RobotManagementSystem.Shared.Models.Components;
using RobotManagementSystem.Shared.Models.Robot;
using RobotManagementSystem.Shared.Models.Users;

namespace RobotManagementSystem.Shared.Models.MissionLog;

public class AddMissionLogRequest
{
    public int UserId { get; set; }
    public UserRole Role { get; set; }
    public RobotCommand Command { get; set; }
    public RobotCommandResult CommandResult { get; set; }
    public string? Details { get; set; }
    public double? RobotBattery { get; set; }
    public Vector2D? RobotPosition { get; set; }
    public string? ConnectionStatus { get; set; }
}