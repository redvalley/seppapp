namespace SeppApp.Services;

public interface ISpeechToTextService
{
    Task StartListeningAsync(CancellationToken cancellationToken,
        Action<string> recognitionDone,
        Action permissionMissing,
        string? defaultWordSeperator = null,
        int? maxListenTimeMilliSeconds = null);

    Task Initialize();
}