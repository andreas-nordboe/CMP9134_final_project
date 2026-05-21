using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using RobotManagementSystem.Data;
using RobotManagementSystem.Services;
using RobotManagementSystem.Services.MissionLogs;
using RobotManagementSystem.Shared.Models.Components;
using RobotManagementSystem.Shared.Models.MissionLog;
using RobotManagementSystem.Shared.Models.Robot;
using RobotManagementSystem.Shared.Models.Users;

namespace RobotManagementSystem.Tests.MissionLogs;

public class MissionLogsServiceTests
{
    // Unit Test: verifies that AddMissionLog saves a mission log when the request is valid.
    // This confirms the audit trail persists the command, role, result, and robot status snapshot.
    [Fact]
    public async Task AddMissionLog_WhenRequestIsValid_SavesMissionLogToDatabase()
    {
        // Arrange
        await using var database = CreateInMemoryDatabase();
        await SeedUserAsync(database.DbContext);

        var robotStatusService = new Mock<IRobotStatusService>();
        robotStatusService
            .Setup(x => x.GetRobotStatusAsync())
            .ReturnsAsync(new RobotStatusResponse
            {
                Position = new Vector2D
                {
                    X = 4,
                    Y = 3
                },
                Battery = 84.5,
                Status = "IDLE"
            });

        var service = new MissionLogsService(database.DbContext, robotStatusService.Object);

        var request = new AddMissionLogRequest
        {
            UserId = 1,
            Role = UserRole.Commander,
            Command = RobotCommand.Move,
            CommandResult = RobotCommandResult.Success,
            Details = "Move command completed."
        };

        // Act
        var result = await service.AddMissionLog(request);

        // Assert
        Assert.NotEqual(0, result.Id);
        Assert.Equal(1, result.UserId);
        Assert.Equal(UserRole.Commander, result.Role);
        Assert.Equal(RobotCommand.Move, result.Command);
        Assert.Equal(RobotCommandResult.Success, result.CommandResult);
        Assert.Equal("Move command completed.", result.Details);
        Assert.Equal(4, result.RobotX);
        Assert.Equal(3, result.RobotY);
        Assert.Equal(84.5, result.Battery);
        Assert.Equal("IDLE", result.ConnectionStatus);

        var savedLog = await database.DbContext.MissionLogs.SingleAsync();

        Assert.Equal(result.Id, savedLog.Id);
        Assert.Equal(RobotCommand.Move, savedLog.Command);
        Assert.Equal(RobotCommandResult.Success, savedLog.CommandResult);
    }

    // Unit Test: verifies that AddMissionLog uses safe fallback robot values when robot status is unavailable.
    // This confirms audit logging still works during robot API connection loss.
    [Fact]
    public async Task AddMissionLog_WhenRobotStatusIsUnavailable_SavesLogWithDisconnectedFallbackValues()
    {
        // Arrange
        await using var database = CreateInMemoryDatabase();
        await SeedUserAsync(database.DbContext);

        var robotStatusService = new Mock<IRobotStatusService>();
        robotStatusService
            .Setup(x => x.GetRobotStatusAsync())
            .ReturnsAsync((RobotStatusResponse?)null);

        var service = new MissionLogsService(database.DbContext, robotStatusService.Object);

        var request = new AddMissionLogRequest
        {
            UserId = 1,
            Role = UserRole.Commander,
            Command = RobotCommand.Reset,
            CommandResult = RobotCommandResult.Failure,
            Details = "Robot API unavailable."
        };

        // Act
        var result = await service.AddMissionLog(request);

        // Assert
        Assert.NotEqual(0, result.Id);
        Assert.Equal(0, result.RobotX);
        Assert.Equal(0, result.RobotY);
        Assert.Equal(0, result.Battery);
        Assert.Equal("Disconnected", result.ConnectionStatus);

        var savedLog = await database.DbContext.MissionLogs.SingleAsync();

        Assert.Equal(RobotCommand.Reset, savedLog.Command);
        Assert.Equal(RobotCommandResult.Failure, savedLog.CommandResult);
        Assert.Equal("Disconnected", savedLog.ConnectionStatus);
    }

