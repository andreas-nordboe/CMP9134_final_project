using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using RobotManagementSystem.Shared.Models.Users;

namespace RobotManagementSystem.Client.Modals.Admin;

public partial class EditUserRoleDialog : ComponentBase
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; }
    
    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    private UserRole _selectedRole; 

    [Parameter] public UserAccountDto UserAccount { get; set; } = new UserAccountDto();

    protected override void OnInitialized()
    {
        _selectedRole = UserAccount.Role;
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        await JSRuntime.InvokeVoidAsync(
            "robotDialogFocus.focusElementById",
            "edit-user-dropdown");
    }


    private void ChangeUserRole() => MudDialog.Close(DialogResult.Ok(_selectedRole));

    private void Cancel() => MudDialog.Cancel();
}