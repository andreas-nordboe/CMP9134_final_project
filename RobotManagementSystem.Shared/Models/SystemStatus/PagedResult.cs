namespace RobotManagementSystem.Shared.Models.SystemStatus;

public class PagedResult<T>
{
    public List<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
}