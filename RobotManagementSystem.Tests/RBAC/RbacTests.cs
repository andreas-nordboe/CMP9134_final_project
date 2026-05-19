using System.Net;
using System.Net.Http.Json;
using RobotManagementSystem.Shared.Models.Robot;
using RobotManagementSystem.Shared.Models.Users;

namespace RobotManagementSystem.Tests.RBAC;

public class RbacTests : IClassFixture<RbacWebApplicationFactory>
{
    private readonly RbacWebApplicationFactory _factory;

    public RbacTests(RbacWebApplicationFactory factory)
    {
        _factory = factory;
    }

    // Integration test: verifies that unauthenticated users cannot access protected endpoints,
    // ensuring private user-management data is not exposed.
    [Fact]
    public async Task ProtectedEndpoint_WhenUserIsUnauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/users/all");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // Integration test: verifies that Viewers cannot access admin user-management endpoints,
    // enforcing read-only access for non-admin users.
    [Fact]
    public async Task AdminEndpoint_WhenUserIsViewer_ReturnsForbidden()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", UserRole.Viewer.ToString());

        // Act
        var response = await client.GetAsync("/users/all");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // Integration test: verifies that Commanders can access robot command endpoints,
    // allowing authorised operators to control the robot.
    [Fact]
    public async Task MoveEndpoint_WhenUserIsCommander_DoesNotReturnUnauthorizedOrForbidden()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", UserRole.Commander.ToString());

        var request = new RobotNavigationRequest
        {
            X = 1,
            Y = 1
        };

        // Act
        var response = await client.PostAsJsonAsync("/robot/commands/move", request);

        // Assert
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // Integration test: verifies that Viewers cannot send move commands,
    // preventing read-only users from controlling the robot.
    [Fact]
    public async Task MoveEndpoint_WhenUserIsViewer_ReturnsForbidden()
    {
        // Arrange
        var client = _factory.CreateClient();
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

    // Integration test: verifies that users with NoRole cannot view robot telemetry until approved,
    // ensuring newly registered users do not automatically gain operational access.
    [Fact]
    public async Task RobotTelemetryEndpoint_WhenUserHasNoRole_ReturnsForbidden()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", UserRole.NoRole.ToString());

        // Act
        var response = await client.GetAsync("/robot/commands/status");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}