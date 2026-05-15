using RobotManagementSystem.Shared.Models.MissionLog;

namespace RobotManagementSystem.Services.MissionLogs;

// Tasks: Record commands, record who executed them, record responses and timestamps
public interface IMissionLogsService
{
    Task<MissionLog> AddMissionLog(AddMissionLogRequest missionLog);
    Task<List<MissionLogDto>> GetAllMissionLogs();
    Task<List<MissionLog>> GetAllMissionLogsByUserId(int userId);
}