using RobotManagementSystem.Shared.Models.Robot;

namespace RobotManagementSystem.Services;

public class RobotStatusService : IRobotStatusService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RobotStatusService> _logger;

    public RobotStatusService(ILogger<RobotStatusService> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<RobotStatusResponse?> GetRobotStatusAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<RobotStatusResponse>("status");
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Failed to retrieve robot status.");
            return null;
        }
    }
}