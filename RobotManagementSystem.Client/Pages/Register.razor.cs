using Microsoft.AspNetCore.Components;
using MudBlazor;
using RobotManagementSystem.Client.Helpers;
using RobotManagementSystem.Client.Services.Authentication;
using RobotManagementSystem.Shared.Models.Authentication;
using RobotManagementSystem.Shared.Models.Users;

namespace RobotManagementSystem.Client.Pages;

public partial class Register : ComponentBase
{
    public RegisterUserDTO UserDetails { get; set; } = new RegisterUserDTO();
    
    [Inject]
    NavigationManager NavigationManager { get; set; }
    [Inject]
    IAuthenticationService AuthenticationService { get; set; }

    private async Task RegisterUser()
    {
        var registerUserResponse = await AuthenticationService.RegisterUserAsync(new RegisterUserRequest
        {
            FirstName = UserDetails.FirstName,
            LastName = UserDetails.LastName,
            Username = UserDetails.Username,
            Password = UserDetails.Password,
            ConfirmPassword = UserDetails.RepeatPassword
        });

        if (registerUserResponse != null)
        {
            NavigationManager.NavigateTo("/");
            return;
        }
        else
        {
            SnackBar.Add("Failed to register user. Please try again.", Severity.Error);
        }
        
    }

    private void LoginUser()
    {
        
    }
    
}