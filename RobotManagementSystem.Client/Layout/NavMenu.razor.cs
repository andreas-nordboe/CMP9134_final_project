using Microsoft.AspNetCore.Components;
<<<<<<< Updated upstream
=======
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using RobotManagementSystem.Client.Modals;
>>>>>>> Stashed changes
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
    
<<<<<<< Updated upstream
=======
    protected async Task ConfirmLogout()
    {
        
        var options = new DialogOptions
        {
            CloseButton = true,
            MaxWidth = MaxWidth.ExtraSmall,
            FullWidth = true
        };

        var dialog = await DialogService.ShowAsync<ConfirmLogoutModal>(
            "Log Out",
            options);

        var result = await dialog.Result;

        if (!result.Canceled && result.Data is bool confirmed && confirmed)
        {
            await LogoutUser();
        }
    }
    
    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
            await ConfirmLogout();
    }
    
>>>>>>> Stashed changes
}