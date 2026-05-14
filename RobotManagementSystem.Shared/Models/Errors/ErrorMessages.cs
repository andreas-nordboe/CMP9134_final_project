namespace RobotManagementSystem.Shared.Models.Errors;

// This separate class is used to centralise error messages while still supporting specific error messages for APIErrors
public static class ErrorMessages
{
    // Authentication (1000-1099)
    public const string UserNotFound = "User does not exist.";
    public const string UserAlreadyExists = "User already exists.";
    public const string InvalidCredentials = "Invalid credentials.";
    public const string PasswordNotStrongEnough = "Password is not strong enough.";
    public const string PasswordsDoNotMatch = "Passwords do not match.";
    public const string InvalidUsername = "Username is invalid.";
    public const string UsersNotFound = "Users not found.";
    public const string PasswordIsEmpty = "Password cannot be empty.";
    public const string UsernameLengthInvalid = "Username invalid, it must be at least 5 characters long.";
    public const string PasswordLengthInvalid = "Password must be at least 6 characters long.";
    public const string UsernameAlreadyExists = "Username already used.";
    public const string InvalidToken = "Invalid token.";
    public const string TokenExpired = "Token is expired.";
    
    // Mission Logs (2000-2099)
    public const string FailedToCreateMissionLog = "Failed to add mission log.";
    public const string FailedToListMissionLogs = "Failed to list mission logs.";

    
    // Internal (1500-1599)
    public const string InternalServerError = "Internal server error occurred.";
    public const string InvalidRequest = "Invalid request.";
    public const string EmptyRequest = "Request cannot be empty.";

    
    // Robot Command Codes (2000-2099)
    public const string RobotApiNotAvailable = "Robot API not available.";
}