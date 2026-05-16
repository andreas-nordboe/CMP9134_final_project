using Microsoft.EntityFrameworkCore;
using RobotManagementSystem.Data;
using RobotManagementSystem.Mappers;
using RobotManagementSystem.Services.FailureHandling;
using RobotManagementSystem.Shared.Models.Components;
using RobotManagementSystem.Shared.Models.MissionLog;

namespace RobotManagementSystem.Services.MissionLogs;

public class MissionLogsService : IMissionLogsService
{
    private readonly RobotApiDbContext _dbContext;
    private readonly IRobotApiService _robotApiService;
    private readonly IRobotStatusService _robotStatusService;

    public MissionLogsService(RobotApiDbContext dbContext, IRobotStatusService robotStatusService)
    {
        _dbContext = dbContext;
        _robotStatusService = robotStatusService;
    }

    public async Task<MissionLog> AddMissionLog(AddMissionLogRequest missionLog)
    {
        if (missionLog == null)
            throw new ArgumentNullException(nameof(missionLog));

        if (!Enum.IsDefined(missionLog.Role))
        {
            throw new ArgumentException("Command is invalid");
        }
        
        if (!Enum.IsDefined(missionLog.CommandResult))
        {
            throw new ArgumentException("Command result is invalid");
        }
        
        if (!Enum.IsDefined(missionLog.Command))
        {
            throw new ArgumentException("Command is invalid");
        }
        
        var userExists = await _dbContext.Users.AnyAsync(u => u.Id == missionLog.UserId);
        if (!userExists)
        {
            throw new ArgumentException("User does not exist");
        }

        var robotStatus = await _robotStatusService.GetRobotStatusAsync();
        
        MissionLog newMissionLog = new MissionLog
        {
            UserId = missionLog.UserId,
            Timestamp = DateTime.UtcNow,
            Role = missionLog.Role,
            Command =  missionLog.Command,
            CommandResult =  missionLog.CommandResult,
            Details = missionLog.Details,
            RobotX = robotStatus.Position.X,
            RobotY = robotStatus.Position.Y,
            Battery = robotStatus.Battery,
            ConnectionStatus = robotStatus?.Status,
        };

        _dbContext.MissionLogs.Add(newMissionLog);
        await _dbContext.SaveChangesAsync();
        
        return newMissionLog;
    }

    public async Task<List<MissionLogDto>> GetAllMissionLogs()
    {
        return await _dbContext.MissionLogs
            .Include(log => log.User)
            .OrderByDescending(log => log.Timestamp)
            .Select(missionLog => new MissionLogDto
            {
                LogId = missionLog.Id,
                User = UserMapper.ToDto(missionLog.User), // TODO handle exception better or return empty user
                Timestamp = missionLog.Timestamp,
                Role = missionLog.Role,
                Command = missionLog.Command,
                CommandResult = missionLog.CommandResult,
                RobotPosition = new Vector2D
                {
                    X = missionLog.RobotX,
                    Y = missionLog.RobotY
                },
                Battery = missionLog.Battery,
                ConnectionStatus = missionLog.ConnectionStatus,
            }).ToListAsync<MissionLogDto>();
    }

    public Task<List<MissionLog>> GetAllMissionLogsByUserId(int userId)
    {
        throw new NotImplementedException();
    }
}