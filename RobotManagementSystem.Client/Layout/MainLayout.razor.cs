using Microsoft.AspNetCore.Components;
using RobotManagementSystem.Client.Services;
using RobotManagementSystem.Shared.Models.RBAC;
using RobotManagementSystem.Shared.Models.Users;

namespace RobotManagementSystem.Client.Layout;

public partial class MainLayout
{
    private bool _sidebarOpen = true; // open by default
    
    [Inject] private IAppState Appstate { get; set; }
    
    [Inject] private NavigationManager NavigationManager { get; set; }
    
    protected override Task OnInitializedAsync()
    {
        Appstate.OnUserChanged += StateHasChanged;
        Appstate.OnDarkModeChanged += StateHasChanged;

        if (Appstate.CurrentUser is null || Appstate.CurrentUser.IsLoggedIn)
        {
            NavigationManager.NavigateTo("/login");
        }
        
        
        var isDarkMode = localStorage.GetItemAsync<bool>("IsDarkMode");
        if (isDarkMode.IsCompleted)
        {
            Appstate.IsDarkMode = isDarkMode.Result;
            //Appstate.OnDarkModeChanged?.Invoke();
        }
        
        return Task.CompletedTask;
    }

    async void ToggleDarkMode()
    {
        Appstate.IsDarkMode = !Appstate.IsDarkMode;
        await localStorage.SetItemAsync("IsDarkMode", Appstate.IsDarkMode);
    }

    void ToggleSidebar()
    {
        _sidebarOpen = !_sidebarOpen;
    }

    public void Dispose()
    {
        Appstate.OnUserChanged -= StateHasChanged;
        Appstate.OnDarkModeChanged -= StateHasChanged;
    }
}