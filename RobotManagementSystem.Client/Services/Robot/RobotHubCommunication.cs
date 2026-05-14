using Microsoft.AspNetCore.SignalR.Client;
using RobotManagementSystem.Shared.Models.Robot;

namespace RobotManagementSystem.Client.Services.Robot;

public class RobotHubCommunication : IAsyncDisposable
{
    private readonly IConfiguration _configuration;
    private HubConnection? _connection;
    private IAppState _appState;
    
    public event Action<string>? ConnectionStatusChanged;
    public event Action<RobotTelemetry>? TelemetryUpdated;
    
    private readonly ILogger<RobotHubCommunication> _logger;
    
    public RobotTelemetry? LatestTelemetry { get; private set; }

    public RobotHubCommunication(IConfiguration configuration, IAppState appState, ILogger<RobotHubCommunication> logger)
    {
        _configuration = configuration;
        _appState = appState;
        _logger = logger;
    }
    
    public bool IsConnected => _connection?.State == HubConnectionState.Connected;
    
    
    public async Task StartAsync()
    {
        _logger.LogInformation("Starting SignalR connection");
        
        if (_connection?.State == HubConnectionState.Connected || _connection?.State == HubConnectionState.Reconnecting || _connection?.State == HubConnectionState.Connecting)
            return;

        if (_connection != null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }
        
        var apiBaseAddress = _configuration["ApiSettings:HubAddress"];
        
        _logger.LogInformation($"Connecting to hub at {apiBaseAddress}");
        
        if (string.IsNullOrEmpty(apiBaseAddress))
            throw new InvalidOperationException("ApiSettings:HubAddress is not set or missing");

        _connection = new HubConnectionBuilder()
            .WithUrl(apiBaseAddress)
            .WithAutomaticReconnect()
            .Build();

        _connection.Reconnecting += error =>
        {
            ConnectionStatusChanged?.Invoke(RobotApiStatus.Reconnecting);
            return Task.CompletedTask;
        };
        
        _connection.Reconnected += error =>
        {
            ConnectionStatusChanged?.Invoke(RobotApiStatus.Connected);
            return Task.CompletedTask;
        };

        _connection.Closed += async error =>
        {
            ConnectionStatusChanged?.Invoke(RobotApiStatus.Disconnected);

            await Task.Delay(3000);

            try
            {
                await StartAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to reconnect to SignalR");
            }
        };
        
        _connection.On<string>(RobotApiStatus.StatusMethod, status =>
        {
            _appState.ApiStatus = status;
            ConnectionStatusChanged?.Invoke(status);
            Console.WriteLine($"Connection status changed: {status}");
        });
        
        _connection.On<RobotTelemetry>("TelemetryUpdated", telemetryData =>
        {
            LatestTelemetry = telemetryData;
            TelemetryUpdated?.Invoke(telemetryData);
        });

        await _connection.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection != null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }
    }
}