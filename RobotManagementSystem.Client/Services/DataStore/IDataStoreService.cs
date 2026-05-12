using RobotManagementSystem.Shared.Models.Authentication;

namespace RobotManagementSystem.Client.Services.DataStore;

public interface IDataStoreService
{
    Task StoreAuthenticationDetailsAsync(AuthenticationResponse authenticationDetails);
    Task<AuthenticationResponse?> LoadAuthenticationDetailsAsync();
    Task ClearAuthenticationDetailsAsync();
}