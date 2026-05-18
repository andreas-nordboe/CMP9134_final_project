using Microsoft.AspNetCore.Components;
using RobotManagementSystem.Client.Services;

namespace RobotManagementSystem.Client.Pages;

public partial class Dashboard : ComponentBase, IDisposable
{
    protected override void OnInitialized()
    {
        AppState.OnUserChanged += HandleUserChanged;
    }

    private void HandleUserChanged()
    {
        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        AppState.OnUserChanged -= HandleUserChanged;
    }
}