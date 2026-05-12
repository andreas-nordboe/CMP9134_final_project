using Microsoft.AspNetCore.Components;
using MudBlazor;
using RobotManagementSystem.Client.Services.Robot;
using RobotManagementSystem.Shared.Models.Components;
using RobotManagementSystem.Shared.Models.Robot;

namespace RobotManagementSystem.Client.Components;

public partial class GridComponent : ComponentBase, IDisposable
{
    private const int GridWidth = 21;
    private const int GridHeight = 21;

    [Inject] private RobotHubCommunication _robotHubCommunication { get; set; } = default!;
    [Inject] private IRobotCommanderService _robotCommanderService { get; set; }

    protected List<TileState> Tiles { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        SetupGrid();

        _robotHubCommunication.TelemetryUpdated += OnTelemetryUpdated;

        await _robotHubCommunication.StartAsync();
    }

    private void OnTelemetryUpdated(RobotTelemetry robotTelemetry)
    {
        InvokeAsync(() =>
        {
            MoveRobot((int)robotTelemetry.Position.X, (int)robotTelemetry.Position.Y);
            StateHasChanged();
        });
        
       
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
    }
}