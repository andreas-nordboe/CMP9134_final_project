using Microsoft.AspNetCore.Components;
using MudBlazor;
using RobotManagementSystem.Client.Services;
using RobotManagementSystem.Client.Services.DataStore;
using RobotManagementSystem.Client.Services.Map;
using RobotManagementSystem.Client.Services.Robot;
using RobotManagementSystem.Client.Services.Sessions;
using RobotManagementSystem.Shared.Models.Components;
using RobotManagementSystem.Shared.Models.Map;
using RobotManagementSystem.Shared.Models.Robot;

namespace RobotManagementSystem.Client.Components;

public partial class GridComponent : ComponentBase, IDisposable
{
    private int GridWidth;
    private int GridHeight;

    [Inject] private RobotHubCommunication _robotHubCommunication { get; set; } = default!;
    [Inject] private IRobotCommanderService _robotCommanderService { get; set; }
    [Inject] private IDataStoreService DataStoreService { get; set; } = default!;
    [Inject] private IMapService _mapService { get; set; } = default!;
    [Inject] private IAppState _appState { get; set; }
    [Inject] private IUserSessionService _userSessionService { get; set; }
    [Inject] private ISnackbar _snackbar { get; set; }
    private bool UseLightMapTheme { get; set; }
    private bool ShowCoordinates { get; set; } = true;
    private TileState? PendingCommandTile { get; set; }
    private bool IsCommandProcessing { get; set; }
    private bool _mapLoadedSuccessfully;
    private readonly SemaphoreSlim _mapLoadLock = new(1, 1);
    private bool _suppressTileTransitions;
    private bool _stuckWarningShown;
    private bool _reloadMapOnNextTelemetry = true;
    private DateTime? _pendingCommandStartedAt;
    private bool _hasSeenRobotMovingForPendingCommand;
    private DateTime? _lastTelemetryReceivedAt;
    private static readonly TimeSpan TelemetryGapReloadThreshold = TimeSpan.FromSeconds(3);

    private string MapShellClass =>
        $"{(UseLightMapTheme ? "robot-map-shell robot-map-light" : "robot-map-shell")} " +
        $"{(_suppressTileTransitions ? "no-tile-transitions" : "")}";
    
    protected List<TileState> Tiles { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        _suppressTileTransitions = true;

        _robotHubCommunication.TelemetryUpdated += OnTelemetryUpdated;
        _robotHubCommunication.ConnectionStatusChanged += OnConnectionStatusChanged;

        _appState.OnRobotReset += HandleResetGrid;
        _appState.OnDarkModeChanged += HandleDarkModeChanged;
        _appState.OnSignalRestored += HandleSignalRestored;
        _appState.OnPendingRobotCommandTargetChanged += HandlePendingRobotCommandTargetChanged;

        await LoadMapAsync();
        await LoadInitialRobotStateAsync();

        _reloadMapOnNextTelemetry = true;

        var storedShowCoordinates = await DataStoreService.LoadShowMapCoordinatesAsync();
        ShowCoordinates = storedShowCoordinates ?? false;

        UseLightMapTheme = !_appState.IsDarkMode;

        await _robotHubCommunication.StartAsync();

        StateHasChanged();

        await Task.Delay(150);

        _suppressTileTransitions = false;

        StateHasChanged();
    }

    private async Task LoadMapAsync(bool forceReload = false)
    {
        await _mapLoadLock.WaitAsync();

        try
        {
            if (_mapLoadedSuccessfully && !forceReload)
                return;

            var map = await _mapService.GetMapAsync();

            if (map != null)
            {
                GridWidth = map.Width;
                GridHeight = map.Height;
                LoadGridFromMapData(map);

                _mapLoadedSuccessfully = true;
                return;
            }

            _mapLoadedSuccessfully = false;

            // Initial fallback only if no map has been displayed
            if (Tiles.Count == 0)
            {
                GridWidth = 21;
                GridHeight = 21;
                SetupGrid();
            }
        }
        finally
        {
            _mapLoadLock.Release();
        }
    }
    
    private async void HandleSignalRestored()
    {
        await InvokeAsync(() =>
        {
            _suppressTileTransitions = true;
            ClearPendingCommand();
            _reloadMapOnNextTelemetry = true;
            StateHasChanged();
        });
    }
    
    private void OnConnectionStatusChanged(string status)
    {
        if (status == RobotApiStatus.Disconnected ||
            status == RobotApiStatus.Reconnecting)
        {
            InvokeAsync(() =>
            {
                _lastTelemetryReceivedAt = null;
                ClearPendingCommand();
                _reloadMapOnNextTelemetry = true;
                StateHasChanged();
            });
        }
    }
    
