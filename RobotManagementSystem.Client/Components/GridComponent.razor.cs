using Microsoft.AspNetCore.Components;
using MudBlazor;
using RobotManagementSystem.Client.Services;
using RobotManagementSystem.Client.Services.Map;
using RobotManagementSystem.Client.Services.Robot;
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
    [Inject] private IMapService _mapService { get; set; } = default!;
    [Inject] private IAppState _appState { get; set; }

    protected List<TileState> Tiles { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        var map = await _mapService.GetMapAsync();

        if (map != null)
        {
            Console.WriteLine(map);
            
            GridWidth = map.Width;
            GridHeight = map.Height;
            LoadGridFromMapData(map);
        }
        else
        {
            SetupGrid(); // empty grid fallback
        }
        
        _robotHubCommunication.TelemetryUpdated += OnTelemetryUpdated;
        await _robotHubCommunication.StartAsync(); // TODO this might be a bug as it could start multiple socket connections

        _appState.OnRobotReset += HandleResetGrid;
    }

    private void LoadGridFromMapData(MapResponse map)
    {
        Tiles.Clear();

        for (int i = 0; i < map.Height; i++)
        {
            for (int j = 0; j < map.Width; j++)
            {
                bool isObstacle = map.Grid[i][j] == 1;
                
                Tiles.Add(new TileState
                {
                    VectorPosition = new Vector2D
                    {
                        X = j,
                        Y = i
                    },
                    ContentType = isObstacle ? GridTileType.Obstacle : GridTileType.FreeSpace
                });
            }
        }
        StateHasChanged(); // refreshes the grid
    }

    private void OnTelemetryUpdated(RobotTelemetry robotTelemetry)
    {
        InvokeAsync(() =>
        {
            MoveRobot((int)robotTelemetry.Position.X, (int)robotTelemetry.Position.Y);

            ClearOldLidarHits();

            if (robotTelemetry.Sensors.Lidar.Count > 0)
            {
                for (int angle = 0; angle < robotTelemetry.Sensors.Lidar.Count; angle++)
                {
                    double distance = robotTelemetry.Sensors.Lidar[angle];

                    if (distance <= 0 || distance > 10)
                    {
                        continue;
                    }
                    
                    double angleRadius = angle * Math.PI / 180;

                    int hitX = (int)Math.Round(robotTelemetry.Position.X + (distance * Math.Cos((angleRadius))));
                    int hitY = (int)Math.Round(robotTelemetry.Position.Y + (distance * Math.Sin((angleRadius))));
                
                    OnLidarHit(hitX, hitY);
                }
            }
            
            StateHasChanged();
        });
    }

    private void ClearOldLidarHits()
    {
        var oldHits = Tiles.Where(t => t.ContentType == GridTileType.LidarHit).ToList();
        foreach (var hit in oldHits)
        {
            hit.ContentType = GridTileType.FreeSpace;
            hit.Label = null;
        }
    }
    
    private void OnLidarHit(int x, int y)
    {
        var targetTile = GetTileState(x, y);
        if (targetTile != null && targetTile.ContentType != GridTileType.Robot)
        {
            targetTile.ContentType = GridTileType.LidarHit;
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
        await _robotCommanderService.MoveRobotAsync(new RobotNavigationRequest
        {
            X = tile.VectorPosition.X,
            Y = tile.VectorPosition.Y
        });
        
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
            tile.ContentType = GridTileType.FreeSpace;
            tile.ImageUrl = null;
        }
        
        var targetTile = GetTileState(x, y);
        if(targetTile is null) return;
        
        targetTile.ContentType = GridTileType.Robot;
        targetTile.ImageUrl = "/images/robot-image.jpg";
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
    }
}