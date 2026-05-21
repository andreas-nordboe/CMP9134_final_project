using System.Diagnostics;
using RobotManagementSystem.Services;

namespace RobotManagementSystem.Tests.Robot;

public class RobotRateLimiterTests
{
    // Unit test: verifies that the global robot command rate limiter delays consecutive commands,
    // ensuring that two commands cannot be sent immediately one after another.
    [Fact]
    public async Task WaitAsync_WhenCalledTwice_DelaysSecondCommand()
    {
        // Arrange
        var rateLimiter = new RobotCommandRateLimiter();
        var stopwatch = Stopwatch.StartNew();

        // Act
        await rateLimiter.WaitAsync();
        await rateLimiter.WaitAsync();

        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds >= 180);
    }
}