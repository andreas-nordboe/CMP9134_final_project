using System.Net;
using Microsoft.AspNetCore.SignalR;
using RobotManagementSystem.Hubs;
using RobotManagementSystem.Shared.Models.Robot;

namespace RobotManagementSystem.Services;

public class RobotStatusService : IRobotStatusService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RobotStatusService> _logger;
    private readonly IRobotApiStatusStore _robotApiStatusStore;
    private readonly IHubContext<RobotTelemetryHub> _hubContext;

    public RobotStatusService(ILogger<RobotStatusService> logger, HttpClient httpClient, IRobotApiStatusStore robotApiStatusStore, IHubContext<RobotTelemetryHub> hubContext)
    {
        _logger = logger;
        _httpClient = httpClient;
        _robotApiStatusStore = robotApiStatusStore;
        _hubContext = hubContext;
    }

    public async Task<RobotStatusResponse?> GetRobotStatusAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("status");

            if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                _logger.LogWarning("Robot API is not available.");
                await SetRobotApiStatusAsync(RobotApiStatus.Reconnecting);
                
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to retrieve robot status.");
                
                await SetRobotApiStatusAsync(RobotApiStatus.Reconnecting);
                return null;
            }
            
            await SetRobotApiStatusAsync(RobotApiStatus.Connected);
            
            var status = await response.Content.ReadFromJsonAsync<RobotStatusResponse>();

            if (status == null)
            {
                await SetRobotApiStatusAsync(RobotApiStatus.Reconnecting);
                return null;
            }
            
            return status;
            
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Failed to retrieve robot status.");
            
            await SetRobotApiStatusAsync(RobotApiStatus.Reconnecting);
            
            return null;
        }
    }

    private async Task SetRobotApiStatusAsync(string status)
    {
        if(_robotApiStatusStore.CurrentApiStatus == status)
            return;
        
        _robotApiStatusStore.CurrentApiStatus = status;
        _logger.LogInformation("Broadcasting robot API status: {Status}", status);
        await _hubContext.Clients.All.SendAsync(RobotApiStatus.StatusMethod, status);
    }
}