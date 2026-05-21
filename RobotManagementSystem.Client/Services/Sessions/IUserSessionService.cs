using RobotManagementSystem.Shared.Models.Authentication;

namespace RobotManagementSystem.Client.Services.Sessions;

public interface IUserSessionService
{
    void MonitorUserSession(AuthenticationResponse? authenticationResponse);
    void StopMonitoringUserSession();
    Task LogoutUserAfterExpiredSessionAsync();
}