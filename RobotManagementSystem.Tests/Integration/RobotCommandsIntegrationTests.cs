using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RobotManagementSystem.Services;
using RobotManagementSystem.Shared.Models.Map;
using RobotManagementSystem.Shared.Models.Robot;
using RobotManagementSystem.Shared.Models.Users;
using RobotManagementSystem.Tests.RBAC;

namespace RobotManagementSystem.Tests.Integration;

public class RobotCommandsIntegrationTests
{
    // Integration Test: verifies that unauthenticated users cannot send robot move commands.
    // This confirms protected robot control endpoints return HTTP 401 when no authenticated identity is provided.
    [Fact]
    public async Task MoveEndpoint_WhenUnauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var fakeRobotApiService = new FakeRobotApiService();
        var fakeRobotStatusService = new FakeRobotStatusService();

        await using var factory = new RobotCommandsWebApplicationFactory(
            fakeRobotApiService,
            fakeRobotStatusService);

        var client = factory.CreateClient();

        var request = new RobotNavigationRequest
        {
            X = 1,
            Y = 1
        };

        // Act
        var response = await client.PostAsJsonAsync("/robot/commands/move", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // Integration Test: verifies that Viewers cannot send robot move commands.
    // This confirms read-only users are blocked from controlling the robot with HTTP 403.
    [Fact]
    public async Task MoveEndpoint_WhenUserIsViewer_ReturnsForbidden()
    {
        // Arrange
        var fakeRobotApiService = new FakeRobotApiService();
        var fakeRobotStatusService = new FakeRobotStatusService();

        await using var factory = new RobotCommandsWebApplicationFactory(
            fakeRobotApiService,
            fakeRobotStatusService);

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", UserRole.Viewer.ToString());

        var request = new RobotNavigationRequest
        {
            X = 1,
            Y = 1
        };

        // Act
        var response = await client.PostAsJsonAsync("/robot/commands/move", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // Integration Test: verifies that Commanders can send valid robot move commands.
    // This confirms the REST endpoint exposes robot movement and returns HTTP 200 for authorised valid commands.
    [Fact]
    public async Task MoveEndpoint_WhenUserIsCommanderAndCommandIsValid_ReturnsOk()
    {
        // Arrange
        var fakeRobotApiService = new FakeRobotApiService
        {
            MoveResponse = new RobotCommandResponse
            {
                Success = true,
                Message = "Robot moved successfully.",
                RobotPosition = new RobotManagementSystem.Shared.Models.Components.Vector2D
                {
                    X = 1,
                    Y = 1
                }
            }
        };

        var fakeRobotStatusService = new FakeRobotStatusService();

        await using var factory = new RobotCommandsWebApplicationFactory(
            fakeRobotApiService,
            fakeRobotStatusService);

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", UserRole.Commander.ToString());

        var request = new RobotNavigationRequest
        {
            X = 1,
            Y = 1
        };

        // Act
        var response = await client.PostAsJsonAsync("/robot/commands/move", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<RobotCommandResponse>();

        Assert.NotNull(body);
        Assert.True(body.Success);
        Assert.Equal(1, body.RobotPosition?.X);
        Assert.Equal(1, body.RobotPosition?.Y);
    }

    // Integration Test: verifies that invalid robot move commands return HTTP 400.
    // This confirms validation failures are treated as client errors rather than robot API outages.
    [Fact]
    public async Task MoveEndpoint_WhenCommandIsInvalid_ReturnsBadRequest()
    {
        // Arrange
        var fakeRobotApiService = new FakeRobotApiService
        {
            MoveResponse = new RobotCommandResponse
            {
                Success = false,
                Message = "Coordinates are not valid."
            }
        };

        var fakeRobotStatusService = new FakeRobotStatusService();

        await using var factory = new RobotCommandsWebApplicationFactory(
            fakeRobotApiService,
            fakeRobotStatusService);

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", UserRole.Commander.ToString());

        var request = new RobotNavigationRequest
        {
            X = 99,
            Y = 99
        };

        // Act
        var response = await client.PostAsJsonAsync("/robot/commands/move", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // Integration Test: verifies that robot API failure returns HTTP 503 from the move endpoint.
    // This confirms unreachable robot API states are exposed as server-side availability failures.
    [Fact]
    public async Task MoveEndpoint_WhenRobotApiUnavailable_ReturnsServiceUnavailable()
    {
        // Arrange
        var fakeRobotApiService = new FakeRobotApiService
        {
            MoveResponse = null
        };

        var fakeRobotStatusService = new FakeRobotStatusService();

        await using var factory = new RobotCommandsWebApplicationFactory(
            fakeRobotApiService,
            fakeRobotStatusService);

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", UserRole.Commander.ToString());

        var request = new RobotNavigationRequest
        {
            X = 1,
            Y = 1
        };

        // Act
        var response = await client.PostAsJsonAsync("/robot/commands/move", request);

        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    // Integration Test: verifies that Commanders can send reset commands through the REST API.
    // This confirms the reset endpoint is exposed and returns HTTP 200 when the robot API succeeds.
    [Fact]
    public async Task ResetEndpoint_WhenUserIsCommanderAndRobotApiSucceeds_ReturnsOk()
    {
        // Arrange
        var fakeRobotApiService = new FakeRobotApiService
        {
            ResetResponse = new RobotCommandResponse
            {
                Success = true,
                Message = "Robot reset successfully."
            }
        };

        var fakeRobotStatusService = new FakeRobotStatusService();

        await using var factory = new RobotCommandsWebApplicationFactory(
            fakeRobotApiService,
            fakeRobotStatusService);

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", UserRole.Commander.ToString());

        // Act
        var response = await client.PostAsync("/robot/commands/reset", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<RobotCommandResponse>();

        Assert.NotNull(body);
        Assert.True(body.Success);
    }

    // Integration Test: verifies that robot status failures return HTTP 503.
    // This confirms the backend does not crash when current robot state cannot be retrieved.
    [Fact]
    public async Task StatusEndpoint_WhenRobotApiUnavailable_ReturnsServiceUnavailable()
    {
        // Arrange
        var fakeRobotApiService = new FakeRobotApiService();

        var fakeRobotStatusService = new FakeRobotStatusService
        {
            StatusResponse = null
        };

        await using var factory = new RobotCommandsWebApplicationFactory(
            fakeRobotApiService,
            fakeRobotStatusService);

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", UserRole.Commander.ToString());

        // Act
        var response = await client.GetAsync("/robot/commands/status");

        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    private sealed class RobotCommandsWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly IRobotApiService _robotApiService;
        private readonly IRobotStatusService _robotStatusService;

        public RobotCommandsWebApplicationFactory(
            IRobotApiService robotApiService,
            IRobotStatusService robotStatusService)
        {
            _robotApiService = robotApiService;
            _robotStatusService = robotStatusService;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services
                    .AddAuthentication(TestAuthHandler.SchemeName)
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        TestAuthHandler.SchemeName,
                        options => { });

                services.PostConfigure<AuthenticationOptions>(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                    options.DefaultScheme = TestAuthHandler.SchemeName;
                });

                services.RemoveAll<IRobotApiService>();
                services.RemoveAll<IRobotStatusService>();

                services.AddSingleton(_robotApiService);
                services.AddSingleton(_robotStatusService);
            });
        }
    }

    private sealed class FakeRobotApiService : IRobotApiService
    {
        public RobotCommandResponse? MoveResponse { get; set; } = new()
        {
            Success = true,
            Message = "Robot moved successfully."
        };

        public RobotCommandResponse? ResetResponse { get; set; } = new()
        {
            Success = true,
            Message = "Robot reset successfully."
        };

        public Task<MapResponse?> GetMapAsync()
        {
            return Task.FromResult<MapResponse?>(new MapResponse
            {
                Width = 21,
                Height = 21,
                Grid =
                [
                    [0, 0],
                    [0, 0]
                ]
            });
        }

        public Task<RobotCommandResponse?> MoveRobotAsync(
            RobotNavigationRequest request,
            int userId,
            UserRole userRole)
        {
            return Task.FromResult(MoveResponse);
        }

        public Task<RobotCommandResponse?> ResetAsync(int userId, UserRole userRole)
        {
            return Task.FromResult(ResetResponse);
        }
    }

    private sealed class FakeRobotStatusService : IRobotStatusService
    {
        public RobotStatusResponse? StatusResponse { get; set; } = new()
        {
            Position = new RobotManagementSystem.Shared.Models.Components.Vector2D
            {
                X = 0,
                Y = 0
            },
            Battery = 100,
            Status = "IDLE"
        };

        public Task<RobotStatusResponse?> GetRobotStatusAsync()
        {
            return Task.FromResult(StatusResponse);
        }
    }
}