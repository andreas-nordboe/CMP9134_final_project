using System.Net.Http.Json;
using RobotManagementSystem.Client.Services.Robot;
using RobotManagementSystem.Shared.Models.Users;
namespace RobotManagementSystem.Client.Services.Admin;

// This is kept separate from get me service to have clear separation between admin and other user services
public class AdminUserManagementService : IAdminUserManagementService
{
    private readonly HttpClient _httpClient;
    private readonly RobotHubCommunication _robotHubCommunication; // TODO notify other admins through when modifying users (SignalRHub groups should work for this)
    private readonly IAppState _appState; // TODO potentially remove this if I'm not using it later
    private readonly ILogger<AdminUserManagementService> _logger;

    public AdminUserManagementService(IHttpClientFactory httpClientFactory, RobotHubCommunication robotHubCommunication, IAppState appState, ILogger<AdminUserManagementService> logger)
    {
        _robotHubCommunication = robotHubCommunication;
        _appState = appState;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("API");
    }

    public async Task<List<UserAccountDto>> ListUsersAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("users/all");
            
            if (!response.IsSuccessStatusCode)
                return new List<UserAccountDto>();
            
            var listUsersResponse = await response.Content.ReadFromJsonAsync<List<UserAccountDto>>();
            
            _logger.LogInformation("List users response: {response}", listUsersResponse);
            
            if(listUsersResponse == null || listUsersResponse.Count == 0)
                return new List<UserAccountDto>();
            
            return listUsersResponse;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to list users.");
            return new List<UserAccountDto>();
        }
    }

    public async Task<UserAccountDto?> GetUserAsync(string userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"users/{userId}");
            
            if (!response.IsSuccessStatusCode)
                return null;
            
            var getUserResponse = await response.Content.ReadFromJsonAsync<UserAccountDto>();
            
            if(getUserResponse == null)
                return null;
            
            return getUserResponse;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to retrieve user.");
            return null;
        }
    }

    public async Task<bool> DeleteUserAsync(int userId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"users/{userId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to delete user.");
            return false;
        }
    }

    public async Task<UserAccountDto?> UpdateUserRoleAsync(int userId, UserRole role)
    {
        try
        {
            var response = await _httpClient.PatchAsJsonAsync($"users/{userId}/role", role);
            
            if (!response.IsSuccessStatusCode)
                return null;
        
            var getUserResponse = await response.Content.ReadFromJsonAsync<UserAccountDto>();
        
            if(getUserResponse == null)
                return null;
        
            return getUserResponse;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to update user role.");
            return null;
        }
    }
}