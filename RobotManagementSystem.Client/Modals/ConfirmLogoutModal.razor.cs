using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace RobotManagementSystem.Client.Modals;

public partial class ConfirmLogoutModal
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = default!;

    private void Confirm()
    {
        MudDialog.Close(DialogResult.Ok(true));
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }
}