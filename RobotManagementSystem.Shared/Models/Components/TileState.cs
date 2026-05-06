namespace RobotManagementSystem.Shared.Models.Components;

public class TileState
{
    public Vector2D VectorPosition { get; set; }

    public GridTileType ContentType { get; set; } = GridTileType.FreeSpace;
    
    public string? ImageUrl { get; set; }
    public string? Label { get; set; }
}