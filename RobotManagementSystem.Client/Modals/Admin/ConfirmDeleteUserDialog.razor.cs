using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using RobotManagementSystem.Shared.Models.Users;

namespace RobotManagementSystem.Client.Modals.Admin;

public partial class ConfirmDeleteUserDialog : ComponentBase
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; }
    
    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        await JSRuntime.InvokeVoidAsync(
            "robotDialogFocus.focusElementById",
            "delete-user-cancel-button");
    }

    [Parameter] public UserAccountDto UserAccount { get; set; } = new UserAccountDto();

    private void DeleteUser() => MudDialog.Close(DialogResult.Ok(true));

    private void Cancel() => MudDialog.Cancel();
}