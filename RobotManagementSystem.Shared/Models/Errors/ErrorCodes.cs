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
    public const int UsersNotFound = 1007;
    public const int PasswordIsEmpty = 1008;
    public const int UsernameLengthInvalid = 1009;
    public const int PasswordLengthInvalid = 1010;
    public const int UsernameAlreadyExists = 1011;
    public const int InvalidToken = 1012;
    public const int TokenExpired = 1013;
    
    // Internal (1500-1599)
    public const int InternalServerError = 1500;
    public const int InvalidRequest = 1501;
    public const int EmptyRequest = 1502;
    
    // Robot Command Codes (2000-2099)
}