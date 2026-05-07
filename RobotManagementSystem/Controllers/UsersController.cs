using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RobotManagementSystem.Data;
using RobotManagementSystem.Services.FailureHandling;
using RobotManagementSystem.Shared.Models.Errors;
using RobotManagementSystem.Shared.Models.Users;
using RobotManagementSystem.Mappers;

namespace RobotManagementSystem.Controllers;

// This controller manages admin and user-related actions
[ApiController]
[Route("users")]
public class UsersController : ControllerBase
{
    private readonly RobotApiDbContext _dbContext;
    private readonly IAPIFailureService _apiFailureService;

    public UsersController(RobotApiDbContext dbContext, IAPIFailureService apiFailureService)
    {
        _dbContext = dbContext;
        _apiFailureService = apiFailureService;
    }
    
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserAccountDto>> GetOwnUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(_apiFailureService.CreateApiError(ErrorCodes.InvalidRequest, ErrorMessages.InvalidRequest));
        }
        
        var user = await _dbContext.Users.FindAsync(userId);

        if (user == null)
        {
            return NotFound(_apiFailureService.CreateApiError(ErrorCodes.UserNotFound, ErrorMessages.UserNotFound));
        }

        return Ok(UserMapper.ToDto(user));
    }
    
    [HttpGet("{userId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserAccountDto>> GetUser(int userId)
    {
        var user = await _dbContext.Users.FindAsync(userId);

        if (user == null)
        {
            return NotFound(_apiFailureService.CreateApiError(ErrorCodes.UserNotFound, ErrorMessages.UserNotFound));
        }

        return Ok(UserMapper.ToDto(user));
    }
    
    [HttpPatch("{userId:int}/role")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserAccountDto>> UpdateUserRole(int userId, [FromBody] UserRole newRole)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        
        // Validate that role exists
        if (Enum.IsDefined(typeof(UserRole), newRole) == false)
        {
            return BadRequest(
                _apiFailureService.CreateApiError(ErrorCodes.InvalidRequest, ErrorMessages.InvalidRequest));
        }
        
        if (user == null)
        {
            return NotFound(_apiFailureService.CreateApiError(ErrorCodes.UserNotFound, ErrorMessages.UserNotFound));
        }
        
        user.Role = newRole;
        await _dbContext.SaveChangesAsync();

        return Ok(UserMapper.ToDto(user));
    }
    
    [HttpDelete("{userId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(int userId)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        
        if (user == null)
        {
            return NotFound(_apiFailureService.CreateApiError(ErrorCodes.UserNotFound, ErrorMessages.UserNotFound));
        }
        
        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();
        
        return NoContent();
    }
    
    
    
}