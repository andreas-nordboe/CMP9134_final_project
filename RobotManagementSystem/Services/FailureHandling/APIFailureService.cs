using RobotManagementSystem.Shared.Models.Authentication;

namespace RobotManagementSystem.Services.FailureHandling;

public class APIFailureService : IAPIFailureService
{
    public APIError CreateApiError(int code, string message)
    {
        return new APIError
        {
            ErrorCode =  code,
            ErrorMessage = message
        };
    }

    public bool ReturnAPIError(APIError apiError)
    {
        throw new NotImplementedException();
    }
}