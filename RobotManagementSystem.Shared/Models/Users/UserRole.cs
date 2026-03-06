namespace RobotManagementSystem.Shared.Models.Users;

// It is essential that role defaults to none for now to ensure least privileges
public enum UserRole
{
    None,
    Viewer,
    Commander,
    Auditor,
    Admin
}