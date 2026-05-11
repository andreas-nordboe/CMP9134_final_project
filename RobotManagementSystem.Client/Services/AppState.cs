using RobotManagementSystem.Client.Pages;
using RobotManagementSystem.Shared.Models.Users;

namespace RobotManagementSystem.Client.Services;

public class AppState : IAppState
{
    public User? CurrentUser { get; private set; }
    public bool IsDarkMode { get; set; }
    public event Action? OnUserChanged;
    public event Action? OnDarkModeChanged;

    public void SetLoggedInUser(User user)
    {
        CurrentUser = user;
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