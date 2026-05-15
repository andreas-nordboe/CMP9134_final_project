using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RobotManagementSystem.Data;
using RobotManagementSystem.Services;
using RobotManagementSystem.Services.FailureHandling;
using RobotManagementSystem.Shared.Models.Errors;
using RobotManagementSystem.Shared.Models.Robot;
using RobotManagementSystem.Shared.Models.Users;

namespace RobotManagementSystem.Controllers;

// GET /commands/status (only for commander) 
// POST /commands/move (only for commander)
// GET /commands/map (only for commander)
// POST /commands/reset (only for commander)
// GET /commands/sensor (viewer and commander)


[ApiController]
[Route("robot/commands/")]
public class RobotCommandsController : ControllerBase
{
    private readonly ILogger<RobotCommandsController> _logger;
    private readonly IRobotApiService _robotApiService;
    private readonly IAPIFailureService _apiFailureService;

    public RobotCommandsController(ILogger<RobotCommandsController> logger, IRobotApiService robotApiService, IAPIFailureService apiFailureService)
    {
        _logger = logger;
        _robotApiService = robotApiService;
        _apiFailureService = apiFailureService;
    }

    [HttpGet("status")]
    [Authorize(Roles = "Admin,Viewer,Commander,Auditor")]
    public async Task<ActionResult<RobotStatusResponse>> GetRobotStatus()
    {
        var robotStatus = await _robotApiService.GetRobotStatusAsync();

        if (robotStatus == null)
        {
            return StatusCode((int)HttpStatusCode.ServiceUnavailable, _apiFailureService.CreateApiError(ErrorCodes.RobotApiNotAvailable, ErrorMessages.RobotApiNotAvailable));
        }
        
        return Ok(robotStatus);
    }
    
    [HttpPost("move")]
    [Authorize(Roles = "Admin,Commander")]
    public async Task<ActionResult<RobotCommandResponse>> MoveRobot([FromBody] RobotNavigationRequest request)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRoleValue = User.FindFirstValue(ClaimTypes.Role);
        
        if(!int.TryParse(userRoleValue, out var userId))
        {
            // TODO return user id not found in token?
            return Unauthorized(_apiFailureService.CreateApiError(ErrorCodes.InvalidRequest, ErrorMessages.InvalidRequest));
        }
        
        var robotMoveResponse  = await _robotApiService.MoveRobotAsync(request, userId, Enum.Parse<UserRole>(userRoleValue));
        if (robotMoveResponse == null || !robotMoveResponse.Success)
        {
            return BadRequest(robotMoveResponse);
        }
        
        return Ok(robotMoveResponse);
    }
    
    [HttpPost("reset")]
    [Authorize(Roles = "Admin,Commander")]
    public async Task<IActionResult> ResetRobot()
    {
        return Ok(await _robotApiService.ResetAsync());
    }
    
}