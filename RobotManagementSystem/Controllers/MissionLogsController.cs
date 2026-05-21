using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RobotManagementSystem.Services.FailureHandling;
using RobotManagementSystem.Services.MissionLogs;
using RobotManagementSystem.Shared.Models.Errors;
using RobotManagementSystem.Shared.Models.MissionLog;

namespace RobotManagementSystem.Controllers;

// Retrieves persisted telemetry logs for auditors 
// GET /api/logs/commands
// GET /api/logs/security
// GET /api/logs/{id}

[ApiController]
[Route("mission-logs")]
public class MissionLogsController : ControllerBase
{
    private readonly ILogger<MissionLogsController> _logger;
    private readonly IMissionLogsService _missionLogsService;
    private readonly IAPIFailureService _apiFailureService;

    public MissionLogsController(ILogger<MissionLogsController> logger, IMissionLogsService missionLogsService, IAPIFailureService apiFailureService)
    {
        _logger = logger;
        _missionLogsService = missionLogsService;
        _apiFailureService = apiFailureService;
    }
    
    [HttpGet("all")]
    [Authorize(Roles = "Admin,Commander,Viewer,Auditor")]
    public async Task<ActionResult<List<MissionLog>>> GetAllMissionLogs()
    {
        try
        {
            var missionLogs = await _missionLogsService.GetAllMissionLogs();
            return Ok(missionLogs);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, _apiFailureService.CreateApiError(ErrorCodes.FailedToListMissionLogs, ErrorMessages.FailedToListMissionLogs));

        }
    }

    [HttpPost("add")]
    [Authorize(Roles = "Admin,Commander,Viewer,Auditor")]
    public async Task<ActionResult<MissionLog>> AddMissionLog([FromBody] AddMissionLogRequest missionLog)
    {
        try
        {
            var newMissionLog = await _missionLogsService.AddMissionLog(missionLog);
            return StatusCode((int)HttpStatusCode.Created, newMissionLog);
        }
        catch (ArgumentException argumentException)
        {
            _logger.LogWarning(argumentException, argumentException.Message);
            
            return BadRequest(_apiFailureService.CreateApiError(ErrorCodes.FailedToCreateMissionLog, ErrorMessages.FailedToCreateMissionLog));
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, _apiFailureService.CreateApiError(ErrorCodes.FailedToCreateMissionLog, ErrorMessages.FailedToCreateMissionLog));
        }
    }
}