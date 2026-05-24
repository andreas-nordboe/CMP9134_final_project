using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RobotManagementSystem.Services.System;
using RobotManagementSystem.Services.SystemStatus;
using RobotManagementSystem.Shared.Models.SystemStatus;

namespace RobotManagementSystem.Controllers;

[ApiController]
[Route("system-status-logs")]
[Authorize (Roles = "Admin,Auditor")]
public class SystemStatusLogsController : ControllerBase
{
    private readonly ILogger<SystemStatusLogsController> _logger;
    private readonly ISystemStatusLogService _systemStatusLogService;

    public SystemStatusLogsController(ILogger<SystemStatusLogsController> logger, ISystemStatusLogService systemStatusLogService)
    {
        _logger = logger;
        _systemStatusLogService = systemStatusLogService;
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetLogs([FromQuery] SystemStatusEventType? eventType, DateTime? fromDate, int page = 1, int pageSize = 25)
    {
        if(page < 1)
            return BadRequest("Page must be greater or equal to 1.");
        
        if (pageSize < 1 || pageSize > 100)
            return BadRequest("Page size must be between 1 and 100.");

        var systemLogs = await _systemStatusLogService.GetSystemStatusLogsAsync(eventType, fromDate, page, pageSize);
        return Ok(systemLogs);
    }
    
    [HttpGet("summary")]
    public async Task<ActionResult<SystemStatusSummary>> GetSummary()
    {
        var summary = await _systemStatusLogService.GetSystemStatusSummaryAsync();
        return Ok(summary);
    }
}