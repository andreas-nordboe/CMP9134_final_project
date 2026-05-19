using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using RobotManagementSystem.Client.Helpers;
using RobotManagementSystem.Client.Services;
using RobotManagementSystem.Client.Services.DataStore;
using RobotManagementSystem.Client.Services.Robot;
using RobotManagementSystem.Client.Services.Sessions;
using RobotManagementSystem.Shared.Models.Users;

namespace RobotManagementSystem.Client.Layout;

public partial class MainLayout : IDisposable
{
    private bool _sidebarOpen = true; // open by default
    [Inject] private NavigationManager NavigationManager { get; set; }
    
    [Inject] private IDataStoreService DataStore { get; set; }
    [Inject] private IAppState AppState { get; set; }
    [Inject] private RobotHubCommunication RobotHubCommunication { get; set; }
    [Inject] private IUserSessionService UserSessionService { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        // Check for login from localstorage
        var auth = await DataStore.LoadAuthenticationDetailsAsync();

        if (auth != null && !JWTHelper.IsAccessTokenExpired(auth.AccessToken))
        {
            AppState.SetLoggedInUserFromAuthentication(auth);
            UserSessionService.MonitorUserSession(auth);
            await RobotHubCommunication.StartAsync();
        }
        else
        {
            await DataStore.ClearAuthenticationDetailsAsync();
            AppState.ClearUser();
            await RobotHubCommunication.StopAsync();
        }
        
        AppState.OnUserChanged += StateHasChanged;
        AppState.OnDarkModeChanged += StateHasChanged;
        AppState.OnApiStatusChanged += OnApiStatusChanged;

        if (AppState.CurrentUser is null || !AppState.CurrentUser.IsLoggedIn)
        {
            NavigationManager.NavigateTo("/login");
        }
        
        
        var isDarkMode = await localStorage.GetItemAsync<bool>("IsDarkMode");
        if (isDarkMode)
        {
            AppState.IsDarkMode = isDarkMode;
            //Appstate.OnDarkModeChanged?.Invoke();
            StateHasChanged();
        }
    }

    void ToggleSidebar()
    {
        _sidebarOpen = !_sidebarOpen;
    }
    
    private void OnApiStatusChanged()
    {
        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        AppState.OnUserChanged -= StateHasChanged;
        AppState.OnDarkModeChanged -= StateHasChanged;
        AppState.OnApiStatusChanged -= OnApiStatusChanged;
    }
}