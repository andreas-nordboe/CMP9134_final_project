using System.ComponentModel.DataAnnotations;
using RobotManagementSystem.Shared.Models.Robot;
using RobotManagementSystem.Shared.Models.Users;

namespace RobotManagementSystem.Shared.Models.MissionLog;

public class MissionLog
{
    [Key]
    public int Id { get; set; }
    public int UserId { get; set; }
    public UserAccount? User { get; set; } = default!;
    public DateTime Timestamp { get; set; }
    public UserRole Role { get; set; }
    public RobotCommand Command { get; set; }
    public RobotCommandResult CommandResult { get; set; }
    public string? Details { get; set; }
    public int? RobotX { get; set; }
    public int? RobotY { get; set; }
    public int? Battery { get; set; }
    public string? ConnectionStatus { get; set; }
}