    // Unit Test: verifies that AddMissionLog rejects logs for users that do not exist.
    // This prevents orphaned audit entries from being created without a valid user account.
    [Fact]
    public async Task AddMissionLog_WhenUserDoesNotExist_ThrowsArgumentException()
    {
        // Arrange
        await using var database = CreateInMemoryDatabase();

        var robotStatusService = new Mock<IRobotStatusService>();
        var service = new MissionLogsService(database.DbContext, robotStatusService.Object);

        var request = new AddMissionLogRequest
        {
            UserId = 999,
            Role = UserRole.Commander,
            Command = RobotCommand.Move,
            CommandResult = RobotCommandResult.Success,
            Details = "Invalid user test."
        };

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.AddMissionLog(request));

        // Assert
        Assert.Equal("User does not exist", exception.Message);
        Assert.False(await database.DbContext.MissionLogs.AnyAsync());
    }

    // Unit Test: verifies that AddMissionLog rejects invalid enum values.
    // This protects the audit trail from storing command records with invalid roles, commands, or results.
    [Fact]
    public async Task AddMissionLog_WhenCommandResultIsInvalid_ThrowsArgumentException()
    {
        // Arrange
        await using var database = CreateInMemoryDatabase();
        await SeedUserAsync(database.DbContext);

        var robotStatusService = new Mock<IRobotStatusService>();
        var service = new MissionLogsService(database.DbContext, robotStatusService.Object);

        var request = new AddMissionLogRequest
        {
            UserId = 1,
            Role = UserRole.Commander,
            Command = RobotCommand.Move,
            CommandResult = (RobotCommandResult)999,
            Details = "Invalid command result test."
        };

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.AddMissionLog(request));

        // Assert
        Assert.Equal("Command result is invalid", exception.Message);
        Assert.False(await database.DbContext.MissionLogs.AnyAsync());
    }

    // Unit Test: verifies that GetAllMissionLogs returns logs newest first with user and robot data mapped.
    // This confirms the mission log page receives usable audit data for review.
    [Fact]
    public async Task GetAllMissionLogs_WhenLogsExist_ReturnsLogsOrderedByNewestFirst()
    {
        // Arrange
        await using var database = CreateInMemoryDatabase();
        await SeedUserAsync(database.DbContext);

        database.DbContext.MissionLogs.AddRange(
            new MissionLog
            {
                UserId = 1,
                Timestamp = DateTime.UtcNow.AddMinutes(-10),
                Role = UserRole.Commander,
                Command = RobotCommand.Move,
                CommandResult = RobotCommandResult.Success,
                Details = "Older log.",
                RobotX = 1,
                RobotY = 1,
                Battery = 90,
                ConnectionStatus = "IDLE"
            },
            new MissionLog
            {
                UserId = 1,
                Timestamp = DateTime.UtcNow,
                Role = UserRole.Commander,
                Command = RobotCommand.Reset,
                CommandResult = RobotCommandResult.Success,
                Details = "Newer log.",
                RobotX = 0,
                RobotY = 0,
                Battery = 100,
                ConnectionStatus = "IDLE"
            });

        await database.DbContext.SaveChangesAsync();

        var robotStatusService = new Mock<IRobotStatusService>();
        var service = new MissionLogsService(database.DbContext, robotStatusService.Object);

        // Act
        var result = await service.GetAllMissionLogs();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(RobotCommand.Reset, result[0].Command);
        Assert.Equal(RobotCommand.Move, result[1].Command);
        Assert.Equal("Newer log.", result[0].Details);
        Assert.Equal(0, result[0].RobotPosition?.X);
        Assert.Equal(0, result[0].RobotPosition?.Y);
    }

    private static async Task SeedUserAsync(RobotApiDbContext dbContext)
    {
        dbContext.Users.Add(new UserAccount
        {
            Id = 1,
            Username = "test-user",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "hashed-password",
            Role = UserRole.Commander,
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();
    }

    private static TestDatabase CreateInMemoryDatabase()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<RobotApiDbContext>()
            .UseSqlite(connection)
            .Options;

        var dbContext = new RobotApiDbContext(options);
        dbContext.Database.EnsureCreated();

        return new TestDatabase(connection, dbContext);
    }

    private sealed class TestDatabase : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        public RobotApiDbContext DbContext { get; }

        public TestDatabase(SqliteConnection connection, RobotApiDbContext dbContext)
        {
            _connection = connection;
            DbContext = dbContext;
        }

        public async ValueTask DisposeAsync()
        {
            await DbContext.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}