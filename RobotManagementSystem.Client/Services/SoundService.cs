using Microsoft.JSInterop;
using RobotManagementSystem.Client.Services.DataStore;

namespace RobotManagementSystem.Client.Services;

public class SoundService : ISoundService
{
    private readonly IJSRuntime JsRuntime;
    private readonly IDataStoreService _dataStoreService;

    public SoundService(IJSRuntime jsRuntime, IDataStoreService dataStoreService)
    {
        JsRuntime = jsRuntime;
        _dataStoreService = dataStoreService;
    }

    public async Task PlaySoundAsync(string soundName)
    {
        if(await _dataStoreService.LoadEnableSoundEffectsAsync() != true)
            return;
        
        // Sourced from: https://opengameart.org/content/short-alarm
        await JsRuntime.InvokeVoidAsync(
            "robotSoundEffects.play",
            $"/sounds/{soundName}"
        );
    }

    public async Task PlayErrorSoundAsync()
    {
        await PlaySoundAsync("error-sound.mp3");
    }
}