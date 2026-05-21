using Microsoft.AspNetCore.Components;
using MudBlazor;
using RobotManagementSystem.Client.Services.MissionLogs;
using RobotManagementSystem.Shared.Models.MissionLog;
using RobotManagementSystem.Shared.Models.Robot;
using RobotManagementSystem.Shared.Models.Users;

namespace RobotManagementSystem.Client.Pages;

public partial class MissionLogs : ComponentBase
{
    public IList<MissionLogDto> Logs { get; set; } = new List<MissionLogDto>();
    public bool IsLoadingLogs { get; set; }
    [Inject] public IMissionLogService MissionLogService { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        // Feed in data to showcase UI functionality
        var missionLogs = await MissionLogService.GetAllMissionLogs();
        if (missionLogs.Count > 0)
        {
            Logs = missionLogs;
        }
        else
        {
            // TODO Refactor to static error message class and display error message on the UI
            SnackBar.Add("Failed to load mission logs.", Severity.Error);
        }
    }

    private string DetailsText(string? logDetails)
    {
        return string.IsNullOrWhiteSpace(logDetails) ? "N/A" : logDetails;
    }
}