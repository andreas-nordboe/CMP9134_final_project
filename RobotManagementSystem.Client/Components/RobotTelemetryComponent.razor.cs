using Microsoft.AspNetCore.Components;
using RobotManagementSystem.Client.Services;
using RobotManagementSystem.Client.Services.Robot;
using RobotManagementSystem.Shared.Models.Robot;

namespace RobotManagementSystem.Client.Components;

public partial class RobotTelemetryComponent : ComponentBase
{
    [Inject] public RobotHubCommunication RobotHubCommunication { get; set; } = default!;
    [Inject] public IAppState _appState { get; set; }
    
    
    private string? ConnectionStatus { get; set; }
    private RobotTelemetry? LatestTelemetry { get; set; }

    protected override void OnInitialized()
    {
        // Set this immediately first as it may have been cached during login
        ConnectionStatus = _appState.ApiStatus;
        
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