namespace RobotManagementSystem.Shared.Models.Map;

public class MapResponse
{
    public int Width { get; set; }
    public int Height { get; set; }
    public int[][] Grid { get; set; } = [];
}