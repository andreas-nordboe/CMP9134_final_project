using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using RobotManagementSystem.Hubs;
using RobotManagementSystem.Services;
using RobotManagementSystem.Services.System;
using RobotManagementSystem.Services.SystemStatus;
using RobotManagementSystem.Shared.Models.Robot;
using RobotManagementSystem.Tests.Helpers;
using Xunit;

namespace RobotManagementSystem.Tests.Robot;

public class RobotStatusServiceTests
{
    private readonly Mock<ILogger<RobotStatusService>> _loggerMock = new();
    private readonly Mock<IRobotApiStatusStore> _statusStoreMock = new();
    private readonly Mock<IHubContext<RobotTelemetryHub>> _hubContextMock = new();
    private readonly Mock<IHubClients> _hubClientsMock = new();
    private readonly Mock<IClientProxy> _clientProxyMock = new();
    private readonly Mock<ISystemStatusLogService> _systemStatusLogServiceMock = new();

    public RobotStatusServiceTests()
    {
        _hubClientsMock
            .Setup(clients => clients.All)
            .Returns(_clientProxyMock.Object);

        _hubContextMock
            .Setup(context => context.Clients)
            .Returns(_hubClientsMock.Object);

        _statusStoreMock.SetupProperty(store => store.CurrentApiStatus, RobotApiStatus.Disconnected);
        _statusStoreMock.SetupProperty(store => store.LastLatencyMs);
        _statusStoreMock.SetupProperty(store => store.LastRobotState);
        _statusStoreMock.SetupProperty(store => store.LastSnapshotLoggedAt, DateTime.MinValue);

        _statusStoreMock
            .Setup(store => store.RecentLatenciesMs)
            .Returns(new List<double>());
    }

