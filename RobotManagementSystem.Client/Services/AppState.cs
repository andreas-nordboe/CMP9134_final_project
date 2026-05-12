using Blazored.LocalStorage;
using RobotManagementSystem.Client.Helpers;
using RobotManagementSystem.Client.Pages;
using RobotManagementSystem.Shared.Models.Authentication;
using RobotManagementSystem.Shared.Models.Users;
using RobotManagementSystem.Shared.Models.Robot;

namespace RobotManagementSystem.Client.Services;

public class AppState : IAppState
{
    private readonly ILocalStorageService _localStorage;

    public AppState(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public User? CurrentUser { get; private set; }
    public bool IsDarkMode { get; set; }
    public string ApiStatus { get; set; } = RobotApiStatus.Disconnected;
    public AuthenticationResponse? AuthenticationDetails { get; set; }
    public event Action? OnUserChanged;
    public event Action? OnDarkModeChanged;

    public async void SetLoggedInUserFromAuthentication(AuthenticationResponse authenticationResponse)
    {
        CurrentUser = UserHelper.ToUser(authenticationResponse);
        AuthenticationDetails = authenticationResponse;
        NotifyUserChanged();
    }

    public bool IsUserLoggedIn()
    {
        return CurrentUser != null && CurrentUser.IsLoggedIn;
    }

    public void ClearUser()
    {
        CurrentUser = null;
        NotifyUserChanged();
    }

    public void ToggleDarkMode()
    {
        IsDarkMode = !IsDarkMode;
        OnDarkModeChanged?.Invoke();
    }

    private void NotifyUserChanged()
    {
        OnUserChanged?.Invoke();
    }
}