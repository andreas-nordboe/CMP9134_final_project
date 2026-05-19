namespace RobotManagementSystem.Services;

public interface IRobotCommandRateLimiter
{
    Task WaitAsync();
}