namespace RobotManagementSystem.Shared.Models.Robot;

public class Sensors
{
    public int N { get; set; }
    public int S { get; set; }
    public int E { get; set; }
    public int W { get; set; }
    public List<double> Lidar { get; set; }
}