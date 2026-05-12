using System.Security.Authentication;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using RobotManagementSystem.Data;
using RobotManagementSystem.Services.FailureHandling;
using RobotManagementSystem.Services.Security;
using RobotManagementSystem.Shared.Models.Authentication;
using RobotManagementSystem.Shared.Models.Errors;
using RobotManagementSystem.Shared.Models.Users;

namespace RobotManagementSystem.Services.Authentication;


public class AuthenticationService : IAuthenticationService
{
    
    private readonly RobotApiDbContext _dbContext;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;
    private readonly IAPIFailureService _apiFailureService;
    

    public AuthenticationService(RobotApiDbContext dbContext, IPasswordService passwordService, ITokenService tokenService, IAPIFailureService apiFailureService)
    {
        _dbContext = dbContext;
        _passwordService = passwordService;
        _tokenService = tokenService;
        _apiFailureService = apiFailureService;
    }

    public async Task<AuthenticationResponse> LoginAsync(AuthenticationRequest loginRequest)
    {
        // Validate user input
        if (string.IsNullOrWhiteSpace(loginRequest.Username) || loginRequest.Username.Length < 5)
        {
            throw new BadHttpRequestException(ErrorMessages.InvalidUsername);
        }
        
        // Check that the password is not empty
        if (string.IsNullOrWhiteSpace(loginRequest.Password))
            throw new BadHttpRequestException(ErrorMessages.PasswordIsEmpty);
        
        // FR-02 fetch user data from database
        var dbUserAccount = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == loginRequest.Username);

        if (dbUserAccount == null)
            throw new InvalidCredentialException(ErrorMessages.InvalidCredentials);
            
        // Hash request password
        if (!_passwordService.VerifyPassword(loginRequest.Password, dbUserAccount.PasswordHash))
        {
            throw new InvalidCredentialException(ErrorMessages.PasswordsDoNotMatch);
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
            FirstName = dbUserAccount.FirstName,
            LastName = dbUserAccount.LastName,
            AccessToken = accessToken,
            Expires = _tokenService.GetAccessTokenExpiryTime(),
            Role = dbUserAccount.Role.ToString()
        };
        
        return response;
    }

    public async Task<AuthenticationResponse> RegisterAsync(RegisterUserRequest registerRequest)
    {
        if (registerRequest == null)
            throw new BadHttpRequestException(ErrorMessages.InvalidRequest);
        
        // FR-02 Verify and persist user details in a database
        // returning user was registered message for now
        
        // Validate user input
        
        if(string.IsNullOrWhiteSpace(registerRequest.Password) || string.IsNullOrWhiteSpace(registerRequest.ConfirmPassword))
            throw new BadHttpRequestException(ErrorMessages.PasswordIsEmpty);
        
        if(registerRequest.Password != registerRequest.ConfirmPassword)
            throw new BadHttpRequestException(ErrorMessages.PasswordsDoNotMatch);
        
        if (string.IsNullOrWhiteSpace(registerRequest.Username) || registerRequest.Username.Length < 5)
        {
            throw new BadHttpRequestException(ErrorMessages.UsernameLengthInvalid);
        }
        
        if (registerRequest.Password.Length < 6)
        {
            throw new BadHttpRequestException(ErrorMessages.PasswordLengthInvalid);
        }
        
        if(await _dbContext.Users.AnyAsync(u => u.Username == registerRequest.Username))
            throw new BadHttpRequestException(ErrorMessages.UsernameAlreadyExists);

        UserAccount newUser = new UserAccount
        {
            Username = registerRequest.Username,
            FirstName = registerRequest.FirstName,
            LastName = registerRequest.LastName,
            PasswordHash = _passwordService.HashPassword(registerRequest.Password),
            Role = UserRole.None
        };

        _dbContext.Add(newUser);
        await _dbContext.SaveChangesAsync();
        
        // FR-02 return response with JWT token to the client
        var accessToken = _tokenService.GenerateJWTToken(new AuthenticationTokenDTO
        {
            UserId = newUser.Id.ToString(), // SQLite fills this in automatically
            Username =  registerRequest.Username,
            Role = newUser.Role
        });
        
        // Return response to the client
        var response = new AuthenticationResponse()
        {
            UserId = newUser.Id.ToString(),
            Username =  newUser.Username,
            AccessToken = accessToken,
            Expires = _tokenService.GetAccessTokenExpiryTime(),
            Role = newUser.Role.ToString()
        };
        
        return response;
    }
}