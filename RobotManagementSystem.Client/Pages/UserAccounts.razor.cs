using Microsoft.AspNetCore.Components;

namespace RobotManagementSystem.Client.Pages;

public partial class UserAccounts : ComponentBase, IAsyncDisposable
{
    private protected DateTime TokenExpiryTime;
    private Timer? _timer;
    
    protected override async Task OnInitializedAsync()
    {
        var accessToken = await _dataStore.LoadAuthenticationDetailsAsync();
        if (accessToken != null) TokenExpiryTime = accessToken.Expires;

        _timer = new Timer(_ =>
        {
            InvokeAsync(StateHasChanged);
        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }

    private string GetTokenExpiryCountdownTime()
    {
        var timeRemaining = TokenExpiryTime - DateTime.UtcNow;
        if(timeRemaining < TimeSpan.Zero) 
            return "Session Expired";

        return $"{timeRemaining.Hours:D2}:{timeRemaining.Minutes:D2}:{timeRemaining.Seconds:D2}";
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_timer != null) await _timer.DisposeAsync();
    }
}