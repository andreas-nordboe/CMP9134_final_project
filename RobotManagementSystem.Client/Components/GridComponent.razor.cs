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

    private string MapShellClass =>
        UseLightMapTheme
            ? "robot-map-shell robot-map-light"
            : "robot-map-shell";
    
    protected List<TileState> Tiles { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadMapAsync();
        
        var storedShowCoordinates = await DataStoreService.LoadShowMapCoordinatesAsync();
        ShowCoordinates = storedShowCoordinates ?? false;
        
        _robotHubCommunication.TelemetryUpdated += OnTelemetryUpdated;
        await _robotHubCommunication.StartAsync(); // TODO this might be a bug as it could start multiple socket connections

        _appState.OnRobotReset += HandleResetGrid;
        _appState.OnDarkModeChanged += HandleDarkModeChanged;
        _appState.OnSignalRestored += HandleSignalRestored;
    }

    private async Task LoadMapAsync()
    {
        var map = await _mapService.GetMapAsync();

        if (map != null)
        {
            Console.WriteLine(map);
            
            GridWidth = map.Width;
            GridHeight = map.Height;
            LoadGridFromMapData(map);
            return;
        }
        
        if (Tiles.Count == 0)
        {
            GridWidth = 21;
            GridHeight = 21;
            SetupGrid(); // empty grid fallback
        }
    }
    
    private async void HandleSignalRestored()
    {
        await InvokeAsync(async () =>
        {
            await LoadMapAsync();
            StateHasChanged();
        });
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
        
        StateHasChanged(); // refreshes the grid
    }

    private void OnTelemetryUpdated(RobotTelemetry robotTelemetry)
    {
        InvokeAsync(() =>
        {
            ClearOldLidarHits();
            MoveRobot((int)robotTelemetry.Position.X, (int)robotTelemetry.Position.Y);
            
            if (robotTelemetry.Sensors.Lidar.Count > 0)
            {
                for (int angle = 0; angle < robotTelemetry.Sensors.Lidar.Count; angle++)
                {
                    double distance = robotTelemetry.Sensors.Lidar[angle];

                    if (distance <= 0 || distance > 10)
                    {
                        continue;
                    }
                
                    //OnLidarHit(hitX, hitY);
                    VisualiseLidarSensor(robotTelemetry.Position.X, robotTelemetry.Position.Y, angle, distance);
                }
            }
            
            StateHasChanged();
        });
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
            await ResetGrid();
            StateHasChanged();
        });
    }

    protected async Task ResetGrid()
    {
        var newMap = await _mapService.GetMapAsync();
        if (newMap != null)
        {
            GridWidth = newMap.Width;
            GridHeight = newMap.Height;
            LoadGridFromMapData(newMap);
            StateHasChanged();
        }
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
                    ContentType = GridTileType.FreeSpace
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

    protected async void OnTileClicked(TileState tile)
    {
        // TODO Refactor these into a service or robot movement safety handler later
        // I'm just doing client side validation here as well but the request could still be sent to the backend
        // however, from testing it seems like the backend handles this safely

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
        
        if (_robotHubCommunication.LatestTelemetry?.Battery <= 10)
        {
            _snackbar.Add("Robot battery is critically low! Please return to the charging station!", Severity.Warning);
        }
        
        if (_robotHubCommunication.LatestTelemetry?.Battery <= 25)
        {
            _snackbar.Add("Robot battery is low! Please return to the charging station!", Severity.Warning);
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
            await _robotCommanderService.MoveRobotAsync(new RobotNavigationRequest
            {
                X = tile.VectorPosition.X,
                Y = tile.VectorPosition.Y
            });
        }
        catch (Exception e)
        {
            _snackbar.Add("Could not send move command to robot! There simulation may currently be unavailable.", Severity.Error);
        }
        
        StateHasChanged();
    }

    protected void OnTileHovered(TileState tile)
    {
        // todo
    }

    protected void OnTileUnhovered(TileState tile)
    {
        // todo
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
        _appState.OnRobotReset -= HandleResetGrid;
        _appState.OnDarkModeChanged -= HandleDarkModeChanged;
        _appState.OnSignalRestored -= HandleSignalRestored;
    }
    
    private async Task OnShowCoordinatesChanged(bool value)
    {
        ShowCoordinates = value;
        await DataStoreService.StoreShowMapCoordinatesAsync(value);
    }
}