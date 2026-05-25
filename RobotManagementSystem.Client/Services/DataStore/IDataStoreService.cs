using RobotManagementSystem.Shared.Models.Authentication;

namespace RobotManagementSystem.Client.Services.DataStore;

public interface IDataStoreService
{
    Task StoreAuthenticationDetailsAsync(AuthenticationResponse authenticationDetails);
    Task<AuthenticationResponse?> LoadAuthenticationDetailsAsync();
    Task ClearAuthenticationDetailsAsync();
    Task StoreShowMapCoordinatesAsync(bool showCoordinates);
    Task StoreEnableSoundEffectsAsync(bool enableSoundEffects);
    Task<bool?> LoadShowMapCoordinatesAsync();
    Task<bool?> LoadEnableSoundEffectsAsync();
    Task ClearShowMapCoordinatesAsync();
    Task<AuthenticationResponse> GetAuthenticationDetailsAsync();
}