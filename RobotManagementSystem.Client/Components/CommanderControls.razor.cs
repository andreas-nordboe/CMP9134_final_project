using Microsoft.AspNetCore.Components;
using RobotManagementSystem.Shared.Models.Components;

namespace RobotManagementSystem.Client.Components;

public partial class CommanderControls : ComponentBase
{
    public int UsesAdvancedControls { get; set; }
    public Vector2D InputVector { get; set; } = new();

    protected void MoveRobot()
    {
        if (UsesAdvancedControls == 1)
        {
            // Send specific X and Y coordinates to the server using InputVector
        }
    }

    protected void EmergencyStop()
    {
        
    }

    protected void MoveRobotLeft()
    {
        
    }
    
    protected void MoveRobotRight()
    {
        
    }
    
    protected void MoveRobotUp()
    {
        
    }
    
    protected void MoveRobotDown()
    {
        
    }
    
}