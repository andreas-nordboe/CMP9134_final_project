using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using RobotManagementSystem.Client.AuthState;
using RobotManagementSystem.Client.Services;
using RobotManagementSystem.Client.Services.Admin;
using RobotManagementSystem.Client.Services.Authentication;
using RobotManagementSystem.Client.Services.DataStore;
using RobotManagementSystem.Client.Services.Map;
using RobotManagementSystem.Client.Services.MissionLogs;
using RobotManagementSystem.Client.Services.Robot;
using RobotManagementSystem.Client.Services.Sessions;
using RobotManagementSystem.Client.Services.SystemStatus;

namespace RobotManagementSystem.Client;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");
        builder.Services.AddMudServices(config =>
        {
            config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopLeft;
            config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
            
            config.SnackbarConfiguration.ShowCloseIcon = true;
            config.SnackbarConfiguration.VisibleStateDuration = 4000;
            config.SnackbarConfiguration.HideTransitionDuration = 250;
            config.SnackbarConfiguration.ShowTransitionDuration = 250;
            
            config.SnackbarConfiguration.NewestOnTop = false;
            config.SnackbarConfiguration.PreventDuplicates = false;
        });
        builder.Services.AddTransient<JWtAuthorisationHandler>();
        builder.Services.AddBlazoredLocalStorage();
        builder.Services.AddHttpClient("API", client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseAddress"]!);
        }).AddHttpMessageHandler<JWtAuthorisationHandler>();
        
        builder.Services.AddScoped(serviceProvider => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
        builder.Services.AddScoped<IAppState, AppState>();
        builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
        builder.Services.AddScoped<RobotHubCommunication>();
        builder.Services.AddScoped<IRobotCommanderService, RobotCommanderService>();
        builder.Services.AddScoped<IDataStoreService, DataStoreServiceService>();
        builder.Services.AddScoped<IUserSessionService, UserSessionService>();
        builder.Services.AddScoped<IAdminUserManagementService, AdminUserManagementService>();
        builder.Services.AddScoped<IMapService, MapService>();
        builder.Services.AddScoped<IMissionLogService, MissionLogService>();
        builder.Services.AddScoped<ISystemStatusLogService, SystemStatusLogService>();
        builder.Services.AddScoped<ISoundService, SoundService>();
        
        builder.Services.AddAuthorizationCore();

        builder.Services.AddScoped<CustomAuthStateProvider>();

        builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
            sp.GetRequiredService<CustomAuthStateProvider>());
        
        await builder.Build().RunAsync();
    }
}