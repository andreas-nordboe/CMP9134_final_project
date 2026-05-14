using Microsoft.EntityFrameworkCore;
using RobotManagementSystem.Data;
using RobotManagementSystem.Services.FailureHandling;
using RobotManagementSystem.Shared.Models.MissionLog;

namespace RobotManagementSystem.Services.MissionLogs;

public class MissionLogsService : IMissionLogsService
{
    private readonly RobotApiDbContext _dbContext;

    public MissionLogsService(RobotApiDbContext dbContext)
    {
        _dbContext = dbContext;
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
        
        MissionLog newMissionLog = new MissionLog
        {
            UserId = missionLog.UserId,
            Timestamp = missionLog.Timestamp == default ? DateTime.UtcNow : missionLog.Timestamp,
            Role = missionLog.Role,
            Command =  missionLog.Command,
            CommandResult =  missionLog.CommandResult
        };

        _dbContext.MissionLogs.Add(newMissionLog);
        await _dbContext.SaveChangesAsync();
        
        return newMissionLog;
    }

    public async Task<List<MissionLog>> GetAllMissionLogs()
    {
        return await _dbContext.MissionLogs.ToListAsync();
    }

    public Task<List<MissionLog>> GetAllMissionLogsByUserId(int userId)
    {
        throw new NotImplementedException();
    }
}