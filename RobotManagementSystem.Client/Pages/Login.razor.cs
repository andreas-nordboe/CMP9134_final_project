using Microsoft.AspNetCore.Components;
using RobotManagementSystem.Shared.Models.RBAC;
using RobotManagementSystem.Shared.Models.Users;
using RobotManagementSystem.Shared.Services;

namespace RobotManagementSystem.Client.Pages;

public partial class Login : ComponentBase
{
    protected bool StayLoggedIn { get; set; }
    public LoginDetails LoginDetails { get; set; } = new LoginDetails();
    [Inject] 
    private NavigationManager NavigationManager { get; set; }

    private void LoginUser()
    {
        // This is just for testing the frontend state and navigation
        User dummyUser = new User
        {
            FirstName = "Andreas",
            LastName = "Robotics",
            IsLoggedIn = true,
            Role = UserRole.Viewer,
            UserId = Guid.NewGuid().ToString()
        };

        appState.SetLoggedInUser(dummyUser);
        
        NavigationManager.NavigateTo("/");
        StateHasChanged();
    }
}