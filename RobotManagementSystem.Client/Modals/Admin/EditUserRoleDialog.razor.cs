using Microsoft.AspNetCore.Components;
using MudBlazor;
using RobotManagementSystem.Shared.Models.Users;

namespace RobotManagementSystem.Client.Modals.Admin;

public partial class EditUserRoleDialog : ComponentBase
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; }

    private UserRole _selectedRole; 

    [Parameter] public UserAccountDto UserAccount { get; set; } = new UserAccountDto();

    protected override void OnInitialized()
    {
        _selectedRole = UserAccount.Role;
    }

    private void ChangeUserRole() => MudDialog.Close(DialogResult.Ok(_selectedRole));

    private void Cancel() => MudDialog.Cancel();
}