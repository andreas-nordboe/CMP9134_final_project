using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using RobotManagementSystem.Data;
using RobotManagementSystem.Services.Security;
using RobotManagementSystem.Shared.Models.Authentication;

namespace RobotManagementSystem.Services.Authentication;


public class AuthenticationService : IAuthenticationService
{
    
    private readonly RobotApiDbContext _dbContext;
    private readonly IPasswordService _passwordService;
    

    public AuthenticationService(RobotApiDbContext dbContext, IPasswordService passwordService)
    {
        _dbContext = dbContext;
        _passwordService = passwordService;
    }

    public async Task<AuthenticationResponse> LoginAsync(AuthenticationRequest loginRequest)
    {
        // FR-02 fetch user data from database
        var dbUserAccount = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

        if (dbUserAccount == null)
            throw new UnauthorizedAccessException("Invalid user login credentials");
            
        // Hash request password
        if (!_passwordService.VerifyPassword(request.Password, dbUserAccount.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid user login credentials");
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
            AccessToken = accessToken,
            Expires = _tokenService.GetAccessTokenExpiryTime(),
            Role = dbUserAccount.Role.ToString()
        };
    }

    public async Task<AuthenticationResponse> RegisterAsync(RegisterUserRequest registerRequest)
    {
        throw new NotImplementedException();
    }
}