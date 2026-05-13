using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using RobotManagementSystem.Client.Helpers;
using RobotManagementSystem.Client.Services;
using RobotManagementSystem.Client.Services.DataStore;
using RobotManagementSystem.Client.Services.Robot;
using RobotManagementSystem.Shared.Models.Users;

namespace RobotManagementSystem.Client.Layout;

public partial class MainLayout
{
    private bool _sidebarOpen = true; // open by default
    
    [Inject] private IAppState Appstate { get; set; }
    
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
        
        Appstate.OnUserChanged += StateHasChanged;
        Appstate.OnDarkModeChanged += StateHasChanged;
        Appstate.OnUserChanged += StateHasChanged;

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