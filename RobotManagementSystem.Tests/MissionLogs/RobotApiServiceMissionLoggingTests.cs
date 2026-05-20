using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using RobotManagementSystem.Services;
using RobotManagementSystem.Services.MissionLogs;
using RobotManagementSystem.Shared.Models.MissionLog;
using RobotManagementSystem.Shared.Models.Robot;
using RobotManagementSystem.Shared.Models.Users;

namespace RobotManagementSystem.Tests.MissionLogs;

public class RobotApiServiceMissionLoggingTests
{
    // Unit Test: verifies that a successful move command creates a success mission log.
    // This confirms the audit trail records authorised robot movement activity.
    [Fact]
    public async Task MoveRobotAsync_WhenMoveSucceeds_AddsSuccessMissionLog()
    {
        // Arrange
        var handler = new SequentialRobotHttpMessageHandler([
            new ExpectedRobotResponse(HttpMethod.Get, "/api/map", HttpStatusCode.OK, ValidEmptyMapJson),
            new ExpectedRobotResponse(HttpMethod.Post, "/api/move", HttpStatusCode.OK, "{}")
        ]);

        var missionLogsService = CreateMissionLogMock();
        var service = CreateRobotApiService(handler, missionLogsService);

        var request = new RobotNavigationRequest
        {
            X = 1,
            Y = 1
        };

        // Act
        var result = await service.MoveRobotAsync(request, 1, UserRole.Commander);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(1, result.RobotPosition?.X);
        Assert.Equal(1, result.RobotPosition?.Y);

        missionLogsService.Verify(x => x.AddMissionLog(It.Is<AddMissionLogRequest>(log =>
            log.UserId == 1 &&
            log.Role == UserRole.Commander &&
            log.Command == RobotCommand.Move &&
            log.CommandResult == RobotCommandResult.Success
        )), Times.Once);
    }

    // Unit Test: verifies that invalid coordinates create an invalid-coordinate mission log.
    // This prevents unsafe movement requests from disappearing without an audit record.
    [Fact]
    public async Task MoveRobotAsync_WhenCoordinatesAreOutOfBounds_AddsInvalidCoordinateMissionLog()
    {
        // Arrange
        var handler = new SequentialRobotHttpMessageHandler([
            new ExpectedRobotResponse(HttpMethod.Get, "/api/map", HttpStatusCode.OK, ValidEmptyMapJson)
        ]);

        var missionLogsService = CreateMissionLogMock();
        var service = CreateRobotApiService(handler, missionLogsService);

        var request = new RobotNavigationRequest
        {
            X = 21,
            Y = 1
        };

        // Act
        var result = await service.MoveRobotAsync(request, 1, UserRole.Commander);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);

        missionLogsService.Verify(x => x.AddMissionLog(It.Is<AddMissionLogRequest>(log =>
            log.UserId == 1 &&
            log.Role == UserRole.Commander &&
            log.Command == RobotCommand.Move &&
            log.CommandResult == RobotCommandResult.InvalidCoordinates
        )), Times.Once);