    // Unit test: verifies that a successful robot API response is returned,
    // sets the connection state to Connected, logs it, and broadcasts it to clients.
    [Fact]
    public async Task GetRobotStatusAsync_WhenRobotApiReturnsSuccess_ReturnsStatusAndSetsConnected()
    {
        // Arrange
        var robotStatus = new RobotStatusResponse
        {
            Status = RobotStatus.IDLE.ToString()
        };

        var handler = CreateHandler(HttpStatusCode.OK, robotStatus);
        var httpClient = CreateHttpClient(handler);
        var service = CreateService(httpClient);

        // Act
        var result = await service.GetRobotStatusAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(RobotStatus.IDLE.ToString(), result.Status);

        Assert.Equal(HttpMethod.Get, handler.LastSentRequest!.Method);
        Assert.Equal("/status", handler.LastSentRequest.RequestUri!.AbsolutePath);

        Assert.Equal(RobotApiStatus.Connected, _statusStoreMock.Object.CurrentApiStatus);

        _systemStatusLogServiceMock.Verify(service =>
            service.LogConnectionChangedAsync(RobotApiStatus.Connected),
            Times.Once);

        _clientProxyMock.Verify(client =>
                client.SendCoreAsync(
                    RobotApiStatus.StatusMethod,
                    It.Is<object[]>(args =>
                        args.Length == 1 &&
                        (string)args[0] == RobotApiStatus.Connected),
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // Unit test: verifies that a 503 Service Unavailable response is handled safely,
    // returning null and setting/broadcasting the API state as Reconnecting.
    [Fact]
    public async Task GetRobotStatusAsync_WhenRobotApiReturnsServiceUnavailable_ReturnsNullAndSetsReconnecting()
    {
        // Arrange
        var handler = new MockHttpMessageHandler(string.Empty, HttpStatusCode.ServiceUnavailable);
        var httpClient = CreateHttpClient(handler);
        var service = CreateService(httpClient);

        // Act
        var result = await service.GetRobotStatusAsync();

        // Assert
        Assert.Null(result);

        Assert.Equal(HttpMethod.Get, handler.LastSentRequest!.Method);
        Assert.Equal("/status", handler.LastSentRequest.RequestUri!.AbsolutePath);

        Assert.Equal(RobotApiStatus.Reconnecting, _statusStoreMock.Object.CurrentApiStatus);

        _systemStatusLogServiceMock.Verify(service =>
            service.LogConnectionChangedAsync(RobotApiStatus.Reconnecting),
            Times.Once);

        _clientProxyMock.Verify(client =>
                client.SendCoreAsync(
                    RobotApiStatus.StatusMethod,
                    It.Is<object[]>(args =>
                        args.Length == 1 &&
                        (string)args[0] == RobotApiStatus.Reconnecting),
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // Unit test: verifies that a failed robot API response, such as HTTP 500,
    // returns null and changes the API state to Reconnecting.
    [Fact]
    public async Task GetRobotStatusAsync_WhenRobotApiReturnsFailureStatus_ReturnsNullAndSetsReconnecting()
    {
        // Arrange
        var handler = new MockHttpMessageHandler(string.Empty, HttpStatusCode.InternalServerError);
        var httpClient = CreateHttpClient(handler);
        var service = CreateService(httpClient);

        // Act
        var result = await service.GetRobotStatusAsync();

        // Assert
        Assert.Null(result);

        Assert.Equal(HttpMethod.Get, handler.LastSentRequest!.Method);
        Assert.Equal("/status", handler.LastSentRequest.RequestUri!.AbsolutePath);

        Assert.Equal(RobotApiStatus.Reconnecting, _statusStoreMock.Object.CurrentApiStatus);

        _systemStatusLogServiceMock.Verify(service =>
            service.LogConnectionChangedAsync(RobotApiStatus.Reconnecting),
            Times.Once);

        _clientProxyMock.Verify(client =>
                client.SendCoreAsync(
                    RobotApiStatus.StatusMethod,
                    It.Is<object[]>(args =>
                        args.Length == 1 &&
                        (string)args[0] == RobotApiStatus.Reconnecting),
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // Unit test: verifies that a robot state transition, such as IDLE to MOVING,
    // is logged and stored as the latest robot state.
    [Fact]
    public async Task GetRobotStatusAsync_WhenRobotStateChanges_LogsRobotStatusChanged()
    {
        // Arrange
        _statusStoreMock.Object.LastRobotState = RobotStatus.IDLE.ToString();

        var robotStatus = new RobotStatusResponse
        {
            Status = RobotStatus.MOVING.ToString()
        };

        var handler = CreateHandler(HttpStatusCode.OK, robotStatus);
        var httpClient = CreateHttpClient(handler);
        var service = CreateService(httpClient);

        // Act
        var result = await service.GetRobotStatusAsync();

        // Assert
        Assert.NotNull(result);

        _systemStatusLogServiceMock.Verify(service =>
            service.LogRobotStatusChangedAsync(
                It.Is<RobotStatusResponse>(status =>
                    status.Status == RobotStatus.MOVING.ToString())),
            Times.Once);

        Assert.Equal(RobotStatus.MOVING.ToString(), _statusStoreMock.Object.LastRobotState);
    }

    // Unit test: verifies that unchanged robot states are not logged repeatedly,
    // preventing duplicate/noisy system status log entries.
    [Fact]
    public async Task GetRobotStatusAsync_WhenRobotStateHasNotChanged_DoesNotLogRobotStatusChanged()
    {
        // Arrange
        _statusStoreMock.Object.LastRobotState = RobotStatus.IDLE.ToString();

        var robotStatus = new RobotStatusResponse
        {
            Status = RobotStatus.IDLE.ToString()
        };

        var handler = CreateHandler(HttpStatusCode.OK, robotStatus);
        var httpClient = CreateHttpClient(handler);
        var service = CreateService(httpClient);

        // Act
        var result = await service.GetRobotStatusAsync();

        // Assert
        Assert.NotNull(result);

        _systemStatusLogServiceMock.Verify(service =>
            service.LogRobotStatusChangedAsync(It.IsAny<RobotStatusResponse>()),
            Times.Never);
    }

    // Unit test: verifies that a telemetry snapshot is logged when the configured
    // snapshot interval has passed.
    [Fact]
    public async Task GetRobotStatusAsync_WhenSnapshotIntervalHasPassed_LogsTelemetrySnapshot()
    {
        // Arrange
        _statusStoreMock.Object.LastSnapshotLoggedAt = DateTime.UtcNow.AddSeconds(-20);

        var robotStatus = new RobotStatusResponse
        {
            Status = RobotStatus.IDLE.ToString()
        };

        var handler = CreateHandler(HttpStatusCode.OK, robotStatus);
        var httpClient = CreateHttpClient(handler);
        var service = CreateService(httpClient);

        // Act
        var result = await service.GetRobotStatusAsync();

        // Assert
        Assert.NotNull(result);

        _systemStatusLogServiceMock.Verify(service =>
            service.LogTelemetrySnapshotAsync(
                It.Is<RobotStatusResponse>(status =>
                    status.Status == RobotStatus.IDLE.ToString())),
            Times.Once);
    }

    // Unit test: verifies that telemetry snapshots are not logged too frequently
    // when the previous snapshot was recent.
    [Fact]
    public async Task GetRobotStatusAsync_WhenSnapshotIntervalHasNotPassed_DoesNotLogTelemetrySnapshot()
    {
        // Arrange
        _statusStoreMock.Object.LastSnapshotLoggedAt = DateTime.UtcNow;

        var robotStatus = new RobotStatusResponse
        {
            Status = RobotStatus.IDLE.ToString()
        };

        var handler = CreateHandler(HttpStatusCode.OK, robotStatus);
        var httpClient = CreateHttpClient(handler);
        var service = CreateService(httpClient);

        // Act
        var result = await service.GetRobotStatusAsync();

        // Assert
        Assert.NotNull(result);

        _systemStatusLogServiceMock.Verify(service =>
            service.LogTelemetrySnapshotAsync(It.IsAny<RobotStatusResponse>()),
            Times.Never);
    }

    // Unit test: verifies that only the most recent 20 latency values are kept,
    // preventing the in-memory latency history from growing indefinitely.
    [Fact]
    public async Task GetRobotStatusAsync_WhenCalledMoreThanTwentyTimes_KeepsOnlyTwentyRecentLatencies()
    {
        // Arrange
        var robotStatus = new RobotStatusResponse
        {
            Status = RobotStatus.IDLE.ToString()
        };

        var handler = CreateHandler(HttpStatusCode.OK, robotStatus);
        var httpClient = CreateHttpClient(handler);
        var service = CreateService(httpClient);

        // Act
        for (var i = 0; i < 25; i++)
        {
            await service.GetRobotStatusAsync();
        }

        // Assert
        Assert.Equal(20, _statusStoreMock.Object.RecentLatenciesMs.Count);
    }

    // Unit test: verifies that the same API status is not logged or broadcast again,
    // preventing duplicate SignalR updates and repeated connection log entries.
    [Fact]
    public async Task GetRobotStatusAsync_WhenApiStatusAlreadySame_DoesNotRebroadcastStatus()
    {
        // Arrange
        _statusStoreMock.Object.CurrentApiStatus = RobotApiStatus.Connected;

        var robotStatus = new RobotStatusResponse
        {
            Status = RobotStatus.IDLE.ToString()
        };

        var handler = CreateHandler(HttpStatusCode.OK, robotStatus);
        var httpClient = CreateHttpClient(handler);
        var service = CreateService(httpClient);

        // Act
        var result = await service.GetRobotStatusAsync();

        // Assert
        Assert.NotNull(result);

        _systemStatusLogServiceMock.Verify(service =>
            service.LogConnectionChangedAsync(It.IsAny<string>()),
            Times.Never);

        _clientProxyMock.Verify(client =>
            client.SendCoreAsync(
                RobotApiStatus.StatusMethod,
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // Unit test: verifies that malformed JSON from the robot API is handled safely,
    // returning null and setting/broadcasting the API state as Reconnecting.
    [Fact]
    public async Task GetRobotStatusAsync_WhenRobotApiReturnsInvalidJson_ReturnsNullAndSetsReconnecting()
    {
        // Arrange
        var handler = new MockHttpMessageHandler("{ invalid json", HttpStatusCode.OK);
        var httpClient = CreateHttpClient(handler);
        var service = CreateService(httpClient);

        // Act
        var result = await service.GetRobotStatusAsync();

        // Assert
        Assert.Null(result);

        Assert.Equal(RobotApiStatus.Reconnecting, _statusStoreMock.Object.CurrentApiStatus);

        _systemStatusLogServiceMock.Verify(service =>
                service.LogConnectionChangedAsync(RobotApiStatus.Reconnecting),
            Times.Once);

        _clientProxyMock.Verify(client =>
                client.SendCoreAsync(
                    RobotApiStatus.StatusMethod,
                    It.Is<object[]>(args =>
                        args.Length == 1 &&
                        (string)args[0] == RobotApiStatus.Reconnecting),
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private RobotStatusService CreateService(HttpClient httpClient)
    {
        return new RobotStatusService(
            _loggerMock.Object,
            httpClient,
            _statusStoreMock.Object,
            _hubContextMock.Object,
            _systemStatusLogServiceMock.Object);
    }

    private static HttpClient CreateHttpClient(MockHttpMessageHandler handler)
    {
        return new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost")
        };
    }

    private static MockHttpMessageHandler CreateHandler<TBody>(
        HttpStatusCode statusCode,
        TBody body)
    {
        var json = JsonSerializer.Serialize(body);

        return new MockHttpMessageHandler(json, statusCode);
    }
}