namespace RobotManagementSystem.Services;

// Tasks: Validate user permissions, send commands to the robot, persist the command before it is sent (in case of latency drops), update command state, possibly handle errors and unknown states
// State machine would be useful here 
// States could include Sent, Confirmed, Failed and Unknown
// backoff could be useful (e.g., 1s, 2s, 4s, 8s, 15s..)

public class CommandService
{
    
}