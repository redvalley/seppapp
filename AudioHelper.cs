using Plugin.Maui.Audio;

namespace SeppApp;

public static class AudioHelper
{
    public static IAudioPlayer CreateAudioPlayer(IAudioManager audioManager, string soundFile)
    {
        return audioManager.CreatePlayer(FileSystem.OpenAppPackageFileAsync(soundFile).Result);
    }
}