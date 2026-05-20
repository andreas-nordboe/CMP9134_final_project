using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using RobotManagementSystem.Client.Services;
using RobotManagementSystem.Client.Services.Robot;
using RobotManagementSystem.Shared.Models.Components;
using RobotManagementSystem.Shared.Models.Robot;
using Microsoft.JSInterop;
using RobotManagementSystem.Client.Modals;

namespace RobotManagementSystem.Client.Components;

public partial class CommanderControls : ComponentBase, IDisposable
{
    public int UsesAdvancedControls { get; set; }
    public Vector2D InputVector { get; set; } = new();

    private bool IsProcessingCommand { get; set; }
    private DotNetObjectReference<CommanderControls>? _dotNetReference;

    [Inject] private IRobotCommanderService RobotCommanderService { get; set; } = default!;
    [Inject] private RobotHubCommunication RobotHubCommunication { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IAppState AppState { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private ISoundService _soundService { get; set; } = default;

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

    protected override void OnInitialized()
    {
        RobotHubCommunication.TelemetryUpdated += OnTelemetryUpdated;
        RobotHubCommunication.ConnectionStatusChanged += OnConnectionStatusChanged;
        AppState.OnPendingRobotCommandTargetChanged += HandlePendingRobotCommandTargetChanged;
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        _dotNetReference = DotNetObjectReference.Create(this);

        await JSRuntime.InvokeVoidAsync(
            "robotKeyboardShortcuts.register",
            _dotNetReference);
    }

    private void OnTelemetryUpdated(RobotTelemetry telemetry)
    {
        InvokeAsync(StateHasChanged);
    }
    
    private void OnConnectionStatusChanged(string status)
    {
        if (status == RobotApiStatus.Disconnected ||
            status == RobotApiStatus.Reconnecting)
        {
            InvokeAsync(() =>
            {
                IsProcessingCommand = false;
                AppState.SetPendingRobotCommandTarget(null);
                StateHasChanged();
            });
        }
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
            
            AppState.SetPendingRobotCommandTarget(null);

            Snackbar.Add("Reset simulation command sent.", Severity.Warning);
        }
        catch
        {
            IsProcessingCommand = false;
            AppState.SetPendingRobotCommandTarget(null);
            Snackbar.Add("Could not send reset simulation command.", Severity.Error);
            await _soundService.PlayErrorSoundAsync();
        }
        finally
        {
            IsProcessingCommand = false;
            StateHasChanged();
        }
    }
    
    protected async Task ConfirmResetSimulation()
    {
        var options = new DialogOptions
        {
            CloseButton = true,
            MaxWidth = MaxWidth.ExtraSmall,
            FullWidth = true
        };

        var dialog = await DialogService.ShowAsync<ConfirmResetSimulationModal>(
            "Reset Simulation",
            options);

        var result = await dialog.Result;

        if (!result.Canceled && result.Data is bool confirmed && confirmed)
        {
            await ResetSimulation();
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
            await _soundService.PlayErrorSoundAsync();
            return;
        }

        var targetX = (int)LatestTelemetry.Position.X + deltaX;
        var targetY = (int)LatestTelemetry.Position.Y + deltaY;

        await SendMoveCommandAsync(targetX, targetY);
    }

    private async Task SendMoveCommandAsync(int x, int y)
    {
        if (!RobotHubCommunication.IsConnected)
        {
            Snackbar.Add("Robot connection is unavailable. Please wait for reconnection.", Severity.Warning);
            await _soundService.PlayErrorSoundAsync();
            return;
        }

        if (IsProcessingCommand)
        {
            Snackbar.Add("A movement command is already being processed.", Severity.Info);
            await _soundService.PlayErrorSoundAsync();
            return;
        }

        if (RobotHubCommunication.LatestTelemetry?.Status == nameof(RobotStatus.MOVING))
        {
            Snackbar.Add("Robot is already moving!", Severity.Info);
            await _soundService.PlayErrorSoundAsync();
            return;
        }

        if (RobotHubCommunication.LatestTelemetry?.Battery <= 0)
        {
            Snackbar.Add("Robot battery is empty!", Severity.Error);
            await _soundService.PlayErrorSoundAsync();
            return;
        }
        
        var tile = AppState.GetTileState(x, y);

        if (tile == null)
        {
            Snackbar.Add("You can't move outside the map!", Severity.Warning);
            await _soundService.PlayErrorSoundAsync();
            return;
        }

        if (tile.ContentType == GridTileType.Obstacle
            || tile.OriginalContentType == GridTileType.Obstacle
            || tile.ContentType == GridTileType.LidarHit)
        {
            Snackbar.Add("You can't move there!", Severity.Warning);
            await _soundService.PlayErrorSoundAsync();
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

            var response = await RobotCommanderService.MoveRobotAsync(new RobotNavigationRequest
            {
                X = x,
                Y = y
            });

            if (response == null || !response.Success)
            {
                AppState.SetPendingRobotCommandTarget(null);
                Snackbar.Add(response?.Message ?? "Move command failed.", Severity.Error);
                await _soundService.PlayErrorSoundAsync();
                return;
            }

            Snackbar.Add($"Move command sent to ({x}, {y}).", Severity.Success);
            await _soundService.PlayErrorSoundAsync();
        }
        catch
        {
            AppState.SetPendingRobotCommandTarget(null);
            Snackbar.Add("Could not send move command to robot.", Severity.Error);
            await _soundService.PlayErrorSoundAsync();
        }
        finally
        {
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
    
    private void HandlePendingRobotCommandTargetChanged(Vector2D? target)
    {
        InvokeAsync(() =>
        {
            IsProcessingCommand = target != null;
            StateHasChanged();
        });
    }
    
    private bool CanMoveRobotTo(int deltaX, int deltaY)
    {
        if (!CanSendCommand || LatestTelemetry?.Position == null)
            return false;

        var targetX = (int)LatestTelemetry.Position.X + deltaX;
        var targetY = (int)LatestTelemetry.Position.Y + deltaY;

        var tile = AppState.GetTileState(targetX, targetY);

        if (tile == null)
            return false;

        return tile.ContentType != GridTileType.Obstacle
               && tile.OriginalContentType != GridTileType.Obstacle
               && tile.ContentType != GridTileType.LidarHit;
    }
    
    [JSInvokable]
    public async Task HandleGlobalKeyboardInput(string key)
    {
        if (IsProcessingCommand)
            return;

        switch (key)
        {
            case "w":
                await MoveRobotUp();
                break;

            case "a":
                await MoveRobotLeft();
                break;

            case "s":
                await MoveRobotDown();
                break;

            case "d":
                await MoveRobotRight();
                break;

            case "shift+r":
                await ConfirmResetSimulation();
                break;
        }
    }
    
    public void Dispose()
    {
        RobotHubCommunication.TelemetryUpdated -= OnTelemetryUpdated;
        RobotHubCommunication.ConnectionStatusChanged -= OnConnectionStatusChanged;
        AppState.OnPendingRobotCommandTargetChanged -= HandlePendingRobotCommandTargetChanged;
        
        _ = JSRuntime.InvokeVoidAsync("robotKeyboardShortcuts.unregister");
        _dotNetReference?.Dispose();
    }
}