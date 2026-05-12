using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.SignalR;
using RobotManagementSystem.Hubs;
using System.Text.Json;
using Newtonsoft.Json.Serialization;
using RobotManagementSystem.Shared.Models.Robot;

namespace RobotManagementSystem.BackgroundServices;

// This service rebroadcasts robot telemetry to all connected clients using the SignalR hub
public class RobotTelemetryBackgroundService : BackgroundService
{
    private readonly IHubContext<RobotTelemetryHub> _hubContext;
    private readonly ILogger<RobotTelemetryBackgroundService> _logger;
    private readonly IConfiguration _configuration;

    public RobotTelemetryBackgroundService(IHubContext<RobotTelemetryHub> hubContext, ILogger<RobotTelemetryBackgroundService> logger, IConfiguration configuration)
    {
        _hubContext = hubContext;
        _logger = logger;
        _configuration = configuration;
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

                await _hubContext.Clients.All.SendCoreAsync("ConnectionStatus", new object[] { "Connected" },
                    stoppingToken);

                var buffer = new byte [8192];

                while (webSocket.State == WebSocketState.Open && !stoppingToken.IsCancellationRequested)
                {
                    var response = await webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        stoppingToken);

                    if (response.MessageType == WebSocketMessageType.Close)
                        break;

                    var jsonResponse = Encoding.UTF8.GetString(buffer, 0, response.Count);

                    RobotTelemetry? robotTelemetry;
                    
                    try
                    {
                        var jsonParseOptions = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };

                        robotTelemetry = JsonSerializer.Deserialize<RobotTelemetry>(jsonResponse, jsonParseOptions);
                    }
                    catch (JsonException e)
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
                break; // TODO This is for clean shutdown, but needs to be tested
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Robot Telemetry Websocket was disconnected.");

                await _hubContext.Clients.All.SendCoreAsync("ConnectionStatus", new object[]{"Reconnecting"}, stoppingToken);
                
                await Task.Delay(TimeSpan.FromMilliseconds(2500), stoppingToken);
            }
            
        }
    }
}