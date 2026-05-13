using RobotManagementSystem.Shared.Models.Map;

namespace RobotManagementSystem.Client.Services.Map;

public interface IMapService
{
    Task<MapResponse?> GetMapAsync();
}