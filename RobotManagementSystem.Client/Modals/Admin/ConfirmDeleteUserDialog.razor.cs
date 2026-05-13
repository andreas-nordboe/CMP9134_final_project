using Microsoft.AspNetCore.Components;
using MudBlazor;
using RobotManagementSystem.Shared.Models.Users;

namespace RobotManagementSystem.Client.Modals.Admin;

public partial class ConfirmDeleteUserDialog : ComponentBase
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; }

    [Parameter] public UserAccountDto UserAccount { get; set; } = new UserAccountDto();

    private void DeleteUser() => MudDialog.Close(DialogResult.Ok(true));

    private void Cancel() => MudDialog.Cancel();
}