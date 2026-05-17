using System.Net;
using Microsoft.AspNetCore.SignalR;
using RobotManagementSystem.Hubs;
using RobotManagementSystem.Services.System;
using RobotManagementSystem.Services.SystemStatus;
using RobotManagementSystem.Shared.Models.Robot;

namespace RobotManagementSystem.Services;

public class RobotStatusService : IRobotStatusService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RobotStatusService> _logger;
    private readonly IRobotApiStatusStore _robotApiStatusStore;
    private readonly IHubContext<RobotTelemetryHub> _hubContext;
    private readonly ISystemStatusLogService _systemStatusLogService;

    public RobotStatusService(ILogger<RobotStatusService> logger, HttpClient httpClient, IRobotApiStatusStore robotApiStatusStore, IHubContext<RobotTelemetryHub> hubContext, ISystemStatusLogService systemStatusLogService)
    {
        _logger = logger;
        _httpClient = httpClient;
        _robotApiStatusStore = robotApiStatusStore;
        _hubContext = hubContext;
        _systemStatusLogService = systemStatusLogService;
    }

    public async Task<RobotStatusResponse?> GetRobotStatusAsync()
    {
        try
        {
            var startedAt = DateTime.UtcNow;
            var response = await _httpClient.GetAsync("status");
            var latencyMs = (DateTime.UtcNow - startedAt).TotalMilliseconds;

            _robotApiStatusStore.LastLatencyMs = latencyMs;
            _robotApiStatusStore.RecentLatenciesMs.Add(latencyMs);

            if (_robotApiStatusStore.RecentLatenciesMs.Count > 20)
            {
                _robotApiStatusStore.RecentLatenciesMs.RemoveAt(0);
            }

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
            
            var status = await response.Content.ReadFromJsonAsync<RobotStatusResponse>();

            if (status != null && _robotApiStatusStore.LastRobotState != status.Status)
            {
                await _systemStatusLogService.LogRobotStatusChangedAsync(status);
                _robotApiStatusStore.LastRobotState = status.Status;
            }

            if (status == null)
            {
                await SetRobotApiStatusAsync(RobotApiStatus.Reconnecting);
                return null;
            }
            
            await SetRobotApiStatusAsync(RobotApiStatus.Connected);
            
            if (DateTime.UtcNow - _robotApiStatusStore.LastSnapshotLoggedAt > TimeSpan.FromSeconds(15))
            {
                await _systemStatusLogService.LogTelemetrySnapshotAsync(status);
                _robotApiStatusStore.LastSnapshotLoggedAt = DateTime.UtcNow;
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
        
        await _systemStatusLogService.LogConnectionChangedAsync(status);
        
        _logger.LogInformation("Broadcasting robot API status: {Status}", status);
        
        await _hubContext.Clients.All.SendAsync(RobotApiStatus.StatusMethod, status);
    }
}