using RobotManagementSystem.Shared.Models.Authentication;
using RobotManagementSystem.Shared.Models.Users;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using RobotManagementSystem.Client.Helpers;
using RobotManagementSystem.Client.Services.DataStore;
using RobotManagementSystem.Client.Services.Robot;
using RobotManagementSystem.Client.Services.Sessions;
using RobotManagementSystem.Shared.Utils;

namespace RobotManagementSystem.Client.Services.Authentication;

public class AuthenticationService : IAuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly RobotHubCommunication _robotHubCommunication;
    private readonly IAppState _appState;
    private readonly IDataStoreService _dataStoreService;
    private readonly IUserSessionService _userSessionService;

    public AuthenticationService(IHttpClientFactory httpClientFactory, RobotHubCommunication robotHubCommunication, IAppState appState, IDataStoreService dataStoreService, IUserSessionService userSessionService)
    {
        _robotHubCommunication = robotHubCommunication;
        _appState = appState;
        _dataStoreService = dataStoreService;
        _userSessionService = userSessionService;
        _httpClient = httpClientFactory.CreateClient("API");
    }

    public async Task<AuthenticationResponse?> LoginUserAsync(LoginDetails loginDetails)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("auth/login", new AuthenticationRequest
            {
                Username = loginDetails.Username,
                Password = loginDetails.Password
            });
            
            if (!response.IsSuccessStatusCode)
                return null;
            
            var authResponse = await response.Content.ReadFromJsonAsync<AuthenticationResponse>();
            
            if(authResponse == null || string.IsNullOrWhiteSpace(authResponse.AccessToken))
                return null;

            await _dataStoreService.StoreAuthenticationDetailsAsync(authResponse);
            _appState.SetLoggedInUserFromAuthentication(authResponse);
            _userSessionService.MonitorUserSession(authResponse);
            
            await _robotHubCommunication.StartAsync();
            
            return authResponse;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }

    public async Task<AuthenticationResponse?> RegisterUserAsync(RegisterUserRequest registerUserDetails)
    {
        try
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("auth/register", new RegisterUserRequest()
                {
                    Username = registerUserDetails.Username,
                    FirstName = registerUserDetails.FirstName,
                    LastName = registerUserDetails.LastName,
                    Password = registerUserDetails.Password,
                    ConfirmPassword = registerUserDetails.ConfirmPassword
                });

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var registerUserResponse = await response.Content.ReadFromJsonAsync<AuthenticationResponse>();

                // TODO improve handling and put it into a static helper class that checks token validity
                // the returned JWT access token is signed so if client tampers with the token, it will be rejected on the backend

                if (registerUserResponse == null || string.IsNullOrWhiteSpace(registerUserResponse.AccessToken))
                    return null;
                
                await _dataStoreService.StoreAuthenticationDetailsAsync(registerUserResponse);
                _appState.SetLoggedInUserFromAuthentication(registerUserResponse);
                await _robotHubCommunication.StartAsync();

                return registerUserResponse;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
            
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }

    public async Task LogoutUserAsync()
    {
        await _dataStoreService.ClearAuthenticationDetailsAsync();
        _appState.ClearUser();
        await _robotHubCommunication.DisposeAsync();
    }
}