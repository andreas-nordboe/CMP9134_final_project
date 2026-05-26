using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using MudBlazor;
using RobotManagementSystem.Client.Services;
using RobotManagementSystem.Client.Services.DataStore;
using RobotManagementSystem.Client.Services.Map;
using RobotManagementSystem.Client.Services.Robot;
using RobotManagementSystem.Client.Services.Sessions;
using RobotManagementSystem.Shared.Models.Components;
using RobotManagementSystem.Shared.Models.Map;
using RobotManagementSystem.Shared.Models.Robot;
using RobotManagementSystem.Shared.Models.Users;

namespace RobotManagementSystem.Client.Components;

public partial class GridComponent : ComponentBase, IDisposable
{
    private int GridWidth;
    private int GridHeight;

    [Inject] private RobotHubCommunication _robotHubCommunication { get; set; } = default!;
    [Inject] private IRobotCommanderService _robotCommanderService { get; set; }
    [Inject] private IDataStoreService DataStoreService { get; set; } = default!;
    [Inject] private IMapService _mapService { get; set; } = default!;
    [Inject] private ISoundService _soundService { get; set; } = default;
    [Inject] private IAppState _appState { get; set; }
    [Inject] private IUserSessionService _userSessionService { get; set; }
    [Inject] private ISnackbar _snackbar { get; set; }
    private bool UseLightMapTheme { get; set; }
    private bool ShowCoordinates { get; set; } = false;
    private bool ShowGroundTruthMap { get; set; } = true;
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
    private bool _wasRobotOnChargingStation;
    private bool _mapOutageEffect;
    
    // Lidar rings
    private bool _hasLidarData;

    private const int LidarMaxRange = 10;
    private const int LidarRingSpacing = 2;

    private bool HasLidarRangeRings =>
        !_mapOutageEffect &&
        IsMapReady &&
        _hasLidarData &&
        _robotX.HasValue &&
        _robotY.HasValue;
    
    // Proximity data
    private bool _hasProximityData;
    private const int ProximityMaxRange = 5;

    private int? _robotX;
    private int? _robotY;

    private int _northProximity = ProximityMaxRange;
    private int _eastProximity = ProximityMaxRange;
    private int _southProximity = ProximityMaxRange;
    private int _westProximity = ProximityMaxRange;
    
    
    private bool IsMapReady =>
        _mapLoadedSuccessfully &&
        GridWidth > 0 &&
        GridHeight > 0 &&
        Tiles.Count == GridWidth * GridHeight;

    private bool HasProximitySensors =>
        !_mapOutageEffect &&
        IsMapReady &&
        _hasProximityData &&
        _robotX.HasValue &&
        _robotY.HasValue;
    
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;
    
    // Sounds/effects
    private bool _showStuckEffect;
    private DateTime? _lastStuckEffectAt;
    private static readonly TimeSpan StuckEffectCooldown = TimeSpan.FromSeconds(2);
    private bool SoundEffectsEnabled { get; set; } = false;
    private bool _robotIsStuck;
    
    private string MapShellClass =>
        $"{(UseLightMapTheme ? "robot-map-shell robot-map-light" : "robot-map-shell")} " +
        $"{(_suppressTileTransitions ? "no-tile-transitions" : "")} " +
        $"{(_mapOutageEffect ? "map-outage-effect" : "")}";
    
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
        
        var storedShowGroundTruthMap = await DataStoreService.LoadShowGroundTruthMapAsync();
        ShowGroundTruthMap = storedShowGroundTruthMap ?? true;
        
        var storedPlaySoundEffects = await DataStoreService.LoadEnableSoundEffectsAsync();
        SoundEffectsEnabled = storedPlaySoundEffects ?? false;
        UseLightMapTheme = !_appState.IsDarkMode;
        

        if (_appState.CurrentUser != null && _appState.CurrentUser.Role != UserRole.NoRole)
        {
            _robotHubCommunication.AllowStart();
            await _robotHubCommunication.StartAsync();
        }

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
        InvokeAsync(() =>
        {
            _mapOutageEffect =
                status == RobotApiStatus.Disconnected ||
                status == RobotApiStatus.Reconnecting;

            if (_mapOutageEffect)
            {
                _lastTelemetryReceivedAt = null;
                ClearPendingCommand();
                _reloadMapOnNextTelemetry = true;
            }

            StateHasChanged();
        });
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
        
