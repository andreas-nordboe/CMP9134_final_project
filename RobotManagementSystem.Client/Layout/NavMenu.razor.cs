using Microsoft.AspNetCore.Components;
using RobotManagementSystem.Client.Services;
using RobotManagementSystem.Client.Services.Authentication;

namespace RobotManagementSystem.Client.Layout;

public partial class NavMenu
{
    [Inject] 
    public IAppState AppState { get; set; }
    [Inject] 
    public IAuthenticationService AuthenticationService { get; set; }
    [Inject] 
    public NavigationManager NavigationManager { get; set; }

    protected override void OnInitialized()
    {
        AppState.OnUserChanged += StateHasChanged;
    }

    private async void LogoutUser()
    {
        await AuthenticationService.LogoutUserAsync();
        NavigationManager.NavigateTo("/login"); // Navigating to login for now just to test layout
    }
    
}