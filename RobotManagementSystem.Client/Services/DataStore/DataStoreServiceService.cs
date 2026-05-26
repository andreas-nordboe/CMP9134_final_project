using Blazored.LocalStorage;
using RobotManagementSystem.Shared.Models.Authentication;

namespace RobotManagementSystem.Client.Services.DataStore;

public class DataStoreServiceService : IDataStoreService
{
    private readonly ILocalStorageService _localStorage;
    private const string AuthenticationDetailsKey = "AuthenticationDetails";
    private const string ShowMapCoordinatesKey = "ShowMapCoordinates";
    private const string EnableSoundEffectsKey = "EnableSoundEffects";
    private const string ShowGroundTruthMapKey = "ShowGroundTruthMap";

    public DataStoreServiceService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task StoreAuthenticationDetailsAsync(AuthenticationResponse authenticationDetails)
    {
        await _localStorage.SetItemAsync(AuthenticationDetailsKey, authenticationDetails); 
    }

    public async Task<AuthenticationResponse?> LoadAuthenticationDetailsAsync()
    {
        return await _localStorage.GetItemAsync<AuthenticationResponse>(AuthenticationDetailsKey);
    }

    public async Task ClearAuthenticationDetailsAsync()
    {
        await _localStorage.RemoveItemAsync(AuthenticationDetailsKey);
    }
    
    public async Task StoreShowMapCoordinatesAsync(bool showCoordinates)
    {
        await _localStorage.SetItemAsync(ShowMapCoordinatesKey, showCoordinates);
    }

    public async Task StoreEnableSoundEffectsAsync(bool enableSoundEffects)
    {
        await _localStorage.SetItemAsync(EnableSoundEffectsKey, enableSoundEffects);
    }

    public async Task<bool?> LoadShowMapCoordinatesAsync()
    {
        return await _localStorage.GetItemAsync<bool?>(ShowMapCoordinatesKey);
    }

    public async Task<bool?> LoadEnableSoundEffectsAsync()
    {
        return await _localStorage.GetItemAsync<bool?>(EnableSoundEffectsKey);
    }

    public async Task ClearShowMapCoordinatesAsync()
    {
        await _localStorage.RemoveItemAsync(ShowMapCoordinatesKey);
    }

    public async Task<AuthenticationResponse> GetAuthenticationDetailsAsync()
    {
        return await _localStorage.GetItemAsync<AuthenticationResponse>(AuthenticationDetailsKey);
    }

    public async Task StoreShowGroundTruthMap(bool showGroundTruthMap)
    {
        await _localStorage.SetItemAsync(ShowGroundTruthMapKey, showGroundTruthMap);
    }

    public async Task<bool?> LoadShowGroundTruthMapAsync()
    {
        return await _localStorage.GetItemAsync<bool?>(ShowGroundTruthMapKey);
    }
}