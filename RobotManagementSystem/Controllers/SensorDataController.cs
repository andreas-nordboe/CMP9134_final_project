using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RobotManagementSystem.Services;
using RobotManagementSystem.Services.FailureHandling;
using RobotManagementSystem.Shared.Models.Errors;
using RobotManagementSystem.Shared.Models.Map;

namespace RobotManagementSystem.Controllers;

[ApiController]
public class SensorDataController : ControllerBase
{
    private readonly IRobotApiService _robotApiService;
    private readonly IAPIFailureService _apiFailureService;

    public SensorDataController(IRobotApiService robotApiService, IAPIFailureService apiFailureService)
    {
        _robotApiService = robotApiService;
        _apiFailureService = apiFailureService;
    }
    
    [HttpGet("sensor/")]
    [Authorize(Roles = "Admin,Viewer,Commander,Auditor")]
    public async Task<ActionResult<MapResponse>> GetSensorData()
    {
        var sensorDataResponse = await _robotApiService.GetSensorDataAsync();

        if (sensorDataResponse == null)
        {
            return StatusCode((int)HttpStatusCode.ServiceUnavailable, _apiFailureService.CreateApiError(ErrorCodes.RobotApiNotAvailable, ErrorMessages.RobotApiNotAvailable));
        }
        
        return Ok(sensorDataResponse);
    }
}