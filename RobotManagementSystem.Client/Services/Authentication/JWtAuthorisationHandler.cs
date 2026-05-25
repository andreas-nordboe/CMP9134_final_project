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
        var auth = await _dataStoreService.GetAuthenticationDetailsAsync();

        var token = auth?.AccessToken;

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        if (auth != null)
        {
            _appState.SetLoggedInUserFromAuthentication(auth);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}