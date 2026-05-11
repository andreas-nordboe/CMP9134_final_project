using Microsoft.AspNetCore.Components;
using RobotManagementSystem.Client.Services.Robot;

namespace RobotManagementSystem.Client.Components;

public partial class RobotTelemetry : ComponentBase
{
    [Inject] 
    public RobotHubCommunication RobotHubCommunication { get; set; }
    
    private string ConnectionStatus { get; set; } = "Disconnected";
    private string LatestTelemetry { get; set; } = string.Empty;

    protected override void OnInitialized()
    {
        RobotHubCommunication.ConnectionStatusChanged += OnTelemetryUpdated;
        RobotHubCommunication.TelemetryUpdated += OnTelemetryUpdated;
    }

    private async void OnConnectionStatusChanged(string status)
    {
        ConnectionStatus = status;
        await InvokeAsync(StateHasChanged);
    }

    private async void OnTelemetryUpdated(string telemetryJson)
    {
        LatestTelemetry = telemetryJson;
        await InvokeAsync(StateHasChanged);
    }
    
}