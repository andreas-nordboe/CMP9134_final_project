using Microsoft.AspNetCore.Components;
using RobotManagementSystem.Client.Services.Robot;
using RobotManagementSystem.Shared.Models.Robot;

namespace RobotManagementSystem.Client.Components;

public partial class RobotTelemetryComponent : ComponentBase
{
    [Inject] public RobotHubCommunication RobotHubCommunication { get; set; } = default!;
    
    private string ConnectionStatus { get; set; } = "Disconnected";
    private RobotTelemetry? LatestTelemetry { get; set; }

    protected override void OnInitialized()
    {
        RobotHubCommunication.ConnectionStatusChanged += OnConnectionStatusChanged;
        RobotHubCommunication.TelemetryUpdated += OnTelemetryUpdated;
    }

    private async void OnConnectionStatusChanged(string status)
    {
        ConnectionStatus = status;
        await InvokeAsync(StateHasChanged);
    }

    private async void OnTelemetryUpdated(RobotTelemetry telemetryJson)
    {
        LatestTelemetry = telemetryJson;
        await InvokeAsync(StateHasChanged);
    }
    
}