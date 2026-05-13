using System.Net.Http.Json;
using RobotManagementSystem.Client.Services.DataStore;
using RobotManagementSystem.Client.Services.Robot;
using RobotManagementSystem.Client.Services.Sessions;
using RobotManagementSystem.Shared.Models.Map;
using RobotManagementSystem.Shared.Models.Robot;

namespace RobotManagementSystem.Client.Services.Map;

public class MapService : IMapService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MapService> _logger;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly RobotHubCommunication _robotHubCommunication;
    private readonly IAppState _appState;
    private readonly IDataStoreService _dataStoreService;
    private readonly IUserSessionService _userSessionService;

    public MapService(RobotHubCommunication robotHubCommunication, IAppState appState, IDataStoreService dataStoreService, IUserSessionService userSessionService, ILogger<MapService> logger, IHttpClientFactory httpClientFactory)
    {
        _robotHubCommunication = robotHubCommunication;
        _appState = appState;
        _dataStoreService = dataStoreService;
        _userSessionService = userSessionService;
        _logger = logger;
        this.httpClientFactory = httpClientFactory;
        _httpClient = httpClientFactory.CreateClient("API");
    }

    public async Task<MapResponse?> GetMapAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<MapResponse>("/map");
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Failed to retrieve map information.");
            return null;
        }
    }
}