using System.Net.Http.Headers;
using RobotManagementSystem.Client.Services.DataStore;

namespace RobotManagementSystem.Client.Services.Authentication;

public class JWtAuthorisationHandler : DelegatingHandler
{
    private readonly IDataStoreService _dataStoreService;
    private readonly IAppState _appState;

    public JWtAuthorisationHandler(IDataStoreService dataStoreService, IAppState appState)
    {
        _dataStoreService = dataStoreService;
        _appState = appState;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Try to get from memory first
        var token = _appState.AuthenticationDetails?.AccessToken;

        // If not in cache, try to get from local storage
        if (string.IsNullOrWhiteSpace(token))
        {
            var auth = await _dataStoreService.LoadAuthenticationDetailsAsync();

            if (auth != null)
            {
                _appState.SetLoggedInUserFromAuthentication(auth);
                token = auth.AccessToken;
            }
        }

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        
        return await base.SendAsync(request, cancellationToken);
    }
}