        Assert.DoesNotContain(handler.SentRequests, requestMessage =>
            requestMessage.Method == HttpMethod.Post &&
            requestMessage.RequestUri?.AbsolutePath == "/api/move");
    }

    // Unit Test: verifies that obstacle movement attempts create a blocked-by-obstacle mission log.
    // This confirms unsafe path choices are rejected and recorded before reaching the robot API.
    [Fact]
    public async Task MoveRobotAsync_WhenTargetTileIsObstacle_AddsBlockedByObstacleMissionLog()
    {
        // Arrange
        var handler = new SequentialRobotHttpMessageHandler([
            new ExpectedRobotResponse(HttpMethod.Get, "/api/map", HttpStatusCode.OK, MapWithObstacleJson)
        ]);

        var missionLogsService = CreateMissionLogMock();
        var service = CreateRobotApiService(handler, missionLogsService);

        var request = new RobotNavigationRequest
        {
            X = 1,
            Y = 1
        };

        // Act
        var result = await service.MoveRobotAsync(request, 1, UserRole.Commander);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);

        missionLogsService.Verify(x => x.AddMissionLog(It.Is<AddMissionLogRequest>(log =>
            log.UserId == 1 &&
            log.Role == UserRole.Commander &&
            log.Command == RobotCommand.Move &&
            log.CommandResult == RobotCommandResult.BlockedByObstacle
        )), Times.Once);

        Assert.DoesNotContain(handler.SentRequests, requestMessage =>
            requestMessage.Method == HttpMethod.Post &&
            requestMessage.RequestUri?.AbsolutePath == "/api/move");
    }

    // Unit Test: verifies that a missing map creates a failed mission log.
    // This confirms movement is not attempted when map validation cannot be completed.
    [Fact]
    public async Task MoveRobotAsync_WhenMapCannotBeLoaded_AddsFailureMissionLog()
    {
        // Arrange
        var handler = new SequentialRobotHttpMessageHandler([
            new ExpectedRobotResponse(HttpMethod.Get, "/api/map", HttpStatusCode.ServiceUnavailable, "")
        ]);

        var missionLogsService = CreateMissionLogMock();
        var service = CreateRobotApiService(handler, missionLogsService);

        var request = new RobotNavigationRequest
        {
            X = 1,
            Y = 1
        };

        // Act
        var result = await service.MoveRobotAsync(request, 1, UserRole.Commander);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);

        missionLogsService.Verify(x => x.AddMissionLog(It.Is<AddMissionLogRequest>(log =>
            log.UserId == 1 &&
            log.Role == UserRole.Commander &&
            log.Command == RobotCommand.Move &&
            log.CommandResult == RobotCommandResult.Failure
        )), Times.Once);

        Assert.DoesNotContain(handler.SentRequests, requestMessage =>
            requestMessage.Method == HttpMethod.Post &&
            requestMessage.RequestUri?.AbsolutePath == "/api/move");
    }

    // Unit Test: verifies that a failed move response creates a failed mission log.
    // This confirms unsuccessful robot API responses are recorded for auditing.
    [Fact]
    public async Task MoveRobotAsync_WhenRobotApiReturnsServerError_AddsFailureMissionLog()
    {
        // Arrange
        var handler = new SequentialRobotHttpMessageHandler([
            new ExpectedRobotResponse(HttpMethod.Get, "/api/map", HttpStatusCode.OK, ValidEmptyMapJson),
            new ExpectedRobotResponse(HttpMethod.Post, "/api/move", HttpStatusCode.InternalServerError, "{}")
        ]);

        var missionLogsService = CreateMissionLogMock();
        var service = CreateRobotApiService(handler, missionLogsService);

        var request = new RobotNavigationRequest
        {
            X = 1,
            Y = 1
        };

        // Act
        var result = await service.MoveRobotAsync(request, 1, UserRole.Commander);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);

        missionLogsService.Verify(x => x.AddMissionLog(It.Is<AddMissionLogRequest>(log =>
            log.UserId == 1 &&
            log.Role == UserRole.Commander &&
            log.Command == RobotCommand.Move &&
            log.CommandResult == RobotCommandResult.Failure
        )), Times.Once);
    }

    // Unit Test: verifies that a successful reset command creates a success mission log.
    // This confirms high-impact robot control actions are included in the audit trail.
    [Fact]
    public async Task ResetAsync_WhenResetSucceeds_AddsSuccessMissionLog()
    {
        // Arrange
        var handler = new SequentialRobotHttpMessageHandler([
            new ExpectedRobotResponse(HttpMethod.Post, "/api/reset", HttpStatusCode.OK, "{}")
        ]);

        var missionLogsService = CreateMissionLogMock();
        var service = CreateRobotApiService(handler, missionLogsService);

        // Act
        var result = await service.ResetAsync(1, UserRole.Commander);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);

        missionLogsService.Verify(x => x.AddMissionLog(It.Is<AddMissionLogRequest>(log =>
            log.UserId == 1 &&
            log.Role == UserRole.Commander &&
            log.Command == RobotCommand.Reset &&
            log.CommandResult == RobotCommandResult.Success
        )), Times.Once);
    }

    // Unit Test: verifies that a failed reset response creates a failed mission log.
    // This confirms reset failures are not hidden from the audit trail.
    [Fact]
    public async Task ResetAsync_WhenRobotApiReturnsServerError_AddsFailureMissionLog()
    {
        // Arrange
        var handler = new SequentialRobotHttpMessageHandler([
            new ExpectedRobotResponse(HttpMethod.Post, "/api/reset", HttpStatusCode.InternalServerError, "{}")
        ]);

        var missionLogsService = CreateMissionLogMock();
        var service = CreateRobotApiService(handler, missionLogsService);

        // Act
        var result = await service.ResetAsync(1, UserRole.Commander);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);

        missionLogsService.Verify(x => x.AddMissionLog(It.Is<AddMissionLogRequest>(log =>
            log.UserId == 1 &&
            log.Role == UserRole.Commander &&
            log.Command == RobotCommand.Reset &&
            log.CommandResult == RobotCommandResult.Failure
        )), Times.Once);
    }

    private static RobotApiService CreateRobotApiService(
        HttpMessageHandler handler,
        Mock<IMissionLogsService> missionLogsService)
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost")
        };

        var logger = new Mock<ILogger<RobotApiService>>();
        var rateLimiter = new Mock<IRobotCommandRateLimiter>();
        var statusStore = new Mock<IRobotApiStatusStore>();

        rateLimiter
            .Setup(x => x.WaitAsync())
            .Returns(Task.CompletedTask);

        return new RobotApiService(
            httpClient,
            logger.Object,
            missionLogsService.Object,
            rateLimiter.Object,
            statusStore.Object
        );
    }

    private static Mock<IMissionLogsService> CreateMissionLogMock()
    {
        var missionLogsService = new Mock<IMissionLogsService>();

        missionLogsService
            .Setup(x => x.AddMissionLog(It.IsAny<AddMissionLogRequest>()))
            .ReturnsAsync((AddMissionLogRequest request) => new MissionLog
            {
                Id = 1,
                UserId = request.UserId,
                Role = request.Role,
                Command = request.Command,
                CommandResult = request.CommandResult,
                Details = request.Details,
                Timestamp = DateTime.UtcNow
            });

        return missionLogsService;
    }

    private const string ValidEmptyMapJson = """
    {
      "width": 21,
      "height": 21,
      "grid": [
        [0,0,0],
        [0,0,0],
        [0,0,0]
      ]
    }
    """;

    private const string MapWithObstacleJson = """
    {
      "width": 21,
      "height": 21,
      "grid": [
        [0,0,0],
        [0,1,0],
        [0,0,0]
      ]
    }
    """;

    private sealed record ExpectedRobotResponse(
        HttpMethod Method,
        string Path,
        HttpStatusCode StatusCode,
        string Body
    );

    private sealed class SequentialRobotHttpMessageHandler : HttpMessageHandler
    {
        private readonly Queue<ExpectedRobotResponse> _responses;

        public List<HttpRequestMessage> SentRequests { get; } = [];

        public SequentialRobotHttpMessageHandler(IEnumerable<ExpectedRobotResponse> responses)
        {
            _responses = new Queue<ExpectedRobotResponse>(responses);
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            SentRequests.Add(request);

            if (_responses.Count == 0)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent("{}", Encoding.UTF8, "application/json")
                });
            }

            var expected = _responses.Dequeue();

            if (request.Method != expected.Method ||
                request.RequestUri?.AbsolutePath != expected.Path)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("{}", Encoding.UTF8, "application/json")
                });
            }

            return Task.FromResult(new HttpResponseMessage(expected.StatusCode)
            {
                Content = new StringContent(expected.Body, Encoding.UTF8, "application/json")
            });
        }
    }
}