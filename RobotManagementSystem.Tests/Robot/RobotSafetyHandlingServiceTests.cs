using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using RobotManagementSystem.Services;
using RobotManagementSystem.Services.MissionLogs;
using RobotManagementSystem.Shared.Models.MissionLog;
using RobotManagementSystem.Shared.Models.Robot;
using RobotManagementSystem.Shared.Models.Users;
using RobotManagementSystem.Tests.Helpers;

namespace RobotManagementSystem.Tests.Robot;

public class RobotSafetyHandlingServiceTests
{
    
    // Unit test: verifies that a valid move command to a free map tile
    // calls the global robot command rate limiter before sending the /api/move request.
    [Fact]
    public async Task MoveRobotAsync_ValidMove_CallsRateLimiter()
    {
        // Arrange
        var mapJson = """
        {
          "width": 21,
          "height": 21,
          "grid": [
            [0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],
            [0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],
            [0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]
          ]
        }
        """;

        var handler = new MockHttpMessageHandler(mapJson, HttpStatusCode.OK);

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost")
        };

        var logger = Mock.Of<ILogger<RobotApiService>>();

        var missionLogsService = new Mock<IMissionLogsService>();
        missionLogsService
            .Setup(x => x.AddMissionLog(It.IsAny<AddMissionLogRequest>()))
            .ReturnsAsync(new MissionLog());

        var rateLimiter = new Mock<IRobotCommandRateLimiter>();
        rateLimiter
            .Setup(x => x.WaitAsync())
            .Returns(Task.CompletedTask);

        var service = new RobotApiService(
            httpClient,
            logger,
            missionLogsService.Object,
            rateLimiter.Object
        );

        var request = new RobotNavigationRequest
        {
            X = 2,
            Y = 1
        };

        // Act
        var result = await service.MoveRobotAsync(request, userId: 1, role: UserRole.Commander);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);

        rateLimiter.Verify(x => x.WaitAsync(), Times.Once);

        Assert.NotNull(handler.LastSentRequest);
        Assert.Equal(HttpMethod.Post, handler.LastSentRequest.Method);
        Assert.Equal("/api/move", handler.LastSentRequest.RequestUri!.AbsolutePath);
        
        // Test that mock mission log is created 
        missionLogsService.Verify(x => x.AddMissionLog(
            It.Is<AddMissionLogRequest>(log =>
                log.Command == RobotCommand.Move &&
                log.CommandResult == RobotCommandResult.Success
            )), Times.Once);
    }

    // Unit test: verifies that a move command targeting an obstacle is blocked,
    // logs the obstacle result, and does not call the rate limiter because no robot command is sent.
    [Fact]
    public async Task MoveRobotAsync_BlockedByObstacle_DoesNotCallRateLimiter()
    {
        // Arrange
        // Lecturer said that the map uses 21x21 in the workshop when he presented the robot simulation API but I am trying to use 3x3 here just to test that dynamic map size is working
        var mapJson = """
        {
          "width": 3,
          "height": 3,
          "grid": [
            [0,0,0],
            [0,1,0],
            [0,0,0]
          ]
        }
        """;

        var handler = new MockHttpMessageHandler(mapJson, HttpStatusCode.OK);

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost")
        };

        var logger = Mock.Of<ILogger<RobotApiService>>();

        var missionLogsService = new Mock<IMissionLogsService>();
        missionLogsService
            .Setup(x => x.AddMissionLog(It.IsAny<AddMissionLogRequest>()))
            .ReturnsAsync(new MissionLog());

        var rateLimiter = new Mock<IRobotCommandRateLimiter>();
        rateLimiter
            .Setup(x => x.WaitAsync())
            .Returns(Task.CompletedTask);

        var service = new RobotApiService(
            httpClient,
            logger,
            missionLogsService.Object,
            rateLimiter.Object
        );

        var request = new RobotNavigationRequest
        {
            X = 1,
            Y = 1
        };

        // Act
        var result = await service.MoveRobotAsync(request, userId: 1, role: UserRole.Commander);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);

        rateLimiter.Verify(x => x.WaitAsync(), Times.Never);

        missionLogsService.Verify(x => x.AddMissionLog(
            It.Is<AddMissionLogRequest>(log =>
                log.Command == RobotCommand.Move &&
                log.CommandResult == RobotCommandResult.BlockedByObstacle
            )), Times.Once);
    }
    
    // Unit test: verifies that invalid coordinates are rejected before sending a robot command,
