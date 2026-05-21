using RobotManagementSystem.Shared.Models.Users;

namespace RobotManagementSystem.Client.Services.Admin;

public interface IAdminUserManagementService
{
    Task<List<UserAccountDto>> ListUsersAsync();
    Task<UserAccountDto?> GetUserAsync(string userId);
    Task<bool> DeleteUserAsync(int userId);
    Task<UserAccountDto?> UpdateUserRoleAsync(int userId, UserRole role);
}