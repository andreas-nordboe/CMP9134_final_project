using Microsoft.AspNetCore.Components;
using MudBlazor;
using RobotManagementSystem.Client.Services.SystemStatus;
using RobotManagementSystem.Shared.Models.SystemStatus;

namespace RobotManagementSystem.Client.Components;

public partial class SystemStatusSummaryComponent : ComponentBase, IDisposable
{
    private SystemStatusSummary? Summary { get; set; }

    private string ConnectionStatusText => Summary?.ConnectionStatus ?? "Unknown";

    private string SignalStateText => Summary?.SignalState ?? "Unknown";
    
    private PeriodicTimer? _summaryTimer;
    private CancellationTokenSource? _summaryCts;
    
    [Inject] public ISystemStatusLogService SystemStatusLogService { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        await LoadSummaryAsync();
        
        _summaryCts = new CancellationTokenSource();
        _summaryTimer = new PeriodicTimer(TimeSpan.FromSeconds(1));

        _ = PollSummaryAsync(_summaryCts.Token);
    }
    
    private string AverageLatencyText => Summary?.AverageLatencyMs.HasValue == true
        ? $"{Summary.AverageLatencyMs.Value:0} ms"
        : "N/A";

    private int RetryAttempts => Summary?.RetryAttempts ?? 0;

    private string LastUpdatedText => Summary?.LastUpdated.HasValue == true
        ? Summary.LastUpdated.Value.ToLocalTime().ToString("dd.MM.yyyy HH:mm:ss")
        : "N/A";

    private Color GetConnectionColor()
    {
        return ConnectionStatusText.Equals("Connected", StringComparison.OrdinalIgnoreCase)
            ? Color.Success
            : Color.Error;
    }

    private async Task LoadSummaryAsync()
    {
        Summary = await SystemStatusLogService.GetSystemStatusSummaryAsync();
    }
    
    private async Task PollSummaryAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (await _summaryTimer!.WaitForNextTickAsync(cancellationToken))
            {
                await InvokeAsync(async () =>
                {
                    await InvokeAsync(LoadSummaryAsync);
                });
            }
        }
        catch (OperationCanceledException)
        {
        }
    }
    
    private List<ChartSeries<double>> LatencySeries =>
    [
        new ChartSeries<double>
        {
            Name = "Latency",
            Data = Summary?.RecentLatenciesMs?
                .Select(x => Math.Round(x, 0))
                .ToArray() ?? []
        }
    ];
    
    private string[] LatencyLabels =>
        Summary?.RecentLatenciesMs?
            .Select((_, index) => (index + 1).ToString())
            .ToArray()
        ?? [];
    
    public void Dispose()
    {
        _summaryCts?.Cancel();
        _summaryTimer?.Dispose();
        _summaryCts?.Dispose();
    }
}