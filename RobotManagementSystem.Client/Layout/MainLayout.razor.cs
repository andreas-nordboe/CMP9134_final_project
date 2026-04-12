using Microsoft.AspNetCore.Components;
using RobotManagementSystem.Client.Services;
using RobotManagementSystem.Shared.Models.RBAC;
using RobotManagementSystem.Shared.Models.Users;

namespace RobotManagementSystem.Client.Layout;

public partial class MainLayout
{
    private bool _sidebarOpen = true; // open by default
    
    [Inject]
    IAppState Appstate { get; set; }

    protected override Task OnInitializedAsync()
    {
        Appstate.OnUserChanged += StateHasChanged;
        
        // Create dummy user for testing until database persistence is in place
        User dummyUser = new User
        {
            UserId = Guid.NewGuid().ToString(),
            FirstName = "Andreas",
            LastName = "Nordboe",
            Role = UserRole.Commander,
            IsLoggedIn = true
        };
        Appstate.SetLoggedInUser(dummyUser);
        
        return Task.CompletedTask;
    }

    void ToggleSidebar()
    {
        _sidebarOpen = !_sidebarOpen;
    }

    public void Dispose()
    {
        Appstate.OnUserChanged -= StateHasChanged;
    }
}