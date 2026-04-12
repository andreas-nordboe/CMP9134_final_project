using Microsoft.AspNetCore.Components;
using MudBlazor;
using RobotManagementSystem.Shared.Models.Components;

namespace RobotManagementSystem.Client.Components;

public partial class GridComponent : ComponentBase
{
    private const int GridWidth = 21;
    private const int GridHeight = 21;

    protected List<TileState> Tiles { get; set; } = new();

    protected override void OnInitialized()
    {
        SetupGrid();
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

    protected void OnTileClicked(TileState tile)
    {
        tile.ContentType = GridTileType.Obstacle;
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

    protected void PlaceRobot(int x, int y, string? imageUrl = "/images/robot-icon.png")
    {
        var targetTile = GetTileState(x, y);
        if(targetTile is null) return;
        
        targetTile.ContentType = GridTileType.Robot;
        targetTile.ImageUrl = null;
        StateHasChanged();
    }
    
    protected void PlaceObstacle(int x, int y)
    {
        
    }

    protected void CenterRobot()
    {
        PlaceRobot((GridWidth / 2), (GridHeight / 2), null);
    }

    protected void MoveRobot(int x, int y)
    {
        
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
}