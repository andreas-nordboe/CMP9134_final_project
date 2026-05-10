using System.Security.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RobotManagementSystem.Data;
using RobotManagementSystem.Services.Authentication;
using RobotManagementSystem.Services.FailureHandling;
using RobotManagementSystem.Services.Security;
using RobotManagementSystem.Shared.Models.Authentication;
using RobotManagementSystem.Shared.Models.Errors;
using RobotManagementSystem.Shared.Models.Users;

namespace RobotManagementSystem.Controllers;

// POST /auth/login
// POST /auth/register
// GET /auth/account

// Sends robot actions by forwarding requests to the commands service
// This controller does not require the authorisation decroator as it creates tokens
// through athentication that provide authorisation to other protected controllers (secured endpoints) 

[ApiController]
[Route("auth/")]
public class AuthenticationController : ControllerBase
{
    private readonly IAPIFailureService _apiFailureService;
    private readonly ILogger<AuthenticationController> _logger;
    private readonly IAuthenticationService _authenticationService;
    
    public AuthenticationController(ITokenService tokenService, IAPIFailureService apiFailureService, RobotApiDbContext dbContext, IPasswordService passwordService, ILogger<AuthenticationController> logger, IAuthenticationService authenticationService)
    {
        _apiFailureService = apiFailureService;
        _logger = logger;
        _authenticationService = authenticationService;
    }
    
    [HttpPost("login")]
    public async Task<ActionResult<AuthenticationResponse>> Login(AuthenticationRequest? request)
    {
        if (request == null)
            return BadRequest(_apiFailureService.CreateApiError(ErrorCodes.EmptyRequest, ErrorMessages.EmptyRequest));

        try
        {
            var response = await _authenticationService.LoginAsync(request);
            return Ok(response);

        }
        catch (BadHttpRequestException badHttpRequestException)
        {
            _logger.LogError(badHttpRequestException, badHttpRequestException.Message);
            return BadRequest(badHttpRequestException.Message);
        }
        catch (InvalidCredentialException invalidCredentialException)
        {
            _logger.LogError(invalidCredentialException, invalidCredentialException.Message);
            return Unauthorized(invalidCredentialException.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred during login");
            return StatusCode(StatusCodes.Status500InternalServerError, _apiFailureService.CreateApiError(ErrorCodes.InternalServerError, ErrorMessages.InternalServerError));
        }
    }
    
    [HttpPost("register")]
    public async Task<ActionResult<AuthenticationResponse>> Register(RegisterUserRequest? request)
    {
        if (request == null)
            return BadRequest(_apiFailureService.CreateApiError(ErrorCodes.EmptyRequest, ErrorMessages.EmptyRequest));

        try
        {
            var response = await _authenticationService.RegisterAsync(request);
            return Ok(response);
        }
        catch (BadHttpRequestException badHttpRequestException)
        {
            _logger.LogError(badHttpRequestException, badHttpRequestException.Message);
            return BadRequest(badHttpRequestException.Message);
        }
        catch (InvalidCredentialException invalidCredentialException)
        {
            _logger.LogError(invalidCredentialException, invalidCredentialException.Message);
            return Unauthorized(invalidCredentialException.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred during user registration");
            return StatusCode(StatusCodes.Status500InternalServerError, _apiFailureService.CreateApiError(ErrorCodes.InternalServerError, ErrorMessages.InternalServerError));
        }
    }
}
