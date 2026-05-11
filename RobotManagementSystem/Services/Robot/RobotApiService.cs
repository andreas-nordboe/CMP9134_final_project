using RobotManagementSystem.Shared.Models.Components;
using RobotManagementSystem.Shared.Models.Robot;

namespace RobotManagementSystem.Services;

public class RobotApiService : IRobotApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RobotApiService> _logger;
    
    public async Task<RobotStatusResponse?> GetRobotStatusAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<RobotStatusResponse>("/api/status");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Could not get robot status.");
            return null;
        }
    }

    public async Task<RobotCommandResponse?> MoveRobotAsync(RobotNavigationRequest request)
    {
        // Validate coordinates so they don't go out of bounds (TODO move this to helper later)
        if (request.X < 0 || request.Y < 0 || request.X > 20 || request.Y > 20)
        {
            return new RobotCommandResponse
            {
                Success = false,
                Message = "Robot coordinates not valid and must be between 0 and 20."
            };
        }

        try
        {
            var moveRequest = new RobotNavigationRequest
            {
                X = request.X,
                Y = request.Y
            };
            
            var response = await _httpClient.PostAsJsonAsync("/api/move", moveRequest);
            if (!response.IsSuccessStatusCode)
            {
                return new RobotCommandResponse
                {
                    Success = true,
                    Message = $"Move command failed. Response: {response.StatusCode}." // TODO I'll try stautus code for now and try ReasonPhrase later 
                };
            }
            
            return new RobotCommandResponse
            {
                Success = true,
                Message = $"Robot was moved to {moveRequest.X}, {moveRequest.Y}.",
                RobotPosition = new Vector2D
                {
                    X = moveRequest.X,
                    Y = moveRequest.Y
                }
            };

        }
        catch (Exception e)
        {
            _logger.LogError(e, "Robot move command failed.");
            
            return new RobotCommandResponse
            {
                Success = false,
                Message = "Robot move command failed."
            };
        }
    }

    public Task<RobotCommandResponse?> ResetAsync()
    {
        throw new NotImplementedException();
    }
}