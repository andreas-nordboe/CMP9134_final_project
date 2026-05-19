using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using RobotManagementSystem.Controllers;
using RobotManagementSystem.Services.Authentication;
using RobotManagementSystem.Services.FailureHandling;
using RobotManagementSystem.Services.Security;
using RobotManagementSystem.Shared.Models.Authentication;
using RobotManagementSystem.Shared.Models.Errors;

namespace RobotManagementSystem.Tests;

public class AuthenticationControllerTests
{
    [Fact]
    public async Task ReturnsBadRequest_WhenRequestIsNull()
    {
        var tokenService = new Mock<ITokenService>();
        var apiFailureService = new Mock<IAPIFailureService>();
        var passwordService = new Mock<IPasswordService>();
        var authenticationService = new Mock<IAuthenticationService>();
        var logger = new Mock<ILogger<AuthenticationController>>();

        apiFailureService
            .Setup(x => x.CreateApiError(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(new APIError
            {
                ErrorCode = ErrorCodes.InvalidRequest,
                ErrorMessage = ErrorMessages.InvalidRequest
            });

        var authController = new AuthenticationController(
            apiFailureService.Object,
            logger.Object,
            authenticationService.Object
        );

        var results = await authController.Login(null);
        
        Assert.IsType<BadRequestObjectResult>(results.Result);
    }
}
