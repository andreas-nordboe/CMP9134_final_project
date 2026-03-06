using RobotManagementSystem.Shared.Models.Authentication;

namespace RobotManagementSystem.Services.FailureHandling;

public interface IAPIFailureService
{
    public APIError CreateApiError(int code, string message);
    public bool ReturnAPIError(APIError apiError);
}