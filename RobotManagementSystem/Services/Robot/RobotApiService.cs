    using System.Net;
    using RobotManagementSystem.Services.FailureHandling;
    using RobotManagementSystem.Services.MissionLogs;
    using RobotManagementSystem.Shared.Models.Components;
    using RobotManagementSystem.Shared.Models.Errors;
    using RobotManagementSystem.Shared.Models.Map;
    using RobotManagementSystem.Shared.Models.MissionLog;
    using RobotManagementSystem.Shared.Models.Robot;
    using RobotManagementSystem.Shared.Models.Users;

    namespace RobotManagementSystem.Services;
    
    // Tasks: Validate user permissions, send commands to the robot, persist the command before it is sent (in case of latency drops), update command state, possibly handle errors and unknown states
    // State machine would be useful here 
    // States could include Sent, Confirmed, Failed and Unknown
    // backoff could be useful (e.g., 1s, 2s, 4s, 8s, 15s..)

    public class RobotApiService : IRobotApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RobotApiService> _logger;
        private readonly IMissionLogsService _missionLogsService;
        private static readonly SemaphoreSlim _robotLock = new(1, 1);
        private readonly IRobotCommandRateLimiter _commandRateLimiter;

        public RobotApiService(HttpClient httpClient, ILogger<RobotApiService> logger, IMissionLogsService missionLogsService, IRobotCommandRateLimiter commandRateLimiter)
        {
            _httpClient = httpClient;
            _logger = logger;
            _missionLogsService = missionLogsService;
            _commandRateLimiter = commandRateLimiter;
        }

        public async Task<MapResponse?> GetMapAsync()
        { 
            try
            {
                return await _httpClient.GetFromJsonAsync<MapResponse>("/api/map");
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, ErrorMessages.FailedToRetrieveMapData);
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
                    Message = ErrorMessages.RobotMoveCommandFailed
                };
            }
            
            var map = await GetMapAsync();
            
            // I think it is safer to test that the map and map grid tiles exist before sending the request
            if (map == null || map.Grid.Length == 0)
            {
                await _missionLogsService.AddMissionLog(new AddMissionLogRequest
                {
                    UserId = userId,
                    Role = role,
                    Command = RobotCommand.Move,
                    CommandResult = RobotCommandResult.Failure,
                    Details = ErrorMessages.RobotMapDoesNotExist
                });

                return new RobotCommandResponse
                {
                    Success = false,
                    Message = ErrorMessages.RobotMapDoesNotExist
                };
            }

            // Check invalid coordinates
            if (request.X < 0 || request.Y < 0 || request.X >= map.Width || request.Y >= map.Height || request.Y >= map.Grid.Length || request.X >= map.Grid[request.Y].Length)
            {
                await _missionLogsService.AddMissionLog(new AddMissionLogRequest
                {
                    UserId = userId,
                    Role = role,
                    Command = RobotCommand.Move,
                    CommandResult = RobotCommandResult.InvalidCoordinates,
                    Details = $"{ErrorMessages.RobotAttemptedMove} ({request.X}, {request.Y})"
                });

                return new RobotCommandResponse
                {
                    Success = false,
                    Message = ErrorMessages.RobotCoordinatesNotValid
                };
            }
            
            // Check if blocked by an obstacle
            if (map.Grid[request.Y][request.X] == 1)
            {
                await _missionLogsService.AddMissionLog(new AddMissionLogRequest
                {
                    UserId = userId,
                    Role = role,
                    Command = RobotCommand.Move,
                    CommandResult = RobotCommandResult.BlockedByObstacle,
                    Details = $"{ErrorMessages.TriedToMoveToObstacle} ({request.X}, {request.Y})"
                });

                return new RobotCommandResponse
                {
                    Success = false,
                    Message = ErrorMessages.RobotIsBlocked
                };
            }

            await _robotLock.WaitAsync();

            try
            {
                await _commandRateLimiter.WaitAsync();
                
                var response = await SendWithRetryAsync(() =>
                    _httpClient.PostAsJsonAsync("/api/move", request),
                    userId,
                    role,
                    RobotCommand.Move
                );

                if (response == null)
                {
                    await _missionLogsService.AddMissionLog(new AddMissionLogRequest
                    {
                        UserId = userId,
                        Role = role,
                        Command = RobotCommand.Move,
                        CommandResult = RobotCommandResult.Failure,
                        Details = ErrorMessages.RobotUnavaiableAfterRetrying
                    });
                    
                    return new RobotCommandResponse
                    {
                        Success = false,
                        Message = ErrorMessages.RobotUnavaiableAfterRetrying
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
                        Details = $"{ErrorMessages.RobotApiReturnedStatusCode} {response.StatusCode}"
                    });

                    return new RobotCommandResponse
                    {
                        Success = false,
                        Message =
                            $"{ErrorMessages.RobotMoveCommandFailed} Response: {response.StatusCode}."
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
                    Message = $"{ErrorMessages.RobotMoveCommandSuccess} {request.X}, {request.Y}.",
                    RobotPosition = new Vector2D
                    {
                        X = request.X,
                        Y = request.Y
                    }
                };

            }
            catch (Exception e)
            {
                _logger.LogWarning(e, ErrorMessages.RobotCommandFailed);

                await _missionLogsService.AddMissionLog(new AddMissionLogRequest
                {
                    UserId = userId,
                    Role = role,
                    Command = RobotCommand.Move,
                    CommandResult = RobotCommandResult.Failure,
                    Details = $"{ErrorMessages.RobotMoveException} {e.Message}"
                });

                return new RobotCommandResponse
                {
                    Success = false,
                    Message = ErrorMessages.RobotCommandFailed
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
                await _commandRateLimiter.WaitAsync();
                
                var response = await SendWithRetryAsync(() =>
                    _httpClient.PostAsync("/api/reset", null),
                    userId,
                    userRole,
                    RobotCommand.Reset
                );
                
                if (response == null)
                {
                    await _missionLogsService.AddMissionLog(new AddMissionLogRequest
                    {
                        UserId = userId,
                        Role = userRole,
                        Command = RobotCommand.Reset,
                        CommandResult = RobotCommandResult.Failure,
                        Details = ErrorMessages.RobotUnavaiableAfterRetrying
                    });
                    
                    return new RobotCommandResponse
                    {
                        Success = false,
                        Message = ErrorMessages.RobotUnavaiableAfterRetrying
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
                        Details = $"{ErrorMessages.RobotApiReturnedStatusCode} {response.StatusCode}"
                    });

                    return new RobotCommandResponse
                    {
                        Success = false,
                        Message = $"{ErrorMessages.RobotResetCommandFailed} Response {response.StatusCode}" 
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
                    Message = ErrorMessages.RobotResetCommandSuccess
                };
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, ErrorMessages.RobotResetCommandFailed);

                await _missionLogsService.AddMissionLog(new AddMissionLogRequest
                {
                    UserId = userId,
                    Role = userRole,
                    Command = RobotCommand.Reset,
                    CommandResult = RobotCommandResult.Failure,
                    Details = $"{ErrorMessages.RobotResetException} {e.Message}"
                });

                return new RobotCommandResponse
                {
                    Success = false,
                    Message = ErrorMessages.RobotResetCommandFailed
                };
            }
            finally
            {
                _robotLock.Release();
            }
        }

        // Backoff logic that retries the request until it succeeds or the maximum number of retries is reached
        private async Task<HttpResponseMessage?> SendWithRetryAsync(Func<Task<HttpResponseMessage>> action, int userId, UserRole role, RobotCommand command)
        {
            var delays = new[]
            {
                TimeSpan.Zero,
                TimeSpan.FromMilliseconds(500),
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(4),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(8)
            };

            for (var attempt = 0; attempt < delays.Length; attempt++)
            {
                var delay = delays[attempt];

                if (delay > TimeSpan.Zero)
                    await Task.Delay(delay);

                try
                {
                    var response = await action();

                    if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                    {
                        await LogRetryAttemptAsync(
                            userId,
                            role,
                            command,
                            attempt + 1,
                            ErrorMessages.RobotApiUnavailable);

                        _logger.LogWarning(ErrorMessages.RobotApiUnavailableRetrying);
                        response.Dispose();
                        continue;
                    }

                    return response;
                }
                catch (TaskCanceledException ex)
                {
                    await LogRetryAttemptAsync(
                        userId,
                        role,
                        command,
                        attempt + 1,
                        ErrorMessages.RobotApiTimeout);

                    _logger.LogWarning(ex, ErrorMessages.RobotApiTimeoutRetrying);
                }
                catch (HttpRequestException ex)
                {
                    await LogRetryAttemptAsync(
                        userId,
                        role,
                        command,
                        attempt + 1,
                        ErrorMessages.RobotApiConnectionError);

                    _logger.LogWarning(ex, ErrorMessages.RobotApiConnectionError);
                }
            }

            return null;
        }
        
        private async Task LogRetryAttemptAsync(int userId, UserRole role, RobotCommand command, int attempt, string reason)
        {
            await _missionLogsService.AddMissionLog(new AddMissionLogRequest
            {
                UserId = userId,
                Role = role,
                Command = command,
                CommandResult = RobotCommandResult.Retried,
                Details = $"{reason} Retry attempt {attempt} failed."
            });
        }
    }