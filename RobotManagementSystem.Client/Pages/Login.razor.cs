using Microsoft.AspNetCore.Components;
using RobotManagementSystem.Client.Helpers;
using RobotManagementSystem.Client.Services.Authentication;
using RobotManagementSystem.Shared.Models.Users;
using RobotManagementSystem.Shared.Services;

namespace RobotManagementSystem.Client.Pages;

public partial class Login : ComponentBase
{
    protected bool StayLoggedIn { get; set; }
    public LoginDetails LoginDetails { get; set; } = new LoginDetails();
    [Inject] 
    private NavigationManager NavigationManager { get; set; }
    [Inject]
    IAuthenticationService AuthenticationService { get; set; }

    private async void LoginUser()
    {
        try
        {
            var authResponse = await AuthenticationService.LoginUserAsync(LoginDetails);
            AppState.SetLoggedInUser(UserHelper.ToUser(authResponse));
        
            NavigationManager.NavigateTo("/");
            StateHasChanged();
            
        }
        catch (Exception e)
        {
            Snackbar.Add(e.Message);
        }
    }
}