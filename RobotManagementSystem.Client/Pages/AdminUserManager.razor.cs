using Microsoft.AspNetCore.Components;
using RobotManagementSystem.Shared.Models.Users;

namespace RobotManagementSystem.Client.Pages;

public partial class AdminUserManager : ComponentBase
{
    public IList<UserAccount> UserAccounts { get; set; } = new List<UserAccount>();
    public bool IsLoadingUserAccounts { get; set; }

    protected override void OnInitialized()
    {
        // Feed in data to showcase UI functionality
        UserAccount dummyUserAccount = new UserAccount
        {
            Id = 1,
            Username = "TestUser",
            FirstName = "Test",
            LastName = "Test User",
            Role = UserRole.Admin
        };
        
        UserAccounts.Add(dummyUserAccount);
    }

    void DeleteUserAccount(UserAccount userAccount)
    {
        // TODO Removes the item from the list to refresh UI for now and backend logic will be added later
        UserAccounts.Remove(userAccount);
        StateHasChanged();
    }
    
}