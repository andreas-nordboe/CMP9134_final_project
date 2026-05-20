using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using RobotManagementSystem.Client.Modals;
using RobotManagementSystem.Client.Services;
using RobotManagementSystem.Client.Services.Authentication;

namespace RobotManagementSystem.Client.Layout;

public partial class NavMenu
{
    [Inject] 
    public IAppState AppState { get; set; } = default!;
    [Inject] 
    public IAuthenticationService AuthenticationService { get; set; } = default!;
    [Inject] 
    public NavigationManager NavigationManager { get; set; } = default!;

    [Inject] public IDialogService DialogService { get; set; } = default!;

    protected override void OnInitialized()
    {
        AppState.OnUserChanged += StateHasChanged;
    }

    private async Task LogoutUser()
    {
        await AuthenticationService.LogoutUserAsync();
        NavigationManager.NavigateTo("/login", forceLoad: true);
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
    
    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
            await ConfirmLogout();
    }
}