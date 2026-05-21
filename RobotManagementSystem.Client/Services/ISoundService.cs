namespace RobotManagementSystem.Client.Services;

public interface ISoundService
{
    Task PlaySoundAsync(string soundName);
    Task PlayErrorSoundAsync();
}