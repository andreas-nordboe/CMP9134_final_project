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

    
    // Robot Api and Commands Codes (2000-2099)
    public const string RobotApiNotAvailable = "Robot API not available.";
    public const string RobotMapDoesNotExist = "Could not validate move because map data is unavailable.";
    public const string RobotCoordinatesNotValid = "Robot coordinates not valid and must be between 0 and 20.";
    public const string RobotCommandFailed = "Robot move command failed.";
    public const string RobotMoveCommandFailed = "Failed to move robot: Invalid input";
    public const string RobotMoveCommandSuccess = "Robot was successfully moved to "; // The coordinates are appended to this string
    public const string RobotResetCommandSuccess = "Robot simulation was successfully reset.";
    public const string RobotResetCommandFailed = "Failed to reset robot simulation.";
    public const string RobotUnavaiableAfterRetrying = "API unavailable after retries.";
    public const string RobotApiUnavailableRetrying = "Robot API returned 503 Service Unavailable. Retrying...";
    public const string RobotApiTimeoutRetrying = "Robot API timeout. Retrying...";
    public const string RobotApiConnectionErrorRetrying = "Robot API connection failed. Retrying...";
    public const string RobotAttemptedMove = "Attempted to move robot to invalid coordinates "; // The coordinates are appended to this string
    public const string RobotApiReturnedStatusCode = "Robot API returned status code "; // Status code is appended to this string
    public const string RobotMoveException = "Robot move command exception: "; // Exception is appended to this string
    public const string RobotResetException = "Robot reset command exception: "; // Exception is appended to this string
    public const string TriedToMoveToObstacle = "Tried to move robot into an obstacle at "; // The coordinates are appended to this string
    public const string RobotIsBlocked = "Move blocked because the target position contains an obstacle.";
    public const string RobotApiUnavailable = "Robot API returned 503 Service Unavailable.";
    public const string RobotApiTimeout = "Robot API timeout. Retrying...";
    public const string RobotApiConnectionError = "Robot API connection failed.";
    
    
    // Map
    public const string FailedToRetrieveMapData = "Failed to retrieve map information.";
}