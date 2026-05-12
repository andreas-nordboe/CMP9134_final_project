using Microsoft.AspNetCore.Components;
using RobotManagementSystem.Client.Services.Robot;
using RobotManagementSystem.Shared.Models.Components;
using RobotManagementSystem.Shared.Models.Robot;

namespace RobotManagementSystem.Client.Components;

public partial class CommanderControls : ComponentBase
{
    public int UsesAdvancedControls { get; set; }
    public Vector2D InputVector { get; set; } = new();
    
    [Inject]
    IRobotCommanderService RobotCommanderService { get; set; }

    protected void MoveRobot()
    {
        if (UsesAdvancedControls == 1)
        {
            // Send specific X and Y coordinates to the server using InputVector
            RobotCommanderService.MoveRobotAsync(new RobotNavigationRequest
            {
                X = InputVector.X,
                Y = InputVector.Y
            });
        }
    }

    protected void EmergencyStop()
    {
        RobotCommanderService.ResetAsync();
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