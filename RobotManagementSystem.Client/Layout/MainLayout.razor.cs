using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using MudBlazor;
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
    [Inject] private ISnackbar Snackbar { get; set; }
    
    private void UpdateSnackbarPosition()
    {
        Snackbar.Configuration.PositionClass =
            AppState.IsUserLoggedIn()
                ? Defaults.Classes.Position.TopLeft
                : Defaults.Classes.Position.TopCenter;
        
        if (!AppState.IsUserLoggedIn())
        {
            Snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopCenter;
            Snackbar.Configuration.MaxDisplayedSnackbars = 5;
        }
        else
        {
            Snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopLeft;
            Snackbar.Configuration.MaxDisplayedSnackbars = 35;
        }
    }
    
    private readonly MudTheme _theme = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#2196F3",
            PrimaryContrastText = "#FFFFFF"
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#2196F3",
            PrimaryContrastText = "#FFFFFF"
        }
    };
    
    protected override async Task OnInitializedAsync()
    {
        // Check for login from localstorage
        var auth = await DataStore.LoadAuthenticationDetailsAsync();

        if (auth != null && !JWTHelper.IsAccessTokenExpired(auth.AccessToken))
        {
            AppState.SetLoggedInUserFromAuthentication(auth);
            UserSessionService.MonitorUserSession(auth);
            
            if (AppState.CurrentUser != null && AppState.CurrentUser.Role != UserRole.NoRole)
            {
                RobotHubCommunication.AllowStart();
                await RobotHubCommunication.StartAsync();
            }
        }
        else
        {
            await DataStore.ClearAuthenticationDetailsAsync();
            AppState.ClearUser();
            await RobotHubCommunication.StopAsync();
        }
        
        AppState.OnUserChanged += OnUserChanged;
        AppState.OnDarkModeChanged += StateHasChanged;
        AppState.OnApiStatusChanged += OnApiStatusChanged;
        
        UpdateSnackbarPosition();
        
        RedirectIfLoggedIn();
        
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

    private void OnUserChanged()
    {
        UpdateSnackbarPosition();
        InvokeAsync(() =>
        {
            RedirectIfLoggedIn();
            StateHasChanged();
        });
    }
    
    private void RedirectIfLoggedIn()
    {
        var path = NavigationManager.ToBaseRelativePath(NavigationManager.Uri)
            .Trim('/')
            .ToLowerInvariant();

        var isAuthPage = path is "login" or "register";

        if (AppState.IsUserLoggedIn())
        {
            if (isAuthPage)
            {
                NavigationManager.NavigateTo("/", replace: true);
            }
        }
        else
        {
            if (!isAuthPage)
            {
                NavigationManager.NavigateTo("/login", replace: true);
            }
        }
    }

    public void Dispose()
    {
        AppState.OnUserChanged -= OnUserChanged;
        AppState.OnDarkModeChanged -= StateHasChanged;
        AppState.OnApiStatusChanged -= OnApiStatusChanged;
    }
}