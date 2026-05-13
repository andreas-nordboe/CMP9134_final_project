using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using RobotManagementSystem.Services;
using RobotManagementSystem.Shared.Models.Map;
using RobotManagementSystem.Tests.Helpers;

namespace RobotManagementSystem.Tests.Map;

public class MapTests
{
    // Regression unit test for in case the model is accidentally changed during development
    [Fact] // Checks if the map model has changed by checking if JSON parsing is working properly (uses the schema that was provided by the lecturer)
    public void MapResponse_CheckModelDeserialising()
    {
        var expectedJsonFormat = """
        {
          "width": 21,
          "height": 21,
          "grid": [
            [1,0],
            [0,0]
          ]
        }
       """;

        var results = JsonSerializer.Deserialize<MapResponse>(expectedJsonFormat,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        Assert.NotNull(results);
        Assert.Equal(21, results.Width);
        Assert.Equal(21, results.Height);
        Assert.Equal(2, results.Grid.Length);
        Assert.Equal(2, results.Grid[0].Length);
        Assert.Equal(1, results.Grid[0][0]);
        Assert.Equal(0, results.Grid[1][0]);
    }

    // This one tests using the robot API service
    // future iterations could include integration tests that test the API endpoint
    [Fact]
    public async Task GetMapAsync_WhenApiReturnsValidMap_ItReturnsCorrectMapResponse()
    {
        var expectedJsonFormat = """
          {
            "width": 21,
            "height": 21,
            "grid": [
              [1,0],
              [0,0]
            ]
          }
         """;

        var httpClient = new HttpClient(
            new MockHttpMessageHandler(expectedJsonFormat, HttpStatusCode.OK))
        {
            BaseAddress = new Uri("http://localhost")
        };

        var logger = new Mock<ILogger<RobotApiService>>();
        var apiService = new RobotApiService(httpClient, logger.Object);

        var results = await apiService.GetMapAsync();
        Assert.NotNull(results);
        Assert.Equal(21, results.Width);
        Assert.Equal(21, results.Height);
        Assert.Equal(2, results.Grid.Length);
        Assert.Equal(2, results.Grid[0].Length);
        Assert.Equal(1, results.Grid[0][0]);
        Assert.Equal(0, results.Grid[1][0]);
    }
}