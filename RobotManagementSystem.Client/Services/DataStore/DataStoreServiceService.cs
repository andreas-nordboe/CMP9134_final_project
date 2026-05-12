using Blazored.LocalStorage;
using RobotManagementSystem.Shared.Models.Authentication;

namespace RobotManagementSystem.Client.Services.DataStore;

public class DataStoreServiceService : IDataStoreService
{
    private readonly ILocalStorageService _localStorage;
    private const string AuthenticationDetailsKey = "AuthenticationDetails";

    public DataStoreServiceService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task StoreAuthenticationDetailsAsync(AuthenticationResponse authenticationDetails)
    {
        await _localStorage.SetItemAsync(AuthenticationDetailsKey, authenticationDetails); // TODO Refactor hard-coded key (code smell)
    }

    public async Task<AuthenticationResponse?> LoadAuthenticationDetailsAsync()
    {
        return await _localStorage.GetItemAsync<AuthenticationResponse>(AuthenticationDetailsKey); // TODO Refactor hard-coded key (code smell)
    }

    public async Task ClearAuthenticationDetailsAsync()
    {
        await _localStorage.RemoveItemAsync(AuthenticationDetailsKey);
    }
}