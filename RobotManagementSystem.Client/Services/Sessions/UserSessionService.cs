using Microsoft.AspNetCore.Components;
using MudBlazor;
using RobotManagementSystem.Client.Helpers;
using RobotManagementSystem.Client.Services.DataStore;
using RobotManagementSystem.Shared.Models.Authentication;

namespace RobotManagementSystem.Client.Services.Sessions;

public class UserSessionService : IUserSessionService
{
    private readonly IDataStoreService _dataStore;
    private readonly IAppState _appState;
    private readonly NavigationManager _navigationManager;
    private readonly ISnackbar _snackbar;
    
    private Timer? _sessionExpiryWarningTimer;
    private Timer? _logoutTimer;

    public UserSessionService(IDataStoreService dataStore, IAppState appState, NavigationManager navigationManager, ISnackbar snackbar)
    {
        _dataStore = dataStore;
        _appState = appState;
        _navigationManager = navigationManager;
        _snackbar = snackbar;
    }
    
    public void MonitorUserSession(AuthenticationResponse? authenticationResponse)
    {
        StopMonitoringUserSession();
        
        if(string.IsNullOrWhiteSpace(authenticationResponse?.AccessToken))
            return;
        
        var tokenExpiryTime = JWTHelper.GetTokenExpiryTime(authenticationResponse?.AccessToken);
        var currentTime = DateTime.UtcNow;

        // 2 minute buffer to warn user early
        var warningTime = tokenExpiryTime.AddMinutes(-2) - currentTime;
        var logoutDelay = tokenExpiryTime - currentTime;

        if (warningTime > TimeSpan.Zero)
        {
            _sessionExpiryWarningTimer = new Timer(_ =>
            {
                _snackbar.Add("Login session is about to expire. Please log in again after logout.", Severity.Warning);
            }, null, warningTime, Timeout.InfiniteTimeSpan);
        }

        if (logoutDelay  > TimeSpan.Zero)
        {
            _logoutTimer = new Timer(async _ =>
            {
                await LogoutUserAfterExpiredSessionAsync();
            }, null, logoutDelay, Timeout.InfiniteTimeSpan);
        }
        else
        {
            _ = LogoutUserAfterExpiredSessionAsync();
        }
        
    }
    
    
    public void StopMonitoringUserSession()
    {
        _sessionExpiryWarningTimer?.Dispose();
        _logoutTimer?.Dispose();
        
        _sessionExpiryWarningTimer = null;
        _logoutTimer = null;
    }
    
    public async Task LogoutUserAfterExpiredSessionAsync()
    {
        await _dataStore.ClearAuthenticationDetailsAsync();
        _appState.ClearUser();
        
        _snackbar.Add("Login session has expired. Please log in again.", Severity.Warning);
        
        
        _navigationManager.NavigateTo("/login", forceLoad: true);
    }
    
}