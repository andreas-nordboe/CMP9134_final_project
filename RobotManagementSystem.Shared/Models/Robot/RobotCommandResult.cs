namespace RobotManagementSystem.Shared.Models.Robot;

public enum RobotCommandResult
{
    Success,
    Failure,
    Retried,
    PermissionsDenied,
    InvalidCoordinates,
    BlockedByObstacle // for when the user tries to move into an obstacle
}