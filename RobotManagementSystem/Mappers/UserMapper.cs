using RobotManagementSystem.Shared.Models.Users;

namespace RobotManagementSystem.Mappers;

public static class UserMapper
{
    public static UserAccountDto ToDto(UserAccount user)
    {
        return new UserAccountDto
        {
            Id = user.Id,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role
        };
    }
}