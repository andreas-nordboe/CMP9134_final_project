using System.Net.Http.Json;
using RobotManagementSystem.Client.Services.MissionLogs;
using RobotManagementSystem.Shared.Models.MissionLog;
using RobotManagementSystem.Shared.Models.SystemStatus;

namespace RobotManagementSystem.Client.Services.SystemStatus;

public class SystemStatusLogService : ISystemStatusLogService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly HttpClient _httpClient;
    private readonly ILogger<ISystemStatusLogService> _logger;

    public SystemStatusLogService(IHttpClientFactory httpClientFactory, ILogger<ISystemStatusLogService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("API");
    }

    public async Task<PagedResult<SystemStatusLog>> GetSystemStatusLogsAsync(SystemStatusEventType? eventType, DateTime? fromDate, int page, int pageSize)
    {
        
        try
        {
            return await _httpClient.GetFromJsonAsync<PagedResult<SystemStatusLog>>("/mission-logs/all") ?? new PagedResult<SystemStatusLog>();
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Failed to retrieve mission logs.");
            return new PagedResult<SystemStatusLog>();
        }
    }
}