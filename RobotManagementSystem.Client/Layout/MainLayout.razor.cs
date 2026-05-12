using Microsoft.AspNetCore.Components;
using RobotManagementSystem.Client.Helpers;
using RobotManagementSystem.Client.Services;
using RobotManagementSystem.Client.Services.DataStore;
using RobotManagementSystem.Shared.Models.Users;

namespace RobotManagementSystem.Client.Layout;

public partial class MainLayout
{
    private bool _sidebarOpen = true; // open by default
    
    [Inject] private IAppState Appstate { get; set; }
    
    [Inject] private NavigationManager NavigationManager { get; set; }
    
    [Inject] private IDataStoreService DataStore { get; set; }
    [Inject] private IAppState AppState { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        // Check for login
        var auth = await DataStore.LoadAuthenticationDetailsAsync();

        if (auth != null && JWTHelper.IsAccessTokenExpired(auth.AccessToken))
        {
            AppState.SetLoggedInUserFromAuthentication(auth);
        }
        else
        {
            await DataStore.ClearAuthenticationDetailsAsync();
            AppState.ClearUser();
        }
        
        Appstate.OnUserChanged += StateHasChanged;
        Appstate.OnDarkModeChanged += StateHasChanged;

        if (Appstate.CurrentUser is null || !Appstate.CurrentUser.IsLoggedIn)
        {
            NavigationManager.NavigateTo("/login");
        }
        
        
        var isDarkMode = localStorage.GetItemAsync<bool>("IsDarkMode");
        if (isDarkMode.IsCompleted)
        {
            Appstate.IsDarkMode = isDarkMode.Result;
            //Appstate.OnDarkModeChanged?.Invoke();
            StateHasChanged();
        }

        Appstate.IsDarkMode = true; // Easier on the eyes while developing 
    }

    async void ToggleDarkMode()
    {
        Appstate.IsDarkMode = !Appstate.IsDarkMode;
        await localStorage.SetItemAsync("IsDarkMode", Appstate.IsDarkMode);
    }

    void ToggleSidebar()
    {
        _sidebarOpen = !_sidebarOpen;
    }

    public void Dispose()
    {
        Appstate.OnUserChanged -= StateHasChanged;
        Appstate.OnDarkModeChanged -= StateHasChanged;
    }
}