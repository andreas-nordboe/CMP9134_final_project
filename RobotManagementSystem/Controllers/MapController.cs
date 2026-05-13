using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RobotManagementSystem.Services;
using RobotManagementSystem.Services.FailureHandling;
using RobotManagementSystem.Shared.Models.Errors;
using RobotManagementSystem.Shared.Models.Map;
using RobotManagementSystem.Shared.Models.Robot;

namespace RobotManagementSystem.Controllers;

[ApiController]
public class MapController : ControllerBase
{
    private readonly IRobotApiService _robotApiService;
    private readonly IAPIFailureService _apiFailureService;

    public MapController(IAPIFailureService apiFailureService, IRobotApiService robotApiService)
    {
        _apiFailureService = apiFailureService;
        _robotApiService = robotApiService;
    }

    [HttpGet("map/")]
    [Authorize(Roles = "Admin,Viewer,Commander,Auditor")]
    public async Task<ActionResult<MapResponse>> GetRobotStatus()
    {
        var mapResponse = await _robotApiService.GetMapAsync();

        if (mapResponse == null)
        {
            return StatusCode((int)HttpStatusCode.ServiceUnavailable, _apiFailureService.CreateApiError(ErrorCodes.RobotApiNotAvailable, ErrorMessages.RobotApiNotAvailable));
        }
        
        return Ok(mapResponse);
    }
}