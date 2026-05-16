using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using RobotManagementSystem.Client.Helpers;
using RobotManagementSystem.Client.Services;
using RobotManagementSystem.Client.Services.DataStore;
using RobotManagementSystem.Client.Services.Robot;
using RobotManagementSystem.Shared.Models.Users;

namespace RobotManagementSystem.Client.Layout;

public partial class MainLayout : IDisposable
{
    private bool _sidebarOpen = true; // open by default
    [Inject] private NavigationManager NavigationManager { get; set; }
    
    [Inject] private IDataStoreService DataStore { get; set; }
    [Inject] private IAppState AppState { get; set; }
    [Inject] private RobotHubCommunication RobotHubCommunication { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        // Check for login from localstorage
        var auth = await DataStore.LoadAuthenticationDetailsAsync();

        if (auth != null && !JWTHelper.IsAccessTokenExpired(auth.AccessToken))
        {
            AppState.SetLoggedInUserFromAuthentication(auth);
            await RobotHubCommunication.StartAsync();
        }
        else
        {
            await DataStore.ClearAuthenticationDetailsAsync();
            AppState.ClearUser();
        }
        
        AppState.OnUserChanged += StateHasChanged;
        AppState.OnDarkModeChanged += StateHasChanged;
        AppState.OnUserChanged += StateHasChanged;
        AppState.OnApiStatusChanged += OnApiStatusChanged;

        if (AppState.CurrentUser is null || !AppState.CurrentUser.IsLoggedIn)
        {
            NavigationManager.NavigateTo("/login");
        }
        
        
        var isDarkMode = localStorage.GetItemAsync<bool>("IsDarkMode");
        if (isDarkMode.IsCompleted)
        {
            AppState.IsDarkMode = isDarkMode.Result;
            //Appstate.OnDarkModeChanged?.Invoke();
            StateHasChanged();
        }

        AppState.IsDarkMode = true; // Easier on the eyes while developing 
    }

    async void ToggleDarkMode()
    {
        AppState.IsDarkMode = !AppState.IsDarkMode;
        await localStorage.SetItemAsync("IsDarkMode", AppState.IsDarkMode);
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