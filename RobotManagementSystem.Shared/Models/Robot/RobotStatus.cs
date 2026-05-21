namespace RobotManagementSystem.Shared.Models.Robot;

public enum RobotStatus
{
    IDLE,
    MOVING,
    LOW_BATTERY,
    STUCK,
    CHARGING // UI only
}