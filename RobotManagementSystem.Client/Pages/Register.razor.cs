using Microsoft.AspNetCore.Components;
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

    private async void RegisterUser()
    {
        // TODO Refactor to use RegisterUserRequest on the frontend instead and remove this DTO class
        var registerUserResponse = await AuthenticationService.RegisterUserAsync(new RegisterUserRequest
        {
            FirstName = UserDetails.FirstName,
            LastName = UserDetails.LastName,
            Username = UserDetails.Username,
            Password = UserDetails.Password,
            ConfirmPassword = UserDetails.RepeatPassword
        });
        
        // TODO improve handling and put it into a static helper class that checks token validity
        // the returned JWT access token is signed so if client tampers with the token, it will be rejected on the backend
        if (!string.IsNullOrWhiteSpace(registerUserResponse.AccessToken))
        {
            appState.SetLoggedInUser(UserHelper.ToUser(registerUserResponse));
            NavigationManager.NavigateTo("/");
        }
    }

    private void LoginUser()
    {
        
    }
    
}