namespace RobotManagementSystem.Shared.Models.Robot;

public class Robot
{
    // Identifier for the robot
    public string Id { get; set; }
    
    // Current battery levels (0-100)
    public int Battery { get; set; }

    // Current operational status
    //public RobotStatus Status { get; set; }
}