using RobotManagementSystem.Shared.Models.Robot;
using RobotManagementSystem.Shared.Models.Users;

namespace RobotManagementSystem.Shared.Models.MissionLog;

public class MissionLog
{
    public UserAccount User { get; set; } = new UserAccount();
    public DateTime Timestamp { get; set; }
    public UserRole Role { get; set; }
    public RobotCommand Command { get; set; }
    public RobotCommandResult CommandResult { get; set; }
}