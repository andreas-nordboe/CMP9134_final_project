using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using RobotManagementSystem.Client.Services.DataStore;

namespace RobotManagementSystem.Client.AuthState;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly IDataStoreService _store;
    private readonly HttpClient _http;

    public CustomAuthStateProvider(IDataStoreService store, IHttpClientFactory factory)
    {
        _store = store;
        _http = factory.CreateClient("API");
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var auth = await _store.GetAuthenticationDetailsAsync();

        if (auth?.AccessToken == null)
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, auth.Username),
            new Claim(ClaimTypes.Role, auth.Role)
        }, "jwt");

        return new AuthenticationState(new ClaimsPrincipal(identity));
    }
}