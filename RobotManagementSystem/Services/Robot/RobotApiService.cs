    using System.Net;
    using RobotManagementSystem.Services.FailureHandling;
    using RobotManagementSystem.Services.MissionLogs;
    using RobotManagementSystem.Shared.Models.Components;
    using RobotManagementSystem.Shared.Models.Map;
    using RobotManagementSystem.Shared.Models.MissionLog;
    using RobotManagementSystem.Shared.Models.Robot;
    using RobotManagementSystem.Shared.Models.Users;

    namespace RobotManagementSystem.Services;

    public class RobotApiService : IRobotApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RobotApiService> _logger;
        private readonly IMissionLogsService _missionLogsService;
        private static readonly SemaphoreSlim _robotLock = new(1, 1);

        public RobotApiService(HttpClient httpClient, ILogger<RobotApiService> logger, IMissionLogsService missionLogsService)
        {
            _httpClient = httpClient;
            _logger = logger;
            _missionLogsService = missionLogsService;
        }

        public async Task<MapResponse?> GetMapAsync()
        { 
            try
            {
                return await _httpClient.GetFromJsonAsync<MapResponse>("/api/map");
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Failed to retrieve map information.");
                return null;
            }
        }

        public async Task<RobotCommandResponse?> MoveRobotAsync(RobotNavigationRequest request, int userId, UserRole role)
        {
            if (request == null)
            {
                return new RobotCommandResponse
                {
                    Success = false,
                    Message = "Failed to move robot due to invalid input."
                };
            }
            
            // Validate coordinates so they don't go out of bounds (TODO move this to helper later)
            // TODO: Code smell magic numbers
            if (request.X < 0 || request.Y < 0 || request.X > 20 || request.Y > 20)
            {
                await _missionLogsService.AddMissionLog(new AddMissionLogRequest
                {
                    UserId = userId,
                    Role = role,
                    Command = RobotCommand.Move,
                    CommandResult = RobotCommandResult.InvalidCoordinates,
                    Details = $"Tried to move robot to invalid coordinates ({request.X}, {request.Y})"
                });
                
                return new RobotCommandResponse
                {
                    Success = false,
                    Message = "Robot coordinates not valid and must be between 0 and 20."
                };
            }

            await _robotLock.WaitAsync();

            try
            {
                var response = await SendWithRetryAsync(() =>
                    _httpClient.PostAsJsonAsync("/api/move", request)
                );

                if (response == null)
                {
                    await _missionLogsService.AddMissionLog(new AddMissionLogRequest
                    {
                        UserId = userId,
                        Role = role,
                        Command = RobotCommand.Move,
                        CommandResult = RobotCommandResult.Failure,
                        Details = $"API unavailable after retries."
                    });
                    
                    return new RobotCommandResponse
                    {
                        Success = false,
                        Message = "Robot API unavailable after retries."
                    };
                }
                
                if (!response.IsSuccessStatusCode)
                {
                    await _missionLogsService.AddMissionLog(new AddMissionLogRequest
                    {
                        UserId = userId,
                        Role = role,
                        Command = RobotCommand.Move,
                        CommandResult = RobotCommandResult.Failure,
                        Details = $"API returned status code {response.StatusCode}"
                    });

                    return new RobotCommandResponse
                    {
                        Success = false,
                        Message =
                            $"Move command failed. Response: {response.StatusCode}." // TODO I'll try stautus code for now and try ReasonPhrase later 
                    };
                }

                await _missionLogsService.AddMissionLog(new AddMissionLogRequest
                {
                    UserId = userId,
                    Role = role,
                    Command = RobotCommand.Move,
                    CommandResult = RobotCommandResult.Success
                });

                return new RobotCommandResponse
                {
                    Success = true,
                    Message = $"Robot was moved to {request.X}, {request.Y}.",
                    RobotPosition = new Vector2D
                    {
                        X = request.X,
                        Y = request.Y
                    }
                };

            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Robot move command failed.");

                await _missionLogsService.AddMissionLog(new AddMissionLogRequest
                {
                    UserId = userId,
                    Role = role,
                    Command = RobotCommand.Move,
                    CommandResult = RobotCommandResult.Failure,
                    Details = $"Exception: {e.Message}"
                });

                return new RobotCommandResponse
                {
                    Success = false,
                    Message = "Robot move command failed."
                };
            }
            finally
            {
                _robotLock.Release();
            }
        }

        public async Task<RobotCommandResponse?> ResetAsync(int userId, UserRole userRole)
        {
            await _robotLock.WaitAsync();

            try
            {
                var response = await SendWithRetryAsync(() =>
                    _httpClient.PostAsync("/api/reset", null)
                );
                
                if (response == null)
                {
                    await _missionLogsService.AddMissionLog(new AddMissionLogRequest
                    {
                        UserId = userId,
                        Role = userRole,
                        Command = RobotCommand.Reset,
                        CommandResult = RobotCommandResult.Failure,
                        Details = $"API unavailable after retries."
                    });
                    
                    return new RobotCommandResponse
                    {
                        Success = false,
                        Message = "Robot API unavailable after retries."
                    };
                }

                if (!response.IsSuccessStatusCode)
                {
                    await _missionLogsService.AddMissionLog(new AddMissionLogRequest
                    {
                        UserId = userId,
                        Role = userRole,
                        Command = RobotCommand.Reset,
                        CommandResult = RobotCommandResult.Failure,
                        Details = $"API returned status code {response.StatusCode}"
                    });

                    return new RobotCommandResponse
                    {
                        Success = false,
                        Message =
                            $"Reset robot command failed. Response {response.StatusCode}" // TODO I'll try stautus code for now and try ReasonPhrase later 
                    };
                }

                await _missionLogsService.AddMissionLog(new AddMissionLogRequest
                {
                    UserId = userId,
                    Role = userRole,
                    Command = RobotCommand.Reset,
                    CommandResult = RobotCommandResult.Success
                });

                return new RobotCommandResponse
                {
                    Success = true,
                    Message =
                        "Robot was successfully reset." // TODO I'll try stautus code for now and try ReasonPhrase later 
                };
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Robot reset command failed.");

                await _missionLogsService.AddMissionLog(new AddMissionLogRequest
                {
                    UserId = userId,
                    Role = userRole,
                    Command = RobotCommand.Reset,
                    CommandResult = RobotCommandResult.Failure,
                    Details = $"Exception: {e.Message}"
                });

                return new RobotCommandResponse
                {
                    Success = false,
                    Message = "Robot reset command failed."
                };
            }
            finally
            {
                _robotLock.Release();
            }
        }

        // Backoff logic that retries the request until it succeeds or the maximum number of retries is reached
        private async Task<HttpResponseMessage?> SendWithRetryAsync(Func<Task<HttpResponseMessage>> action)
        {
            var delays = new[] { TimeSpan.FromMilliseconds(500), TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(5) };
            foreach (var delay in delays)
            {
                try
                {
                    var response = await action();

                    if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                    {
                        _logger.LogWarning("Robot API returned 503. Retrying...");
                        await Task.Delay(delay);
                        continue;
                    }

                    return response;
                }
                catch (TaskCanceledException ex)
                {
                    _logger.LogWarning(ex, "Robot API timeout. Retrying...");
                    await Task.Delay(delay);
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogWarning(ex, "Robot API connection failed. Retrying...");
                    await Task.Delay(delay);
                }
            }

            return null;
        }
    }