namespace RobotManagementSystem.Services;

public class RobotCommandRateLimiter : IRobotCommandRateLimiter
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly TimeSpan _minimumInterval = TimeSpan.FromMilliseconds(200);
    private DateTime _lastCommandSentAtUtc = DateTime.MinValue;

    
    public async Task WaitAsync()
    {
        await _lock.WaitAsync();

        try
        {
            var now = DateTime.UtcNow;
            var timeSinceLastCommand = now - _lastCommandSentAtUtc;

            if (timeSinceLastCommand < _minimumInterval)
            {
                var delay = _minimumInterval - timeSinceLastCommand;
                await Task.Delay(delay);
            }

            _lastCommandSentAtUtc = DateTime.UtcNow;
        }
        finally
        {
            _lock.Release();
        }
    }
}