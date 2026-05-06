namespace RobotManagementSystem.Shared.Models.Errors;

// TODO These error codes could potentially be refactored into separate static classes later
public static class ErrorCodes
{
    // Authentication (1000-1099)
    public const int UserNotFound = 1001;
    public const int UserAlreadyExists = 1002;
    public const int InvalidCredentials = 1003;
    public const int PasswordNotStrongEnough = 1004;
    public const int PasswordsDoNotMatch = 1005;
    public const int InvalidUsername = 1006;
    
    // Internal (1500-1599)
    public const int InternalServerError = 1500;
    public const int InvalidRequest = 1501;
    
    // Robot Command Codes (2000-2099)
}