    private void ClearPendingCommand()
    {
        PendingCommandTile = null;
        IsCommandProcessing = false;
        _pendingCommandStartedAt = null;
        _hasSeenRobotMovingForPendingCommand = false;
        _appState.SetPendingRobotCommandTarget(null);
    }

    private async void HandleDarkModeChanged()
    {
        UseLightMapTheme = !_appState.IsDarkMode;
        await InvokeAsync(StateHasChanged);
    }
    
    private void LoadGridFromMapData(MapResponse map)
    {
        Tiles.Clear();

        for (int i = 0; i < map.Height; i++)
        {
            for (int j = 0; j < map.Width; j++)
            {
                bool isObstacle = map.Grid[i][j] == 1;
                var tileType = isObstacle ? GridTileType.Obstacle : GridTileType.FreeSpace;
                
                Tiles.Add(new TileState
                {
                    VectorPosition = new Vector2D
                    {
                        X = j,
                        Y = i
                    },
                    ContentType = tileType,
                    OriginalContentType = tileType
                });
            }
        }
        
        var chargingStationTile = GetTileState(0, 0);

        if (chargingStationTile != null)
        {
            chargingStationTile.ContentType = GridTileType.ChargingStation;
            chargingStationTile.OriginalContentType = GridTileType.ChargingStation;
        }
    }

    private void OnTelemetryUpdated(RobotTelemetry robotTelemetry)
    {
        InvokeAsync(async () =>
        {
            var now = DateTime.UtcNow;

            if (_lastTelemetryReceivedAt.HasValue &&
                now - _lastTelemetryReceivedAt.Value > TelemetryGapReloadThreshold)
            {
                _suppressTileTransitions = true;
                ClearPendingCommand();
                _reloadMapOnNextTelemetry = true;
            }

            _lastTelemetryReceivedAt = now;

            await ProcessTelemetryAsync(robotTelemetry);
            StateHasChanged();

            if (_suppressTileTransitions)
            {
                await Task.Delay(100);
                _suppressTileTransitions = false;
                StateHasChanged();
            }
        });
    }
    
    private async Task ProcessTelemetryAsync(RobotTelemetry robotTelemetry)
    {
        if (_reloadMapOnNextTelemetry)
        {
            _reloadMapOnNextTelemetry = false;
            await LoadMapAsync(forceReload: true);
        }
        else if (!_mapLoadedSuccessfully)
        {
            await LoadMapAsync(forceReload: false);
        }

        var robotX = (int)robotTelemetry.Position.X;
        var robotY = (int)robotTelemetry.Position.Y;

        MoveRobot(robotX, robotY);

        if (PendingCommandTile != null)
        {
            var reachedPendingTarget =
                PendingCommandTile.VectorPosition.X == robotX &&
                PendingCommandTile.VectorPosition.Y == robotY;

            var pendingAge = _pendingCommandStartedAt.HasValue
                ? DateTime.UtcNow - _pendingCommandStartedAt.Value
                : TimeSpan.Zero;

            if (robotTelemetry.Status == nameof(RobotStatus.MOVING))
            {
                _hasSeenRobotMovingForPendingCommand = true;
            }

            if (reachedPendingTarget)
            {
                ClearPendingCommand();
            }
            else if (robotTelemetry.Status == nameof(RobotStatus.STUCK))
            {
                ClearPendingCommand();

                if (!_stuckWarningShown)
                {
                    _snackbar.Add("Robot is stuck. Movement command was cancelled.", Severity.Warning);
                    _stuckWarningShown = true;
                }
            }
            else if (robotTelemetry.Battery <= 0)
            {
                ClearPendingCommand();
                _snackbar.Add("Movement command cancelled because the robot battery is empty.", Severity.Error);
            }
            else if (_hasSeenRobotMovingForPendingCommand &&
                     robotTelemetry.Status != nameof(RobotStatus.MOVING))
            {
                ClearPendingCommand();
            }
            else if (pendingAge > TimeSpan.FromSeconds(15))
            {
                ClearPendingCommand();
                _snackbar.Add("Movement command timed out.", Severity.Warning);
            }
        }
        else
        {
            _stuckWarningShown = robotTelemetry.Status == nameof(RobotStatus.STUCK);
        }

        var hasLidarData =
            robotTelemetry.Sensors?.Lidar != null &&
            robotTelemetry.Sensors.Lidar.Count > 0;

        if (!hasLidarData)
        {
            return;
        }

        ClearOldLidarHits();

        for (int angle = 0; angle < robotTelemetry.Sensors.Lidar.Count; angle++)
        {
            double distance = robotTelemetry.Sensors.Lidar[angle];

            if (distance <= 0 || distance > 10)
                continue;

            VisualiseLidarSensor(
                robotTelemetry.Position.X,
                robotTelemetry.Position.Y,
                angle,
                distance);
        }
    }
    
