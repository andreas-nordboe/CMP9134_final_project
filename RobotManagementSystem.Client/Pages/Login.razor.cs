using Microsoft.AspNetCore.Components;
using RobotManagementSystem.Shared.Models.Users;
using RobotManagementSystem.Shared.Services;

namespace RobotManagementSystem.Client.Pages;

public partial class Login : ComponentBase
{
    protected bool StayLoggedIn { get; set; }
    public LoginDetails LoginDetails { get; set; } = new LoginDetails();
}