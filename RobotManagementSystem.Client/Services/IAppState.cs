using RobotManagementSystem.Client.Pages;
using RobotManagementSystem.Shared.Models;
using RobotManagementSystem.Shared.Models.Authentication;
using RobotManagementSystem.Shared.Models.Robot;
using RobotManagementSystem.Shared.Models.Users;

namespace RobotManagementSystem.Client.Services;

public interface IAppState
{
    User? CurrentUser { get; }
    public bool IsDarkMode { get; set; }
    public string ApiStatus { get; set; }
    public AuthenticationResponse? AuthenticationDetails { get; set; }

    event Action? OnUserChanged;
    event Action? OnDarkModeChanged;
    event Action? OnRobotReset;
    event Action? OnApiStatusChanged;
    event Action? OnSignalRestored;
    
    void SetLoggedInUserFromAuthentication(AuthenticationResponse authenticationResponse);
    void ClearUser();
    void ToggleDarkMode();
    bool IsUserLoggedIn();
    void NotifyRobotReset();
    void SetSignalDisrupted(bool isDisrupted);
}