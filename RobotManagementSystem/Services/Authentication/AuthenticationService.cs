using Microsoft.AspNetCore.Identity.Data;
using RobotManagementSystem.Shared.Models.Authentication;

namespace RobotManagementSystem.Services.Authentication;



public class AuthenticationService : IAuthenticationService
{
    public Task<AuthenticationResponse> LoginAsync(LoginRequest loginRequest)
    {
        throw new NotImplementedException();
    }

    public Task<AuthenticationResponse> RegisterAsync(RegisterRequest registerRequest)
    {
        throw new NotImplementedException();
    }
}