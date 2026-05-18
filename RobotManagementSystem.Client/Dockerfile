FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["RobotManagementSystem.Client/RobotManagementSystem.Client.csproj", "RobotManagementSystem.Client/"]
COPY ["RobotManagementSystem.Shared/RobotManagementSystem.Shared.csproj", "RobotManagementSystem.Shared/"]

RUN dotnet restore "RobotManagementSystem.Client/RobotManagementSystem.Client.csproj"

COPY . .

RUN dotnet publish "RobotManagementSystem.Client/RobotManagementSystem.Client.csproj" -c Release -o /app/publish

FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

COPY --from=build /app/publish/wwwroot .

EXPOSE 80