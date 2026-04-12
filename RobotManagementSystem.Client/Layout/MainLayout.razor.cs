namespace RobotManagementSystem.Client.Layout;

public partial class MainLayout
{
    private bool _sidebarOpen;

    void ToggleSidebar()
    {
        _sidebarOpen = !_sidebarOpen;
    }
}