    private async Task LoadInitialRobotStateAsync()
    {
        try
        {
            var robotStatusResponse = await _robotCommanderService.GetRobotStatusAsync();

            if (robotStatusResponse == null)
                return;

            var robotTelemetry = new RobotTelemetry()
            {
                Position = robotStatusResponse.Position,
                Battery = robotStatusResponse.Battery,
                Status = robotStatusResponse.Status,
                Sensors = robotStatusResponse.Sensors
            };

            await ProcessTelemetryAsync(robotTelemetry);
        }
        catch
        {
            // Ignores initial robot load failure as SignalR telemetry will update the robot when available.
        }
    }

    private void ClearOldLidarHits()
    {
        var oldHits = Tiles.Where(t => t.ContentType == GridTileType.LidarHit || t.ContentType == GridTileType.LidarVisibility).ToList();
        foreach (var hit in oldHits)
        {
            hit.ContentType = hit.OriginalContentType; // todo fix bug that turns obstacle into free space
            hit.Label = null;
        }
    }

    private void VisualiseLidarSensor(int startX, int startY, int angle, double distance)
    {
        double angleRadians = angle * Math.PI / 180;

        for (double step = 0.5; step <= distance; step += 0.5)
        {
            int x = (int)Math.Round(startX + step * Math.Cos(angleRadians));
            int y = (int)Math.Round(startY + step * Math.Sin(angleRadians));

            var targetTile = GetTileState(x, y);

            if (targetTile == null)
                break;

            if (targetTile.ContentType == GridTileType.Robot)
                continue;

            if (targetTile.OriginalContentType == GridTileType.Obstacle ||
                targetTile.ContentType == GridTileType.Obstacle)
            {
                targetTile.ContentType = GridTileType.LidarHit;
                break;
            }

            if (targetTile.ContentType == GridTileType.FreeSpace)
            {
                targetTile.ContentType = GridTileType.LidarVisibility;
            }
        }
    }
    
    private async void HandleResetGrid()
    {
        await InvokeAsync(async () =>
        {
            _reloadMapOnNextTelemetry = true;

            await ResetGrid();
            StateHasChanged();
        });
    }

    protected async Task ResetGrid()
    {
        await LoadMapAsync(forceReload: true);
        StateHasChanged();
    }

    private void SetupGrid()
    {
        Tiles.Clear(); // just for safety

        for (int y = 0; y < GridHeight; y++)
        {
            for (int x = 0; x < GridWidth; x++)
            {
                TileState newTile = new TileState
                {
                    VectorPosition = new Vector2D
                    {
                        X = x,
                        Y = y
                    },
                    ContentType = GridTileType.FreeSpace,
                    OriginalContentType = GridTileType.FreeSpace
                };
                Tiles.Add(newTile);
            }
        }
    }

    protected TileState GetTileState(int x, int y)
    {
        return Tiles.FirstOrDefault(t => t.VectorPosition.X == x && t.VectorPosition.Y == y);
    }

    protected string GetTileCssClass(TileState tile)
    {
        switch (tile.ContentType)
        {
            case GridTileType.FreeSpace:
                return "free-space";
            case GridTileType.Obstacle:
                return "obstacle";
            case GridTileType.Robot:
                return "robot";
            case GridTileType.LidarHit:
                return "lidar-hit";
        }
        return "";
    }

