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
    
    // Internal (1500-1599)
    public const string InternalServerError = "Internal server error occurred.";
    public const string InvalidRequest = "Invalid request.";
    
    // Robot Command Codes (2000-2099)
}