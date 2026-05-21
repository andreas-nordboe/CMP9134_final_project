using Newtonsoft.Json;
using RobotManagementSystem.Shared.Models.Authentication;
using RobotManagementSystem.Shared.Models.Errors;

namespace RobotManagementSystem.Shared.Utils;

public static class ErrorHandler
{
    public static APIError ParseApiError(string payload)
    {
        return JsonConvert.DeserializeObject<APIError>(payload) ?? 
               new APIError
               {
                   ErrorCode =  ErrorCodes.InternalServerError,
                   ErrorMessage = "Internal Server Error"
               };
    }
}