    protected async Task OnTileClicked(TileState tile)
{
    if (!_robotHubCommunication.IsConnected)
    {
        _snackbar.Add("Robot connection is unavailable. Please wait for reconnection.", Severity.Warning);
        return;
    }

    if (IsCommandProcessing)
    {
        _snackbar.Add("A movement command is already being processed.", Severity.Info);
        return;
    }

    if (_robotHubCommunication.LatestTelemetry?.Status == nameof(RobotStatus.MOVING))
    {
        _snackbar.Add("Robot is already moving!", Severity.Info);
        return;
    }

    if (_robotHubCommunication.LatestTelemetry?.Battery <= 0)
    {
        _snackbar.Add("Robot battery is empty!", Severity.Error);
        return;
    }

    if (tile.ContentType == GridTileType.Obstacle
        || tile.OriginalContentType == GridTileType.Obstacle
        || tile.ContentType == GridTileType.LidarHit)
    {
        _snackbar.Add("You can't move there!", Severity.Warning);
        return;
    }

    try
    {
        IsCommandProcessing = true;
        PendingCommandTile = tile;
        _pendingCommandStartedAt = DateTime.UtcNow;
        _hasSeenRobotMovingForPendingCommand = false;

        _appState.SetPendingRobotCommandTarget(new Vector2D
        {
            X = tile.VectorPosition.X,
            Y = tile.VectorPosition.Y
        });

        await InvokeAsync(StateHasChanged);

        var response = await _robotCommanderService.MoveRobotAsync(new RobotNavigationRequest
        {
            X = tile.VectorPosition.X,
            Y = tile.VectorPosition.Y
        });

        if (response == null || !response.Success)
        {
            ClearPendingCommand();
            _snackbar.Add(response?.Message ?? "Move command failed.", Severity.Error);
            return;
        }

        _snackbar.Add($"Move command sent to ({tile.VectorPosition.X}, {tile.VectorPosition.Y}).", Severity.Success);
    }
    catch
    {
        ClearPendingCommand();
        _snackbar.Add("Could not send move command to robot! The simulation may currently be unavailable.", Severity.Error);
    }
    finally
    {
        await InvokeAsync(StateHasChanged);
    }
}
    
    private string GetTileClass(TileState tile)
    {
        var classes = new List<string> { "robot-tile" };

        classes.Add(tile.ContentType switch
        {
            GridTileType.Obstacle => "obstacle",
            GridTileType.Robot => "robot",
            GridTileType.LidarHit => "lidar-hit",
            GridTileType.LidarVisibility => "lidar-visibility",
            GridTileType.ChargingStation => "charging-station",
            _ => "free-space"
        });

        if (tile.OriginalContentType == GridTileType.ChargingStation)
        {
            classes.Add("charging-station-base");
        }

        if (PendingCommandTile == tile)
        {
            classes.Add("pending-command");
        }

        return string.Join(" ", classes);
    }
    
    protected void PlaceRobot(int x, int y, string? imageUrl = "/images/robot-image.png")
    {
        MoveRobot(x, y);
        
        var targetTile = GetTileState(x, y);
        if(targetTile is null) return;
        
        targetTile.ContentType = GridTileType.Robot;
        targetTile.ImageUrl = imageUrl;
        StateHasChanged();
    }
    
    protected void PlaceObstacle(int x, int y)
    {
        
    }

    protected void CenterRobot()
    {
        PlaceRobot((GridWidth / 2), (GridHeight / 2));
    }

    protected void MoveRobot(int x, int y)
    {
        foreach (var tile in Tiles.Where(t => t.ContentType == GridTileType.Robot))
        {
            tile.ContentType = tile.OriginalContentType;
            tile.ImageUrl = null;
        }

        var targetTile = GetTileState(x, y);
        if (targetTile is null) return;

        targetTile.ContentType = GridTileType.Robot;
        targetTile.ImageUrl = "/images/robot-image.png";
    }

    protected void ClearGrid()
    {
        foreach (var tile in Tiles)
        {
            tile.ContentType = GridTileType.FreeSpace;
            tile.ImageUrl = null;
            tile.Label = null;
        }
        
        StateHasChanged();
    }
    
    public void Dispose()
    {
        _robotHubCommunication.TelemetryUpdated -= OnTelemetryUpdated;
        _robotHubCommunication.ConnectionStatusChanged -= OnConnectionStatusChanged;

        _appState.OnRobotReset -= HandleResetGrid;
        _appState.OnDarkModeChanged -= HandleDarkModeChanged;
        _appState.OnSignalRestored -= HandleSignalRestored;
        _appState.OnPendingRobotCommandTargetChanged -= HandlePendingRobotCommandTargetChanged;
    }
    
    private void HandlePendingRobotCommandTargetChanged(Vector2D? target)
    {
        InvokeAsync(() =>
        {
            if (target == null)
            {
                PendingCommandTile = null;
                IsCommandProcessing = false;
                _pendingCommandStartedAt = null;
                _hasSeenRobotMovingForPendingCommand = false;
                StateHasChanged();
                return;
            }

            PendingCommandTile = GetTileState(target.X, target.Y);
            IsCommandProcessing = PendingCommandTile != null;

            if (IsCommandProcessing)
            {
                _pendingCommandStartedAt = DateTime.UtcNow;
                _hasSeenRobotMovingForPendingCommand = false;
            }

            StateHasChanged();
        });
    }
    
    private async Task OnShowCoordinatesChanged(bool value)
    {
        ShowCoordinates = value;
        await DataStoreService.StoreShowMapCoordinatesAsync(value);
    }
}