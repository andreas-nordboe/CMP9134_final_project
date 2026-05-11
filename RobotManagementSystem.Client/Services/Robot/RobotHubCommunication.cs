using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace RobotManagementSystem.Client.Services.Robot;

public class RobotHubCommunication : IAsyncDisposable
{
    private readonly IConfiguration _configuration;
    private HubConnection? _connection;
    
    public event Action<string>? ConnectionStatusChanged;
    public event Action<string>? TelemetryUpdated;

    public RobotHubCommunication(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public bool IsConnected => _connection?.State == HubConnectionState.Connected;
    
    public async Task StartAsync()
    {
        if (_connection != null)
            return;
        
        var apiBaseAddress = _configuration["ApiSettings:HubAddress"];
        
        if (string.IsNullOrEmpty(apiBaseAddress))
            throw new InvalidOperationException("ApiSettings:HubAddress is not set or missing");

        _connection = new HubConnectionBuilder()
            .WithUrl(apiBaseAddress)
            .WithAutomaticReconnect()
            .Build();
        
        _connection.On<string>("ConnectionStatus", status =>
        {
            ConnectionStatusChanged?.Invoke(status);
        });
        
        _connection.On<string>("TelemetryUpdated", telemetryJson =>
        {
            TelemetryUpdated?.Invoke(telemetryJson);
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