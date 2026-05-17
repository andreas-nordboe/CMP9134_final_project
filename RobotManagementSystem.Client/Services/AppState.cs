using Blazored.LocalStorage;
using RobotManagementSystem.Client.Helpers;
using RobotManagementSystem.Client.Pages;
using RobotManagementSystem.Shared.Models.Authentication;
using RobotManagementSystem.Shared.Models.Components;
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
    public string _apiStatus { get; set; } = RobotApiStatus.Disconnected;
    public AuthenticationResponse? AuthenticationDetails { get; set; }
    public event Action? OnUserChanged;
    public event Action? OnDarkModeChanged;
    public event Action? OnRobotReset;
    public event Action? OnApiStatusChanged;
    public event Action? OnSignalRestored;
    public event Action<Vector2D?>? OnPendingRobotCommandTargetChanged;
    public bool IsSignalDisrupted { get; private set; }
    public Vector2D? PendingRobotCommandTarget { get; private set; }



    public string ApiStatus
    {
        get => _apiStatus;
        set
        {
            if(_apiStatus == value)
                return;
            
            _apiStatus = value;
            OnApiStatusChanged?.Invoke();
        }
    }

    public void SetPendingRobotCommandTarget(Vector2D? target)
    {
        PendingRobotCommandTarget = target;
        OnPendingRobotCommandTargetChanged?.Invoke(target);
    }

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

    public void NotifyRobotReset()
    {
        OnRobotReset?.Invoke();
    }

    public void SetSignalDisrupted(bool isDisrupted)
    {
        var wasDisrupted = IsSignalDisrupted;

        IsSignalDisrupted = isDisrupted;

        if (wasDisrupted && !isDisrupted)
        {
            OnSignalRestored?.Invoke();
        }
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