using Microsoft.AspNetCore.Components;
using MudBlazor;
using RobotManagementSystem.Client.Modals.Admin;
using RobotManagementSystem.Client.Services.Admin;
using RobotManagementSystem.Shared.Models.Users;

namespace RobotManagementSystem.Client.Pages;

public partial class AdminUserManager : ComponentBase
{
    public List<UserAccountDto?> UserAccounts { get; set; } = new List<UserAccountDto?>();
    public bool IsLoadingUserAccounts { get; set; }
    [Inject] private IAdminUserManagementService AdminUserManagementService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] private IDialogService DialogService { get; set; } = null!;
    
    protected override async Task OnInitializedAsync()
    {
        var users = await AdminUserManagementService.ListUsersAsync();
        if(users.Count > 0)
        {
            UserAccounts = users;
        }
    }

    async Task DeleteUserAccount(UserAccountDto userAccount)
    {
        var parameters = new DialogParameters<ConfirmDeleteUserDialog>
        {
            { x=> x.UserAccount, userAccount }
        };

        var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Medium };

        var dialog = await DialogService.ShowAsync<ConfirmDeleteUserDialog>("Delete User", parameters, options);
        var dialogResult = await dialog.Result;

        if (!dialogResult.Canceled)
        {
            var deleteResponse = await AdminUserManagementService.DeleteUserAsync(userAccount.Id);
            if (deleteResponse)
            {
                Snackbar.Add("User was successfully deleted", Severity.Success);
                UserAccounts.Remove(userAccount);
                StateHasChanged();
            }
            else
            {
                Snackbar.Add("Failed to delete user", Severity.Error);
            }
        }
    }
    
    async Task EditUserRole(UserAccountDto userAccount)
    {
        var parameters = new DialogParameters<EditUserRoleDialog>
        {
            { x=> x.UserAccount, userAccount }
        };

        var options = new DialogOptions() { CloseButton = true, FullWidth = true, MaxWidth = MaxWidth.Small };

        var dialog = await DialogService.ShowAsync<EditUserRoleDialog>("Update User Role", parameters, options);
        var dialogResult = await dialog.Result;

        if (!dialogResult.Canceled && dialogResult.Data is UserRole newRole)
        {
            var updateUserRoleResponse = await AdminUserManagementService.UpdateUserRoleAsync(userAccount.Id, newRole);
            if (updateUserRoleResponse != null)
            {
                Snackbar.Add("User role was successfully updated", Severity.Success);
                userAccount.Role = updateUserRoleResponse.Role; // updates the UI
                StateHasChanged();
            }
            else
            {
                Snackbar.Add("Failed to update user role", Severity.Error);
            }
        }
    }
    
}