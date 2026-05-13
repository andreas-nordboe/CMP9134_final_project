namespace RobotManagementSystem.Shared.Models.Robot;

public class SensorData
{
    public double N { get; set; }
    public double S { get; set; }
    public double E { get; set; }
    public double W { get; set; }
    public List<double> Lidar { get; set; } = new List<double>();
}