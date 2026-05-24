using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RobotManagementSystem.Services.FailureHandling;
using RobotManagementSystem.Services.MissionLogs;
using RobotManagementSystem.Shared.Models.Errors;
using RobotManagementSystem.Shared.Models.MissionLog;

namespace RobotManagementSystem.Controllers;

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
    [Authorize(Roles = "Admin,Auditor")]
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
}