        _appState.CurrentTiles = Tiles;
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
        // WebSocket is open stop movement in the meantime (I think this is due ot a bug on the robot simulation API)
        if (_appState.ApiStatus == RobotApiStatus.Reconnecting)
            return;
        
        if (_reloadMapOnNextTelemetry)
        {
            _reloadMapOnNextTelemetry = false;
            await LoadMapAsync(forceReload: true);
        }
        else if (!_mapLoadedSuccessfully)
        {
            await LoadMapAsync(forceReload: false);
        }
        
        if (!IsMapReady)
        {
            _hasProximityData = false;
            return;
        }

        var robotX = (int)robotTelemetry.Position.X;
        var robotY = (int)robotTelemetry.Position.Y;
        
        _robotX = robotX;
        _robotY = robotY;

        UpdateProximitySensors(robotTelemetry);
        
        _robotIsStuck = robotTelemetry.Status == nameof(RobotStatus.STUCK);

        MoveRobot(robotX, robotY);

        await HandleChargeStationSoundEffect(robotX, robotY);

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
                    await _soundService.PlayErrorSoundAsync();
                    _stuckWarningShown = true;

                    await TriggerStuckFeedbackAsync();
                }
            }
            else if (robotTelemetry.Battery <= 0)
            {
                ClearPendingCommand();
                _snackbar.Add("Movement command cancelled because the robot battery is empty.", Severity.Error);
                await _soundService.PlayErrorSoundAsync();
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
                await _soundService.PlayErrorSoundAsync();
            }
        }
        else
        {
            _stuckWarningShown = robotTelemetry.Status == nameof(RobotStatus.STUCK);
        }

        _hasLidarData =
            robotTelemetry.Sensors?.Lidar != null &&
            robotTelemetry.Sensors.Lidar.Count > 0;

        if (!_hasLidarData)
        {
            ClearOldLidarHits();
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
            hit.ContentType = hit.OriginalContentType;
            hit.Label = null;
            hit.LidarIntensity = null;
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

                var hitBandDistance = Math.Ceiling(step / LidarRingSpacing) * LidarRingSpacing;
                hitBandDistance = Math.Clamp(hitBandDistance, LidarRingSpacing, LidarMaxRange);
                
                targetTile.LidarIntensity = hitBandDistance switch
                {
                    <= 2 => 1.00,
                    <= 4 => 0.92,
                    <= 6 => 0.82,
                    <= 8 => 0.72,
                    _ => 0.62
                };

                break;
            }

            if (targetTile.ContentType == GridTileType.FreeSpace ||
                targetTile.ContentType == GridTileType.LidarVisibility)
            {
                targetTile.ContentType = GridTileType.LidarVisibility;

                var bandDistance = Math.Ceiling(step / LidarRingSpacing) * LidarRingSpacing;
                bandDistance = Math.Clamp(bandDistance, LidarRingSpacing, LidarMaxRange);
                
                
                var bandOpacity = bandDistance switch
                {
                    <= 2 => 0.34,
                    <= 4 => 0.25,
                    <= 6 => 0.17,
                    <= 8 => 0.11,
                    _ => 0.07
                };

                targetTile.LidarIntensity = targetTile.LidarIntensity.HasValue
                    ? Math.Max(targetTile.LidarIntensity.Value, bandOpacity)
                    : bandOpacity;
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
        
        _appState.CurrentTiles = Tiles;
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
        if (_appState.CurrentUser == null ||
            _appState.CurrentUser.Role == UserRole.NoRole ||
            _appState.CurrentUser.Role == UserRole.Viewer)
        {
            _snackbar.Add("You are not authorised to move the robot.", Severity.Error);
            await _soundService.PlayErrorSoundAsync();
            return;
        }
        
        if (!_robotHubCommunication.IsConnected)
        {
            _snackbar.Add("Robot connection is unstable. Sending command so the server can retry and log it.", Severity.Warning);
            await _soundService.PlayErrorSoundAsync();
            //return;
        }

        if (IsCommandProcessing)
        {
            _snackbar.Add("A movement command is already being processed.", Severity.Info);
            await _soundService.PlayErrorSoundAsync();
            return;
        }

        if (_robotHubCommunication.LatestTelemetry?.Status == nameof(RobotStatus.MOVING))
        {
            _snackbar.Add("Robot is already moving!", Severity.Warning);
            await _soundService.PlayErrorSoundAsync();
            return;
        }

        if (_robotHubCommunication.LatestTelemetry?.Battery <= 0)
        {
            _snackbar.Add("Robot battery is empty!", Severity.Error);
            await _soundService.PlayErrorSoundAsync();
            return;
        }

        if (tile.ContentType == GridTileType.Obstacle
            || tile.OriginalContentType == GridTileType.Obstacle
            || tile.ContentType == GridTileType.LidarHit)
        {
            _snackbar.Add("This move may be blocked. Sending to server for validation and logging.", Severity.Warning);
            await _soundService.PlayErrorSoundAsync();
            // return;
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
            await _soundService.PlayErrorSoundAsync();
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
            GridTileType.Obstacle when ShowGroundTruthMap => "obstacle",
            GridTileType.Obstacle => "free-space",

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
        
        if (tile.ContentType == GridTileType.Robot && _robotIsStuck)
        {
            classes.Add("robot-crashed");
        }

        if (tile.ContentType == GridTileType.Robot && _showStuckEffect)
        {
            classes.Add("robot-stuck-effect");
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
    
    private async Task TriggerStuckFeedbackAsync()
    {
        var now = DateTime.UtcNow;

        if (_lastStuckEffectAt.HasValue &&
            now - _lastStuckEffectAt.Value < StuckEffectCooldown)
        {
            return;
        }

        _lastStuckEffectAt = now;
        _showStuckEffect = true;

        await InvokeAsync(StateHasChanged);

        // Give Blazor a moment to render .robot-stuck-effect before JS searches for it
        await Task.Delay(50);

        if (SoundEffectsEnabled)
        {
            // Sourced from: https://opengameart.org/content/short-alarm
            await _soundService.PlaySoundAsync("alarm.ogg");
        }

        await JsRuntime.InvokeVoidAsync("robotCrashEffects.explode");

        await Task.Delay(900);

        _showStuckEffect = false;
        await InvokeAsync(StateHasChanged);
    }
    
    private async Task OnShowCoordinatesChanged(bool value)
    {
        if (ShowCoordinates == value)
            return;

        ShowCoordinates = value;

        await DataStoreService.StoreShowMapCoordinatesAsync(value);

        await InvokeAsync(StateHasChanged);
    }
    
    private async Task OnShowGroundTruthMapChanged(bool value)
    {
        if (ShowGroundTruthMap == value)
            return;

        ShowGroundTruthMap = value;

        await DataStoreService.StoreShowGroundTruthMap(value);

        await InvokeAsync(StateHasChanged);
    }
    
    private async Task OnEnableSoundEffectsChanged(bool value)
    {
        if (SoundEffectsEnabled == value)
            return;

        SoundEffectsEnabled = value;

        await DataStoreService.StoreEnableSoundEffectsAsync(value);

        await InvokeAsync(StateHasChanged);
    }

    private async Task HandleChargeStationSoundEffect(int robotX, int robotY )
    {
        var isRobotOnChargingStation = robotX == 0 && robotY == 0;

        if (isRobotOnChargingStation && !_wasRobotOnChargingStation)
        {
            await _soundService.PlaySoundAsync("charge-station.mp3");
        }

        _wasRobotOnChargingStation = isRobotOnChargingStation;
    }
    
    private async Task OnShowCoordinatesKeyDown(KeyboardEventArgs e)
    {
        if (e.Key is not ("Enter" or " "))
            return;

        await OnShowCoordinatesChanged(!ShowCoordinates);
    }
    
    private async Task OnSoundEffectsKeyDown(KeyboardEventArgs e)
    {
        if (e.Key is not ("Enter" or " "))
            return;

        await OnEnableSoundEffectsChanged(!SoundEffectsEnabled);
    }
    
    private void UpdateProximitySensors(RobotTelemetry robotTelemetry)
    {
        if (robotTelemetry.Sensors == null)
        {
            _hasProximityData = false;
            return;
        }

        _northProximity = robotTelemetry.Sensors.N;
        _eastProximity = robotTelemetry.Sensors.E;
        _southProximity = robotTelemetry.Sensors.S;
        _westProximity = robotTelemetry.Sensors.W;

        _hasProximityData = true;
    }

    private string GetSensorBeamClass(string direction, int distance)
    {
        var severityClass = distance switch
        {
            <= 1 => "danger",
            <= 2 => "warning",
            >= ProximityMaxRange => "clear",
            _ => "info"
        };

        return $"proximity-beam proximity-beam-{direction.ToLowerInvariant()} proximity-beam-{severityClass}";
    }

    private string GetSensorBeamStyle(string direction, int distance)
    {
        if (!_robotX.HasValue || !_robotY.HasValue || GridWidth <= 0 || GridHeight <= 0)
            return string.Empty;

        var tileWidthPercent = 100d / GridWidth;
        var tileHeightPercent = 100d / GridHeight;

        var robotCenterX = (_robotX.Value + 0.5d) * tileWidthPercent;
        var robotCenterY = (_robotY.Value + 0.5d) * tileHeightPercent;

        var clampedDistance = Math.Clamp(distance, 0, ProximityMaxRange);

        // 0 means blocked/very close, so still show a short warning beam.
        var visibleDistance = distance <= 0
            ? 0.75d
            : Math.Max(1d, clampedDistance);

        var lengthPercent = direction is "N" or "S"
            ? visibleDistance * tileHeightPercent
            : visibleDistance * tileWidthPercent;

        var thicknessPercent = Math.Min(tileWidthPercent, tileHeightPercent) * 0.36d;
        var halfThicknessPercent = thicknessPercent / 2d;

        // Close objects are stronger. Clear directions are softer.
        var distanceRatio = clampedDistance / (double)ProximityMaxRange;
        var opacity = 0.95d - distanceRatio * 0.45d;

        return
            $"--sensor-x:{ToCssNumber(robotCenterX)}%;" +
            $"--sensor-y:{ToCssNumber(robotCenterY)}%;" +
            $"--sensor-length:{ToCssNumber(lengthPercent)}%;" +
            $"--sensor-thickness:{ToCssNumber(thicknessPercent)}%;" +
            $"--sensor-half-thickness:{ToCssNumber(halfThicknessPercent)}%;" +
            $"--sensor-opacity:{ToCssNumber(opacity)};";
    }

    private static string ToCssNumber(double value)
    {
        return value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
    }
    
    private string GetTileStyle(TileState tile)
    {
        if (tile.LidarIntensity.HasValue)
        {
            return $"--lidar-opacity:{ToCssNumber(tile.LidarIntensity.Value)};";
        }

        return string.Empty;
    }
    
    private IEnumerable<int> GetLidarRingDistances()
    {
        for (var distance = LidarRingSpacing; distance <= LidarMaxRange; distance += LidarRingSpacing)
        {
            yield return distance;
        }
    }

    private string GetLidarRangeRingStyle(int distance)
    {
        if (!_robotX.HasValue || !_robotY.HasValue || GridWidth <= 0 || GridHeight <= 0)
            return string.Empty;

        var tileWidthPercent = 100d / GridWidth;
        var tileHeightPercent = 100d / GridHeight;

        var robotCenterX = (_robotX.Value + 0.5d) * tileWidthPercent;
        var robotCenterY = (_robotY.Value + 0.5d) * tileHeightPercent;

        var ringWidth = distance * tileWidthPercent * 2d;
        var ringHeight = distance * tileHeightPercent * 2d;

        var distanceRatio = Math.Clamp(distance / (double)LidarMaxRange, 0.0, 1.0);

        // Exponential falloff: close rings are strong, far rings fade naturally.
        var falloff = Math.Pow(1.0 - distanceRatio, 1.85);

        var opacity = Math.Clamp(0.16 + falloff * 0.62, 0.16, 0.78);
        var thickness = Math.Clamp(2.2 - distanceRatio * 0.9, 1.1, 2.2);

        return
            $"--lidar-ring-x:{ToCssNumber(robotCenterX)}%;" +
            $"--lidar-ring-y:{ToCssNumber(robotCenterY)}%;" +
            $"--lidar-ring-width:{ToCssNumber(ringWidth)}%;" +
            $"--lidar-ring-height:{ToCssNumber(ringHeight)}%;" +
            $"--lidar-ring-opacity:{ToCssNumber(opacity)};" +
            $"--lidar-ring-thickness:{ToCssNumber(thickness)}px;";
    }
    
}