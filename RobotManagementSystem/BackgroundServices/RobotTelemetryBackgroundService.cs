using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.SignalR;
using RobotManagementSystem.Hubs;
using System.Text.Json;
using RobotManagementSystem.Services;
using RobotManagementSystem.Shared.Models.Robot;

namespace RobotManagementSystem.BackgroundServices;

// This service rebroadcasts robot telemetry to all connected clients using the SignalR hub
public class RobotTelemetryBackgroundService : BackgroundService
{
    private readonly IHubContext<RobotTelemetryHub> _hubContext;
    private readonly ILogger<RobotTelemetryBackgroundService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IRobotApiStatusStore _robotApiStatusStore;
    private DateTime _latestTelemetryReceivedAt;

    public RobotTelemetryBackgroundService(IHubContext<RobotTelemetryHub> hubContext, ILogger<RobotTelemetryBackgroundService> logger, IConfiguration configuration, IRobotApiStatusStore robotApiStatusStore)
    {
        _hubContext = hubContext;
        _logger = logger;
        _configuration = configuration;
        _robotApiStatusStore = robotApiStatusStore;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var webSocket = new ClientWebSocket();

            try
            {
                await webSocket.ConnectAsync
                    (new Uri(_configuration["RobotApi:TelemetryAddress"] ?? throw new InvalidOperationException("RobotApi:TelemetryAddress is empty or missing.")), 
                        stoppingToken);

                var buffer = new byte [8192];

                while (webSocket.State == WebSocketState.Open && !stoppingToken.IsCancellationRequested)
                {
                    using var timeoutCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                    
                    timeoutCancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(3));

                    WebSocketReceiveResult response;

                    try
                    {
                        response = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                        timeoutCancellationTokenSource.Token);
                    }
                    catch (OperationCanceledException) when (!stoppingToken.IsCancellationRequested)
                    {
                        _logger.LogWarning("No telemetry received from robot simulation API in 3 seconds, assuming there was an outage.");
                        
                        await Task.Delay(TimeSpan.FromMilliseconds(2500), stoppingToken);
                        break;
                    }
                    
                    var jsonResponse = Encoding.UTF8.GetString(buffer, 0, response.Count);
                    
                    _latestTelemetryReceivedAt = DateTime.UtcNow;

                    RobotTelemetry? robotTelemetry;
                    
                    try
                    {
                        robotTelemetry = JsonSerializer.Deserialize<RobotTelemetry>(jsonResponse,
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });
                    }
                    catch (JsonException)
                    {
                        _logger.LogWarning("Failed to parse robot telemetry JSON.");
                        continue;
                    }
                    
                    if(robotTelemetry == null)
                        continue;
                    
                    // TODO
                    // add mapping service to update from telemetry (inclde sensor data)
                    // send TelemetryUpdated AND MapUdated back to all clients

                    await _hubContext.Clients.All.SendCoreAsync("TelemetryUpdated", new object[] { robotTelemetry },
                        stoppingToken);
                }

            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                
                break;
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Robot Telemetry Websocket was disconnected.");
                
                await Task.Delay(TimeSpan.FromMilliseconds(2500), stoppingToken);
            }
            
        }
    }
}