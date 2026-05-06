using Microsoft.AspNetCore.Components;
using RobotManagementSystem.Shared.Models.MissionLog;
using RobotManagementSystem.Shared.Models.Robot;
using RobotManagementSystem.Shared.Models.Users;

namespace RobotManagementSystem.Client.Pages;

public partial class MissionLogs : ComponentBase
{
    public IList<MissionLog> Logs { get; set; } = new List<MissionLog>();
    public bool IsLoadingLogs { get; set; }

    protected override void OnInitialized()
    {
        // Feed in data to showcase UI functionality
        MissionLog dummyMissionLog = new MissionLog
        {
            User = new UserAccount
            {
                FirstName = "Andreas",
                LastName = "Nordboe",
                Username = "Andreasnordboe",
                Id = 1
            },
            Timestamp = DateTime.Now,
            Command = RobotCommand.MoveDown,
            Role = UserRole.Commander
        };
        
        Logs.Add(dummyMissionLog);

    }
}