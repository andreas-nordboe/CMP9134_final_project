using System.Net.Http.Json;
using RobotManagementSystem.Shared.Models.Map;
using RobotManagementSystem.Shared.Models.MissionLog;

namespace RobotManagementSystem.Client.Services.MissionLogs;

public class MissionLogService : IMissionLogService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly HttpClient _httpClient;
    private readonly ILogger<MissionLogService> _logger;

    public MissionLogService(IHttpClientFactory httpClientFactory, ILogger<MissionLogService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("API");
    }

    public async Task<List<MissionLogDto?>> GetAllMissionLogs()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<MissionLogDto?>>("/mission-logs/all");
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Failed to retrieve mission logs.");
            return new List<MissionLogDto?>();
        }
    }
}