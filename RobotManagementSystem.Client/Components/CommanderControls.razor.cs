using Microsoft.AspNetCore.Components;
using MudBlazor;
using RobotManagementSystem.Client.Services;
using RobotManagementSystem.Client.Services.Robot;
using RobotManagementSystem.Shared.Models.Components;
using RobotManagementSystem.Shared.Models.Robot;

namespace RobotManagementSystem.Client.Components;

public partial class CommanderControls : ComponentBase, IDisposable
{
    public int UsesAdvancedControls { get; set; }
    public Vector2D InputVector { get; set; } = new();

    private bool IsProcessingCommand { get; set; }

    [Inject] private IRobotCommanderService RobotCommanderService { get; set; } = default!;
    [Inject] private RobotHubCommunication RobotHubCommunication { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IAppState AppState { get; set; } = default!;

    private RobotTelemetry? LatestTelemetry => RobotHubCommunication.LatestTelemetry;

    private string CurrentPositionText =>
        LatestTelemetry?.Position == null
            ? "N/A"
            : $"({LatestTelemetry.Position.X}, {LatestTelemetry.Position.Y})";

    private string RobotStatusText =>
        LatestTelemetry?.Status ?? "Unknown";

    private string BatteryText =>
        LatestTelemetry == null
            ? "N/A"
            : $"{LatestTelemetry.Battery}%";

    private string CommandStatusText
    {
        get
        {
            if (!RobotHubCommunication.IsConnected)
                return "Disconnected";

            if (LatestTelemetry == null)
                return "Waiting for telemetry";

            if (LatestTelemetry.Status == nameof(RobotStatus.MOVING))
                return "Robot moving";

            return "Ready";
        }
    }

    private bool CanSendCommand =>
        RobotHubCommunication.IsConnected &&
        LatestTelemetry != null &&
        !IsProcessingCommand &&
        LatestTelemetry.Battery > 0 &&
        LatestTelemetry.Status != nameof(RobotStatus.MOVING);

    private string SafetyMessage
    {
        get
        {
            if (!RobotHubCommunication.IsConnected)
                return "Connection unavailable";

            if (LatestTelemetry == null)
                return "No telemetry";

            if (LatestTelemetry.Battery <= 0)
                return "Battery empty";

            if (LatestTelemetry.Battery <= 10)
                return "Critical battery";

            if (LatestTelemetry.Status == nameof(RobotStatus.MOVING))
                return "Robot moving";

            if (IsProcessingCommand)
                return "Processing";

            return "Ready";
        }
    }

    protected override void OnInitialized()
    {
        RobotHubCommunication.TelemetryUpdated += OnTelemetryUpdated;
    }

    private void OnTelemetryUpdated(RobotTelemetry telemetry)
    {
        InvokeAsync(StateHasChanged);
    }

    protected async Task MoveRobot()
    {
        if (UsesAdvancedControls != 1)
            return;

        await SendMoveCommandAsync(InputVector.X, InputVector.Y);
    }

    protected async Task ResetSimulation()
    {
        try
        {
            IsProcessingCommand = true;
            StateHasChanged();

            await RobotCommanderService.ResetAsync();

            Snackbar.Add("Reset simulation command sent.", Severity.Warning);
        }
        catch
        {
            Snackbar.Add("Could not send reset simulation command.", Severity.Error);
        }
        finally
        {
            IsProcessingCommand = false;
            StateHasChanged();
        }
    }

    protected async Task MoveRobotLeft()
    {
        await SendStepCommandAsync(-1, 0);
    }

    protected async Task MoveRobotRight()
    {
        await SendStepCommandAsync(1, 0);
    }

    protected async Task MoveRobotUp()
    {
        await SendStepCommandAsync(0, -1);
    }

    protected async Task MoveRobotDown()
    {
        await SendStepCommandAsync(0, 1);
    }

    private async Task SendStepCommandAsync(int deltaX, int deltaY)
    {
        if (LatestTelemetry?.Position == null)
        {
            Snackbar.Add("Cannot move robot because telemetry position is unavailable.", Severity.Warning);
            return;
        }

        var targetX = (int)LatestTelemetry.Position.X + deltaX;
        var targetY = (int)LatestTelemetry.Position.Y + deltaY;

        await SendMoveCommandAsync(targetX, targetY);
    }

    private async Task SendMoveCommandAsync(int x, int y)
    {
        if (!CanSendCommand)
        {
            Snackbar.Add(SafetyMessage, Severity.Info);
            return;
        }

        try
        {
            IsProcessingCommand = true;
            
            AppState.SetPendingRobotCommandTarget(new Vector2D
            {
                X = x,
                Y = y
            });
            
            StateHasChanged();

            await RobotCommanderService.MoveRobotAsync(new RobotNavigationRequest
            {
                X = x,
                Y = y
            });

            Snackbar.Add($"Move command sent to ({x}, {y}).", Severity.Success);
        }
        catch
        {
            AppState.SetPendingRobotCommandTarget(null);
            Snackbar.Add("Could not send move command to robot.", Severity.Error);
        }
        finally
        {
            IsProcessingCommand = false;
            StateHasChanged();
        }
    }

    private Color GetCommandStatusColor()
    {
        if (!RobotHubCommunication.IsConnected)
            return Color.Error;

        if (LatestTelemetry == null)
            return Color.Warning;

        if (LatestTelemetry.Status == nameof(RobotStatus.MOVING))
            return Color.Info;

        return Color.Success;
    }

    public void Dispose()
    {
        RobotHubCommunication.TelemetryUpdated -= OnTelemetryUpdated;
    }
}