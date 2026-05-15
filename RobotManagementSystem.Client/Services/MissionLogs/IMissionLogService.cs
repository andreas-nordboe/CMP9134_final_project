using RobotManagementSystem.Shared.Models.MissionLog;

namespace RobotManagementSystem.Client.Services.MissionLogs;

public interface IMissionLogService
{
    Task<List<MissionLog>> GetAllMissionLogs();
}