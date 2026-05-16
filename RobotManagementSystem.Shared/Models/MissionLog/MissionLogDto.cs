using RobotManagementSystem.Shared.Models.Components;
using RobotManagementSystem.Shared.Models.Robot;
using RobotManagementSystem.Shared.Models.Users;

namespace RobotManagementSystem.Shared.Models.MissionLog;

public class MissionLogDto
{
    public int LogId { get; set; }
    public UserAccountDto User { get; set; }
    public DateTime Timestamp { get; set; }
    public UserRole Role { get; set; }
    public RobotCommand Command { get; set; }
    public RobotCommandResult CommandResult { get; set; }
    public string? Details { get; set; }
    public Vector2D? RobotPosition { get; set; }
    public int? Battery { get; set; }
    public string? ConnectionStatus { get; set; }
}