namespace RobotManagementSystem.Shared.Models.Robot;

public enum RobotCommandResult
{
    Success,
    Failure,
    Retried,
    PermissionsDenied,
    InvalidCoordinates
}