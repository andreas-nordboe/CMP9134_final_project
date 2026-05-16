using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using RobotManagementSystem.Controllers;
using RobotManagementSystem.Services;
using RobotManagementSystem.Services.FailureHandling;
using RobotManagementSystem.Services.MissionLogs;
using RobotManagementSystem.Shared.Models.Authentication;
using RobotManagementSystem.Shared.Models.Errors;
using RobotManagementSystem.Shared.Models.Robot;

namespace RobotManagementSystem.Tests.Robot;

public class RobotCommandsControllerTests
{
    [Fact]
    public async Task GetRobotStatus_ReturnsCorrectModelOnSuccess()
    {
        var robotApiService = new Mock<IRobotApiService>();
        var logger = new Mock<ILogger<RobotCommandsController>>();
        var apiFailureService = new Mock<IAPIFailureService>();
        var robotStatusService = new Mock<IRobotStatusService>();
        var missionLogService = new Mock<IMissionLogsService>();
        
        robotStatusService
            .Setup(x => x.GetRobotStatusAsync())
            .ReturnsAsync(new RobotStatusResponse());
        
        apiFailureService
            .Setup(x => x.CreateApiError(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(new APIError
            {
                ErrorCode = ErrorCodes.InvalidRequest,
                ErrorMessage = ErrorMessages.InvalidRequest
            });

        var robotApiController = new RobotCommandsController(
            logger.Object,
            robotApiService.Object,
            apiFailureService.Object,
            missionLogService.Object,
            robotStatusService.Object);

        var results = await robotApiController.GetRobotStatus();
        var okResult = Assert.IsType<OkObjectResult>(results.Result);
        
        var response = Assert.IsType<RobotStatusResponse>(okResult.Value);
        
        robotStatusService.Verify(x => x.GetRobotStatusAsync(), Times.Once);
    }
    
    [Fact]
    public async Task GetRobotStatus_ReturnsUnavailableOnHttpFailure()
    {
        var robotApiService = new Mock<IRobotApiService>();
        var logger = new Mock<ILogger<RobotCommandsController>>();
        var apiFailureService = new Mock<IAPIFailureService>();
        var robotStatusService = new Mock<IRobotStatusService>();
        var missionLogService = new Mock<IMissionLogsService>();
        
        robotStatusService
            .Setup(x => x.GetRobotStatusAsync())
            .ReturnsAsync((RobotStatusResponse?)null);
        
        apiFailureService
            .Setup(x => x.CreateApiError(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(new APIError
            {
                ErrorCode = ErrorCodes.RobotApiNotAvailable,
                ErrorMessage = ErrorMessages.RobotApiNotAvailable
            });

        var robotApiController = new RobotCommandsController(
            logger.Object,
            robotApiService.Object,
            apiFailureService.Object,
            missionLogService.Object,
            robotStatusService.Object);

        var results = await robotApiController.GetRobotStatus();
        var okResult = Assert.IsType<ObjectResult>(results.Result);
        
        var error = Assert.IsType<APIError>(okResult.Value);
        Assert.Equal(ErrorCodes.RobotApiNotAvailable, error.ErrorCode);
        Assert.Equal(ErrorMessages.RobotApiNotAvailable, error.ErrorMessage);
        
        robotStatusService.Verify(x => x.GetRobotStatusAsync(), Times.Once);
    }
}