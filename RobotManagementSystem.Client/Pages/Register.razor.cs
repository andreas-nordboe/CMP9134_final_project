using Microsoft.AspNetCore.Components;
using RobotManagementSystem.Shared.Models.Users;

namespace RobotManagementSystem.Client.Pages;

public partial class Register : ComponentBase
{
    public RegisterUserDTO UserDetails { get; set; } = new RegisterUserDTO();
    
    [Inject]
    NavigationManager NavigationManager { get; set; }

    private void RegisterUser()
    {
        // TODO: Add API call to backend and redirect user to either login or dashboard with created user depending on API response
        NavigationManager.NavigateTo("/login");
    }

    private void LoginUser()
    {
        
    }
    
}