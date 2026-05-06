using RobotManagementSystem.Shared.Models.Authentication;
using RobotManagementSystem.Shared.Models.Users;

namespace RobotManagementSystem.Client.Services;

public interface IAuthenticationService
{
    Task<AuthenticationResponse> LoginUserAsync(LoginDetails loginDetails);
}