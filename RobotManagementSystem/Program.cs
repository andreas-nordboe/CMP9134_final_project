using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using RobotManagementSystem.BackgroundServices;
using RobotManagementSystem.Data;
using RobotManagementSystem.Hubs;
using RobotManagementSystem.Services;
using RobotManagementSystem.Services.Authentication;
using RobotManagementSystem.Services.FailureHandling;
using RobotManagementSystem.Services.Security;
using RobotManagementSystem.Shared.Models.Users;

namespace RobotManagementSystem;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        
        // Allow frontend to send HTTP requests with the backend
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("Frontend", policy =>
            {
                policy.WithOrigins("http://localhost:5116").AllowAnyHeader().AllowAnyMethod();
            });
        });
        
        builder.Services.AddOpenApi(options =>
        {
            options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0; // I had some issues with default JSON content on the Swagger UI, so I downgraded from 3.1 to 3.0
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

                document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    BearerFormat = "JWT",
                    Description = "Enter JWT Access Token"
                };

                document.Security ??= new List<OpenApiSecurityRequirement>();
                
                document.Security.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                });

                return Task.CompletedTask;
            });
            
        });
        
        // Add services
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<IAPIFailureService, APIFailureService>();
        builder.Services.AddScoped<IPasswordService, PasswordService>();
        builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
        builder.Services.AddHostedService<RobotTelemetryBackgroundService>();

        // Adds a Httpclient using the IRobotApi service to interact with the external RobotApi
        builder.Services.AddHttpClient<IRobotApiService, RobotApiService>(client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["RobotApi:BaseAddress"] ?? throw new InvalidOperationException()); // TODO this stops the backend from working if the RobotApi is missing, imrpove with bette error handling and logging later
            client.Timeout = TimeSpan.FromSeconds(10);
        });
        builder.Services.AddSignalR();
        
        // Setup Authentication (JWT Token for now, this might be replaced with OIDC later)
        //builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);
        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, jwtBearerOptions =>
            {
                jwtBearerOptions.RequireHttpsMetadata = false;
                jwtBearerOptions.SaveToken = true;
                jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["JWTConfiguration:SecretKey"] ?? string.Empty)), // Falls back to empty string if JWT is missing to avoid crashes
                    ValidIssuer = builder.Configuration["JWTConfiguration:Issuer"],
                    ValidAudience = builder.Configuration["JWTConfiguration:Audience"],
                    ClockSkew = TimeSpan.Zero,
                    ValidateLifetime = true,
                };
            });

        builder.Services.AddDbContext<RobotApiDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddAuthorization();
        
        var app = builder.Build();
        
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<RobotApiDbContext>();
            var passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();
            dbContext.Database.Migrate(); // auto migrates on startup

            if (!dbContext.Users.Any(user => user.Role == UserRole.Admin))
            {
                var administrator = new UserAccount
                {
                    Username = builder.Configuration["SeedAdminUser:Username"]!,
                    FirstName = "System",
                    LastName = "Administrator",
                    PasswordHash = passwordService.HashPassword(builder.Configuration["SeedAdminUser:Password"]!),
                    Role = UserRole.Admin
                };
                
                dbContext.Users.Add(administrator);
                dbContext.SaveChanges();
            }
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi/v1.json", "Robot Management System");
                options.RoutePrefix = string.Empty;
            });
        }

        app.UseHttpsRedirection();
        app.UseCors("Frontend");

        app.UseAuthentication();
        app.UseAuthorization();


        app.MapControllers();
        app.MapHub<RobotTelemetryHub>("/hubs/robot-telemetry");

        app.Run();
    }
}