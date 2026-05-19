FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["RobotManagementSystem/RobotManagementSystem.csproj", "RobotManagementSystem/"]
COPY ["RobotManagementSystem.Client/RobotManagementSystem.Client.csproj", "RobotManagementSystem.Client/"]
COPY ["RobotManagementSystem.Shared/RobotManagementSystem.Shared.csproj", "RobotManagementSystem.Shared/"]

RUN dotnet restore "RobotManagementSystem/RobotManagementSystem.csproj"

COPY . .

WORKDIR "/src/RobotManagementSystem"
RUN dotnet build "RobotManagementSystem.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "RobotManagementSystem.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "RobotManagementSystem.dll"]