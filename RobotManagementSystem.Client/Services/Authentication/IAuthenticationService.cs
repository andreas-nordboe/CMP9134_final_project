using RobotManagementSystem.Shared.Models.Authentication;
using RobotManagementSystem.Shared.Models.Users;

namespace RobotManagementSystem.Client.Services.Authentication;

public interface IAuthenticationService
{
    Task<AuthenticationResponse> LoginUserAsync(LoginDetails loginDetails);
    Task<AuthenticationResponse> RegisterUserAsync(RegisterUserRequest registerUserDetails);
}