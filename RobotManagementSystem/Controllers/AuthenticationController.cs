using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RobotManagementSystem.Data;
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
    private readonly RobotApiDbContext _dbContext;
    
    public AuthenticationController(ITokenService tokenService, IAPIFailureService apiFailureService, RobotApiDbContext dbContext)
    {
        _tokenService = tokenService;
        _apiFailureService = apiFailureService;
        _dbContext = dbContext;
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
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == request.Username) == null;

            if (user == null)
                return Unauthorized("Invalid user login credentials");
            
            // TODO: hash request.password and return hased variable
            var hashedPassword = "TODO";
            
            if(request.Password != hashedPassword)
                return Unauthorized("Invalid user login credentials");
                
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
        
        if(await _dbContext.Users.AnyAsync(u => u.Username == request.Username))
            return BadRequest(_apiFailureService.CreateApiError(0, "Username already exists."));

        UserAccount newUser = new UserAccount
        {
            Username = request.Username,
            PasswordHash = "HashPasswordLater",
            Role = UserRole.None
        };

        _dbContext.Add(newUser);
        await _dbContext.SaveChangesAsync();
        
        // Validate user input
        if (request.Password.Length < 6)
        {
            
        }

        return Ok("User was successfully registered.");
    }
}
