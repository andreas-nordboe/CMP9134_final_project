using System.Net.Http.Json;
using Microsoft.AspNetCore.WebUtilities;
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
            var query = new Dictionary<string, string?>
            {
                ["page"] = page.ToString(),
                ["pageSize"] = pageSize.ToString()
            };

            if (eventType.HasValue)
                query["eventType"] = eventType.Value.ToString();

            if (fromDate.HasValue)
                query["fromDate"] = fromDate.Value.ToString("O");

            var url = QueryHelpers.AddQueryString("/system-status-logs/all", query);

            return await _httpClient.GetFromJsonAsync<PagedResult<SystemStatusLog>>(url)
                   ?? new PagedResult<SystemStatusLog>();
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Failed to retrieve system status logs.");
            return new PagedResult<SystemStatusLog>();
        }
    }
}