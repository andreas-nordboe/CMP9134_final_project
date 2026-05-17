using Microsoft.AspNetCore.Components;
using MudBlazor;
using RobotManagementSystem.Client.Services;
using RobotManagementSystem.Client.Services.Robot;
using RobotManagementSystem.Shared.Models.Robot;

namespace RobotManagementSystem.Client.Components;

public partial class RobotTelemetryComponent : ComponentBase, IDisposable
{
    [Inject] public RobotHubCommunication RobotHubCommunication { get; set; } = default!;
    [Inject] public IAppState AppState { get; set; } = default!;

    private const double LidarMaxRange = 10.0;
    private const double LidarMaxRangeTolerance = 0.05;

    private string? ConnectionStatus { get; set; }
    private RobotTelemetry? LatestTelemetry { get; set; }

    private IReadOnlyList<double> LidarReadings =>
        LatestTelemetry?.Sensors?.Lidar ?? [];

    private IEnumerable<double> ValidLidarReadings =>
        LidarReadings.Where(distance => distance > 0);

    private IEnumerable<double> ObstacleHits =>
        ValidLidarReadings.Where(distance => distance < LidarMaxRange - LidarMaxRangeTolerance);

    private double? NearestObstacleDistance =>
        ObstacleHits.Any()
            ? ObstacleHits.Min()
            : null;

    private int LidarRayCount =>
        LidarReadings.Count;

    private int LidarHitCount =>
        ObstacleHits.Count();

    protected override void OnInitialized()
    {
        // Set this immediately first as it may have been cached during login.
        ConnectionStatus = AppState.ApiStatus;

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

    private Color GetConnectionColor()
    {
        return ConnectionStatus == RobotApiStatus.Connected
            ? Color.Success
            : ConnectionStatus == RobotApiStatus.Reconnecting
                ? Color.Warning
                : Color.Error;
    }

    private Color GetBatteryColor()
    {
        if (LatestTelemetry == null)
            return Color.Default;

        return LatestTelemetry.Battery switch
        {
            >= 70 => Color.Success,
            >= 25 => Color.Warning,
            _ => Color.Error
        };
    }

    private Color GetRobotStatusColor()
    {
        if (LatestTelemetry == null)
            return Color.Default;

        return LatestTelemetry.Status switch
        {
            nameof(RobotStatus.IDLE) => Color.Success,
            nameof(RobotStatus.MOVING) => Color.Info,
            nameof(RobotStatus.CHARGING) => Color.Warning,
            _ => Color.Default
        };
    }

    private string LidarStatusText
    {
        get
        {
            if (LidarRayCount == 0)
                return "No data";

            if (NearestObstacleDistance is null)
                return "Clear";

            if (NearestObstacleDistance <= 2)
                return "Close obstacle";

            if (NearestObstacleDistance <= 4)
                return "Obstacle nearby";

            return "Scanning";
        }
    }

    private string NearestObstacleText =>
        NearestObstacleDistance.HasValue
            ? $"Closest object: {NearestObstacleDistance.Value:0.0} tiles"
            : "Closest object: clear";

    private string LidarHitCountText
    {
        get
        {
            if (LidarRayCount == 0)
                return "No lidar readings";

            return $"{LidarHitCount} readings in range / {LidarRayCount} rays";
        }
    }

    private Color GetLidarColor()
    {
        if (LidarRayCount == 0)
            return Color.Default;

        if (NearestObstacleDistance is null)
            return Color.Success;

        if (NearestObstacleDistance <= 2)
            return Color.Error;

        if (NearestObstacleDistance <= 4)
            return Color.Warning;

        return Color.Info;
    }

    public void Dispose()
    {
        RobotHubCommunication.ConnectionStatusChanged -= OnConnectionStatusChanged;
        RobotHubCommunication.TelemetryUpdated -= OnTelemetryUpdated;
    }
}