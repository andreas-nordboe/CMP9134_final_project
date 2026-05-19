using Microsoft.AspNetCore.Components;
using MudBlazor;
using RobotManagementSystem.Client.Modals;
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
    [Inject] private IDialogService DialogService { get; set; } = default!;

    protected override void OnInitialized()
    {
        AppState.OnUserChanged += StateHasChanged;
    }

    private async Task LogoutUser()
    {
        await AuthenticationService.LogoutUserAsync();
        NavigationManager.NavigateTo("/login"); // Navigating to login for now just to test layout
    }
    
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
    
}