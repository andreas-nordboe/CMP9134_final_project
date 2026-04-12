using Microsoft.AspNetCore.Components;
using RobotManagementSystem.Client.Services;

namespace RobotManagementSystem.Client.Layout;

public partial class NavMenu
{
    [Inject] 
    public IAppState AppState { get; set; }
    [Inject] 
    public NavigationManager NavigationManager { get; set; }

    protected override void OnInitialized()
    {
        AppState.OnUserChanged += StateHasChanged;
    }

    private void LogoutUser()
    {
        NavigationManager.NavigateTo("/");
    }
    
}