// logs InvalidCoordinates, and does not call the rate limiter.
    [Fact]
    public async Task MoveRobotAsync_InvalidCoordinates_ReturnsFailureAndDoesNotCallRateLimiter()
    {
        // Arrange
        var mapJson = """
                      {
                        "width": 3,
                        "height": 3,
                        "grid": [
                          [0,0,0],
                          [0,0,0],
                          [0,0,0]
                        ]
                      }
                      """;

        var handler = new MockHttpMessageHandler(mapJson, HttpStatusCode.OK);

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost")
        };

        var logger = Mock.Of<ILogger<RobotApiService>>();

        var missionLogsService = new Mock<IMissionLogsService>();
        missionLogsService
            .Setup(x => x.AddMissionLog(It.IsAny<AddMissionLogRequest>()))
            .ReturnsAsync(new MissionLog());

        var rateLimiter = new Mock<IRobotCommandRateLimiter>();
        rateLimiter
            .Setup(x => x.WaitAsync())
            .Returns(Task.CompletedTask);

        var service = new RobotApiService(
            httpClient,
            logger,
            missionLogsService.Object,
            rateLimiter.Object
        );

        var request = new RobotNavigationRequest
        {
            X = 5,
            Y = 1
        };

        // Act
        var result = await service.MoveRobotAsync(request, userId: 1, role: UserRole.Commander);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);

        rateLimiter.Verify(x => x.WaitAsync(), Times.Never);

        missionLogsService.Verify(x => x.AddMissionLog(
            It.Is<AddMissionLogRequest>(log =>
                log.Command == RobotCommand.Move &&
                log.CommandResult == RobotCommandResult.InvalidCoordinates
            )), Times.Once);
    }
    
    // Unit test: verifies that movement fails safely when the map cannot be retrieved,
// logs the failure, and does not call the rate limiter.
    [Fact]
    public async Task MoveRobotAsync_WhenMapUnavailable_ReturnsFailureAndDoesNotCallRateLimiter()
    {
        // Arrange
        var handler = new MockHttpMessageHandler(string.Empty, HttpStatusCode.InternalServerError);

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost")
        };

        var logger = Mock.Of<ILogger<RobotApiService>>();

        var missionLogsService = new Mock<IMissionLogsService>();
        missionLogsService
            .Setup(x => x.AddMissionLog(It.IsAny<AddMissionLogRequest>()))
            .ReturnsAsync(new MissionLog());

        var rateLimiter = new Mock<IRobotCommandRateLimiter>();
        rateLimiter
            .Setup(x => x.WaitAsync())
            .Returns(Task.CompletedTask);

        var service = new RobotApiService(
            httpClient,
            logger,
            missionLogsService.Object,
            rateLimiter.Object
        );

        var request = new RobotNavigationRequest
        {
            X = 1,
            Y = 1
        };

        // Act
        var result = await service.MoveRobotAsync(request, userId: 1, role: UserRole.Commander);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);

        rateLimiter.Verify(x => x.WaitAsync(), Times.Never);

        missionLogsService.Verify(x => x.AddMissionLog(
            It.Is<AddMissionLogRequest>(log =>
                log.Command == RobotCommand.Move &&
                log.CommandResult == RobotCommandResult.Failure
            )), Times.Once);
    }
    
    // Unit test: verifies that reset commands also use the global robot command rate limiter
// before sending the /api/reset request to the robot API.
    [Fact]
    public async Task ResetAsync_ValidReset_CallsRateLimiter()
    {
        // Arrange
        var handler = new MockHttpMessageHandler(string.Empty, HttpStatusCode.OK);

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost")
        };

        var logger = Mock.Of<ILogger<RobotApiService>>();

        var missionLogsService = new Mock<IMissionLogsService>();
        missionLogsService
            .Setup(x => x.AddMissionLog(It.IsAny<AddMissionLogRequest>()))
            .ReturnsAsync(new MissionLog());

        var rateLimiter = new Mock<IRobotCommandRateLimiter>();
        rateLimiter
            .Setup(x => x.WaitAsync())
            .Returns(Task.CompletedTask);

        var service = new RobotApiService(
            httpClient,
            logger,
            missionLogsService.Object,
            rateLimiter.Object
        );

        // Act
        var result = await service.ResetAsync(userId: 1, userRole: UserRole.Commander);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);

        rateLimiter.Verify(x => x.WaitAsync(), Times.Once);

        Assert.NotNull(handler.LastSentRequest);
        Assert.Equal(HttpMethod.Post, handler.LastSentRequest.Method);
        Assert.Equal("/api/reset", handler.LastSentRequest.RequestUri!.AbsolutePath);

        missionLogsService.Verify(x => x.AddMissionLog(
            It.Is<AddMissionLogRequest>(log =>
                log.Command == RobotCommand.Reset &&
                log.CommandResult == RobotCommandResult.Success
            )), Times.Once);
    }
}