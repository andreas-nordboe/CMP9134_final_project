using Microsoft.AspNetCore.Identity.Data;
using RobotManagementSystem.Shared.Models.Authentication;

namespace RobotManagementSystem.Services.Authentication;

// Tasks: Handle business logic like user registration and login and roles (viewer and commander), IAM, verify identity, issues claims  

public interface IAuthenticationService
{
    Task<AuthenticationResponse> LoginAsync(AuthenticationRequest loginRequest);
    Task<AuthenticationResponse> RegisterAsync(RegisterUserRequest registerRequest);
}