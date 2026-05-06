using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using RobotManagementSystem.Data;
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
        builder.Services.AddOpenApi();
        
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("Frontend", policy =>
            {
                policy.WithOrigins("http://localhost:5116").AllowAnyHeader().AllowAnyMethod();
            });
        });
        
        // Add Swagger
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "Robot Management System API",
                Description = "An ASP.NET Core Web API for controlling an autonomous robot",
            });
        });

        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<IAPIFailureService, APIFailureService>();
        builder.Services.AddScoped<IPasswordService, PasswordService>();
        
        // Setup Authentication (JWT Token for now, this might be replaced with OIDC later)
        //builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);
        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer("JwtBearer", jwtBearerOptions =>
            {
                jwtBearerOptions.RequireHttpsMetadata = false;
                jwtBearerOptions.SaveToken = true;
                jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["JWTSettings:SecretKey"] ?? string.Empty)), // Falls back to empty string if JWT is missing to avoid crashes
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
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Robot Management System");
                options.RoutePrefix = string.Empty;
            });
        }

        app.UseHttpsRedirection();
        app.UseCors("Frontend");

        app.UseAuthentication();
        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}