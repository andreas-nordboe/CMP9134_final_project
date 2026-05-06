using RobotManagementSystem.Shared.Models.Authentication;
using RobotManagementSystem.Shared.Models.Users;
using System.Net.Http.Json;

namespace RobotManagementSystem.Client.Services.Authentication;

public class AuthenticationService : IAuthenticationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly HttpClient _httpClient;

    public AuthenticationService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("API");
    }

    public async Task<AuthenticationResponse> LoginUserAsync(LoginDetails loginDetails)
    {
        var response = await _httpClient.PostAsJsonAsync("auth/login", new AuthenticationRequest
        {
            Username = loginDetails.Username,
            Password = loginDetails.Password
        });

        if(!response.IsSuccessStatusCode){
            return null;
        }

        return  await response.Content.ReadFromJsonAsync<AuthenticationResponse>() 
                ?? throw new Exception("Failed to deserialise response");
    }
}