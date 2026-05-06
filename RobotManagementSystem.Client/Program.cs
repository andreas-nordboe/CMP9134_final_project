using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using RobotManagementSystem.Client.Services;
using RobotManagementSystem.Client.Services.Authentication;

namespace RobotManagementSystem.Client;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");
        builder.Services.AddMudServices();
        builder.Services.AddBlazoredLocalStorage();
        builder.Services.AddHttpClient("API",
            client => client.BaseAddress = new Uri("http://localhost:5085/"));
        
        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
        builder.Services.AddScoped<IAppState, AppState>();
        builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
        
        await builder.Build().RunAsync();
    }
}