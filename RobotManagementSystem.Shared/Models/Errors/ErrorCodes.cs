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
    
    // Mission Logs (2000-2099)
    public const int FailedToCreateMissionLog = 2000;
    public const int FailedToListMissionLogs = 2001;
    
    
    // Internal (1500-1599)
    public const int InternalServerError = 1500;
    public const int InvalidRequest = 1501;
    public const int EmptyRequest = 1502;
    
    
    // Robot Api and Commands Codes (2000-2099)
    public const int RobotApiNotAvailable = 2000;
    public const int RobotMapDoesNotExist = 2001;
    public const int RobotCoordinatesNotValid = 2002;
    public const int RobotCommandFailed = 2003;
    public const int RobotMoveCommandFailed = 2004;
    public const int RobotMoveCommandSuccess = 2005;
    public const int RobotResetCommandSuccess = 2006;
    public const int RobotResetCommandFailed = 2007;
    public const int RobotUnavaiableAfterRetrying = 2008;
    public const int RobotApiUnavailableRetrying = 2009;
    public const int RobotApiTimeoutRetrying = 2010;
    public const int RobotApiConnectionErrorRetrying = 2011;
    public const int RobotAttemptedMove = 2012;
    public const int RobotApiReturnedStatusCode = 2013; 
    public const int RobotMoveException = 2014;
    public const int RobotResetException = 2015;
    public const int TriedToMoveToObstacle = 2016;
    public const int RobotIsBlocked = 2017;
    public const int RobotApiUnavailable = 2018;
    public const int RobotApiTimeout = 2019;
    public const int RobotApiConnectionError = 2020;
    
    
    // Map (3000-3099)
    public const int FailedToRetrieveMapData = 3000;
}