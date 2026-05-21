using Microsoft.AspNetCore.Components;
using MudBlazor;
using RobotManagementSystem.Client.Services.SystemStatus;
using RobotManagementSystem.Shared.Models.SystemStatus;

namespace RobotManagementSystem.Client.Pages;

public partial class SystemStatus : ComponentBase
{
    public bool IsLoadingSystemLogs { get; set; }
    [Inject] public ISystemStatusLogService SystemStatusLogService { get; set; } = default!;
    [Inject] public ISnackbar Snackbar { get; set; } = default!;
    private MudTable<SystemStatusLog>? _table;
    public SystemStatusEventType? SelectedEventType { get; set; }
    public DateTime? FromDate { get; set; }
    
    private async Task<TableData<SystemStatusLog>> LoadServerData(TableState state, CancellationToken cancellationToken)
    {
        IsLoadingSystemLogs = true;

        try
        {
            var page = state.Page + 1;
            var pageSize = state.PageSize;

            var result = await SystemStatusLogService.GetSystemStatusLogsAsync(
                SelectedEventType,
                FromDate,
                page,
                pageSize);

            return new TableData<SystemStatusLog>
            {
                Items = result.Items,
                TotalItems = result.TotalCount
            };
        }
        catch
        {
            Snackbar.Add("Failed to retrieve system logs", Severity.Error);

            return new TableData<SystemStatusLog>
            {
                Items = [],
                TotalItems = 0
            };
        }
        finally
        {
            IsLoadingSystemLogs = false;
        }
    }
    
    
    private async Task OnEventTypeChanged(SystemStatusEventType? eventType)
    {
        SelectedEventType = eventType;

        if (_table != null)
        {
            _table.CurrentPage = 0;
            await _table.ReloadServerData();
        }
            
    }

    private async Task OnFromDateChanged(DateTime? date)
    {
        FromDate = date;

        if (_table != null)
        {
            _table.CurrentPage = 0;
            await _table.ReloadServerData();
        }
    }
    
    private static string FormatEventType(SystemStatusEventType eventType)
    {
        return eventType switch
        {
            SystemStatusEventType.ROBOT_STATUS_CHANGED => "Robot status changed",
            SystemStatusEventType.CONNECTION_CHANGED => "Connection changed",
            SystemStatusEventType.TELEMETRY_SNAPSHOT => "Telemetry snapshot",
            _ => "Unknown"
        };
    }
    
    private async Task RefreshLogsAsync()
    {
        if (_table != null) 
        {
            _table.CurrentPage = 0;
            await _table.ReloadServerData();
        }
    }
}