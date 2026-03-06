using Microsoft.AspNetCore.Mvc;
using RobotManagementSystem.Services.FailureHandling;
using RobotManagementSystem.Services.Security;
using RobotManagementSystem.Shared.Models.Authentication;
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
    private readonly ITokenService _tokenService;
    private readonly IAPIFailureService _apiFailureService;
    
    public AuthenticationController(ITokenService tokenService, IAPIFailureService apiFailureService)
    {
        _tokenService = tokenService;
        _apiFailureService = apiFailureService;
    }
    
    [HttpPost("login")]
    public async Task<ActionResult<AuthenticationResponse>> Login(AuthenticationRequest request)
    {
        string accessToken = string.Empty;
        
        // Validate user input
        if (request.Username == string.Empty || request.Username.Length < 5)
        {
            return BadRequest(_apiFailureService.CreateApiError(0, "Username invalid, it must be at least 5 characters long."));
        }
        
        try
        {
            // TODO: FR-02 This currently creates a new dummy userId but once persistence
            // has been implemented the UserId will be retrieved from the database
            accessToken = _tokenService.GenerateJWTToken(new AuthenticationTokenDTO
            {
                UserId = Guid.NewGuid().ToString(),
                Username =  request.Username,
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new AuthenticationResponse
            {
                UserId = string.Empty,
                Username =  string.Empty,
                AccessToken = string.Empty,
                Expires = DateTime.MinValue,
                Role = string.Empty,
                Error = new APIError
                {
                    ErrorCode = 0,
                    ErrorMessage = "Internal Server Error"
                }
            };
        }
        
        // TODO FR-02 fetch user data from database

        var response = new AuthenticationResponse()
        {
            UserId = Guid.NewGuid().ToString(),
            Username =  request.Username,
            AccessToken = accessToken,
            Expires = DateTime.MinValue,
            Role = UserRole.None.ToString()
        };

        return Ok(response);
    }
    
    [HttpPost("register")]
    public async Task<ActionResult<RegisterUserResponse>> Register(RegisterUserRequest request)
    {
        // TODO: FR-02 Verify and persist user details in a database
        // returning user was registered message for now
        
        // Validate user input
        if (request.Password.Length < 6)
        {
            
        }

        return Ok("User was successfully registered.");
    }
}
