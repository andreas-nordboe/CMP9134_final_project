using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RobotManagementSystem.Data;
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
    private readonly ITokenService _tokenService;
    private readonly IAPIFailureService _apiFailureService;
    private readonly RobotApiDbContext _dbContext;
    private readonly IPasswordService _passwordService;
    private readonly ILogger<AuthenticationController> _logger;
    
    public AuthenticationController(ITokenService tokenService, IAPIFailureService apiFailureService, RobotApiDbContext dbContext, IPasswordService passwordService, ILogger<AuthenticationController> logger)
    {
        _tokenService = tokenService;
        _apiFailureService = apiFailureService;
        _dbContext = dbContext;
        _passwordService = passwordService;
        _logger = logger;
    }
    
    [HttpPost("login")]
    public async Task<ActionResult<AuthenticationResponse>> Login(AuthenticationRequest? request)
    {
        if (request == null)
            return BadRequest(_apiFailureService.CreateApiError(ErrorCodes.InvalidRequest, "Request cannot be empty."));
        
        // Validate user input
        if (string.IsNullOrWhiteSpace(request.Username) || request.Username.Length < 5)
        {
            return BadRequest(_apiFailureService.CreateApiError(ErrorCodes.InvalidUsername, "Username invalid, it must be at least 5 characters long."));
        }
        
        // Check that the password is not empty
        if (string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(_apiFailureService.CreateApiError(ErrorCodes.PasswordNotStrongEnough, "Password cannot be empty."));
        
        try
        {
            // FR-02 fetch user data from database
            var dbUserAccount = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            if (dbUserAccount == null)
                return Unauthorized("Invalid user login credentials");
            
            // Hash request password
            if (!_passwordService.VerifyPassword(request.Password, dbUserAccount.PasswordHash))
            {
                return Unauthorized("Invalid user login credentials");
            }
                
            // FR-02 return response with JWT token to the client
            var accessToken = _tokenService.GenerateJWTToken(new AuthenticationTokenDTO
            {
                UserId = dbUserAccount.Id.ToString(),
                Username =  dbUserAccount.Username,
                Role = dbUserAccount.Role
            });
            
            // Return response to the client
            var response = new AuthenticationResponse()
            {
                UserId = dbUserAccount.Id.ToString(),
                Username =  dbUserAccount.Username,
                FirstName =  dbUserAccount.FirstName,
                LastName =  dbUserAccount.LastName,
                AccessToken = accessToken,
                Expires = _tokenService.GetAccessTokenExpiryTime(),
                Role = dbUserAccount.Role.ToString()
            };

            return Ok(response);
            
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred during login");
            return StatusCode(StatusCodes.Status500InternalServerError, _apiFailureService.CreateApiError(ErrorCodes.InternalServerError, "Internal server error."));
        }
    }
    
    [HttpPost("register")]
    public async Task<ActionResult<AuthenticationResponse>> Register(RegisterUserRequest? request)
    {
        if (request == null)
            return BadRequest(_apiFailureService.CreateApiError(ErrorCodes.InvalidRequest, "Request cannot be empty."));
        
        // FR-02 Verify and persist user details in a database
        // returning user was registered message for now
        
        // Validate user input
        
        if(string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.ConfirmPassword))
            return BadRequest(_apiFailureService.CreateApiError(ErrorCodes.PasswordNotStrongEnough, "Passwords cannot be empty."));
        
        if(request.Password != request.ConfirmPassword)
            return BadRequest(_apiFailureService.CreateApiError(ErrorCodes.PasswordsDoNotMatch, "Passwords do not match."));
        
        if (string.IsNullOrWhiteSpace(request.Username) || request.Username.Length < 5)
        {
            return BadRequest(_apiFailureService.CreateApiError(ErrorCodes.InvalidUsername, "Username invalid, it must be at least 5 characters long."));
        }
        
        if (request.Password.Length < 6)
        {
            return BadRequest(_apiFailureService.CreateApiError(ErrorCodes.PasswordNotStrongEnough, "Password must be at least 6 characters long."));
        }
        
        if(await _dbContext.Users.AnyAsync(u => u.Username == request.Username))
            return BadRequest(_apiFailureService.CreateApiError(ErrorCodes.UserAlreadyExists, "Username already exists."));

        UserAccount newUser = new UserAccount
        {
            Username = request.Username,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PasswordHash = _passwordService.HashPassword(request.Password),
            Role = UserRole.NoRole
        };

        _dbContext.Add(newUser);
        await _dbContext.SaveChangesAsync();
        
        // FR-02 return response with JWT token to the client
        var accessToken = _tokenService.GenerateJWTToken(new AuthenticationTokenDTO
        {
            UserId = newUser.Id.ToString(), // SQLite fills this in automatically
            Username =  request.Username,
            Role = newUser.Role
        });
        
        // Return response to the client
        var response = new AuthenticationResponse()
        {
            UserId = newUser.Id.ToString(),
            Username =  newUser.Username,
            FirstName =  newUser.FirstName,
            LastName =  newUser.LastName,
            AccessToken = accessToken,
            Expires = _tokenService.GetAccessTokenExpiryTime(),
            Role = newUser.Role.ToString()
        };
        
        return Ok(response);
    }
}
