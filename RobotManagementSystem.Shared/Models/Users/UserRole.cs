namespace RobotManagementSystem.Shared.Models.Users;

// It is essential that role defaults to none for now to ensure least privileges
// It is crucial to not change the order of these as they are used in the SQLite database
public enum UserRole
{
    NoRole,
    Viewer,
    Commander,
    Auditor,
    Admin
}