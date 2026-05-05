using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using RobotManagementSystem.Data;
using RobotManagementSystem.Services.FailureHandling;
using RobotManagementSystem.Services.Security;

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
            var db = scope.ServiceProvider.GetRequiredService<RobotApiDbContext>();
            db.Database.Migrate(); // auto migrates on startup
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

        app.UseAuthentication();
        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}