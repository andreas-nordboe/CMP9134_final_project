using Microsoft.AspNetCore.SignalR.Client;
using RobotManagementSystem.Shared.Models.Robot;

namespace RobotManagementSystem.Client.Services.Robot;

public class RobotHubCommunication : IAsyncDisposable
{
    private readonly IConfiguration _configuration;
    private HubConnection? _connection;
    private readonly IAppState _appState;
    
    public event Action<string>? ConnectionStatusChanged;
    public event Action<RobotTelemetry>? TelemetryUpdated;
    
    private readonly ILogger<RobotHubCommunication> _logger;
    
    public RobotTelemetry? LatestTelemetry { get; private set; }
    
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private bool _hasBeenDisposed;
    private bool _retryHasBeenScheduled;

    public RobotHubCommunication(IConfiguration configuration, IAppState appState, ILogger<RobotHubCommunication> logger)
    {
        _configuration = configuration;
        _appState = appState;
        _logger = logger;
    }
    
    public bool IsConnected => _connection?.State == HubConnectionState.Connected;
    
    
    public async Task StartAsync()
    {
        await _connectionLock.WaitAsync();
        
        try
        {
            if(_hasBeenDisposed)
                return;
            
            _logger.LogInformation("Starting SignalR connection");

            if (_connection?.State == HubConnectionState.Connected ||
                _connection?.State == HubConnectionState.Reconnecting ||
                _connection?.State == HubConnectionState.Connecting)
                return;

            if (_connection != null)
            {
                await _connection.DisposeAsync();
                _connection = null;
            }

            var apiBaseAddress = _configuration["ApiSettings:HubAddress"];

            _logger.LogInformation("Connecting to hub at {HubAddress}", apiBaseAddress);
            
            if (string.IsNullOrWhiteSpace(apiBaseAddress))
                throw new InvalidOperationException("ApiSettings:HubAddress is not set or missing");

            _connection = new HubConnectionBuilder()
                .WithUrl(apiBaseAddress)
                .WithAutomaticReconnect()
                .Build();

            _connection.Reconnecting += error =>
            {
                _appState.SetSignalDisrupted(true);
                ConnectionStatusChanged?.Invoke(RobotApiStatus.Reconnecting);
                return Task.CompletedTask;
            };

            _connection.Reconnected += error =>
            {
                _appState.SetSignalDisrupted(false);
                ConnectionStatusChanged?.Invoke(RobotApiStatus.Connected);
                return Task.CompletedTask;
            };

            _connection.Closed += error =>
            {
                _appState.SetSignalDisrupted(true);
                ConnectionStatusChanged?.Invoke(RobotApiStatus.Disconnected);

                _logger.LogWarning(error, "SignalR connection closed");

                ScheduleRetry();

                return Task.CompletedTask;
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

            try
            {
                await _connection.StartAsync();
                _appState.SetSignalDisrupted(false);
                ConnectionStatusChanged?.Invoke(RobotApiStatus.Connected);

                _logger.LogInformation("SignalR Connected!");
            }
            catch (Exception e)
            {
                _appState.SetSignalDisrupted(true);
                ConnectionStatusChanged?.Invoke(RobotApiStatus.Disconnected);

                _logger.LogError(e, "Failed to connect to SignalR Hub at address {HubAddress}", apiBaseAddress);

                await _connection.DisposeAsync();
                _connection = null;

                ScheduleRetry();
            }
        }
        finally
        {
            _connectionLock.Release();
        }
    }
    
    private void ScheduleRetry()
    {
        if (_hasBeenDisposed || _retryHasBeenScheduled)
            return;

        _retryHasBeenScheduled = true;

        _ = Task.Run(async () =>
        {
            await Task.Delay(3200);

            

            if (_hasBeenDisposed)
                return;

            try
            {
                await StartAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Retry failed");
            }
            
            _retryHasBeenScheduled = false;
        });
    }


    public async ValueTask DisposeAsync()
    {
        _hasBeenDisposed = true;

        await _connectionLock.WaitAsync();

        try
        {
            if (_connection != null)
            {
                await _connection.DisposeAsync();
                _connection = null;
            }
        }
        finally
        {
            _connectionLock.Release();
            _connectionLock.Dispose();
        }
    }
}