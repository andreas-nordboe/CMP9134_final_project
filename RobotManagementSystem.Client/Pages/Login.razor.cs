using Microsoft.AspNetCore.Components;
using RobotManagementSystem.Shared.Services;

namespace RobotManagementSystem.Client.Pages;

public partial class Login : ComponentBase
{
    [Inject]
    GridService _gridService { get; set; }

    public Login(GridService gridService)
    {
        _gridService = gridService;
    }
    
}