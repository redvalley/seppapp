using System.Globalization;
using CommunityToolkit.Maui.Media;
using RedValley;
using RedValley.Extensions;
using RedValley.Helper;
using SeppApp.Models;

namespace SeppApp.Services;

public class SpeechToTextService : ISpeechToTextService
{
    private readonly ISpeechToText _speechToText;

    private string _recognizedText = string.Empty;
    private Action<string>? _recognitionDoneHandler;
    public const int DefaultMaxListenTimeMilliSeconds = 5000;
    private string _defaultWordSeperator = string.Empty;

    public SpeechToTextService(ISpeechToText speechToText)
    {
        _speechToText = speechToText;
    }

    public async Task Initialize()
    {
        await ExceptionHelper.TryAsync("SpeechToTextService.Initialize", async () =>
        {
#if IOS
            if (_speechToText is IOSSeppAppSpeechToTextImplementation iosSeppAppSpeechToTextImplementation)
            {
                await iosSeppAppSpeechToTextImplementation.PrepareCustomLm();
            }
#else
            await Task.Run(()=>{});
#endif
        }, Logging.CreateCoreLogger());
    }

    public async Task StartListeningAsync(CancellationToken cancellationToken, 
        Action<string> recognitionDone, 
        Action permissionMissing,
        string? defaultWordSeperator = null,
        int? maxListenTimeMilliSeconds = null)
    {
        await ExceptionHelper.TryAsync("SpeechToTextService.StartListeningAsync", async () =>
        {

            var isGranted = await _speechToText.RequestPermissions(cancellationToken);
            if (!isGranted)
            {
                permissionMissing?.Invoke();
                return;
            }

            _defaultWordSeperator = defaultWordSeperator ?? string.Empty;

            _recognizedText = string.Empty;
            _recognitionDoneHandler = recognitionDone;
#if IOS
            _speechToText.RecognitionResultUpdated -= OnSpeechToTextOnRecognitionResultUpdated;
            _speechToText.RecognitionResultUpdated += OnSpeechToTextOnRecognitionResultUpdated;
#else
            _speechToText.RecognitionResultCompleted -= OnSpeechToTextOnRecognitionResultCompleted;
            _speechToText.RecognitionResultCompleted += OnSpeechToTextOnRecognitionResultCompleted;
#endif

            var germanCultureInfo = CultureInfo.GetCultureInfo("de-DE");
            await _speechToText.StartListenAsync(new SpeechToTextOptions()
                {
                    Culture = germanCultureInfo,
#if IOS
                    ShouldReportPartialResults = true,
#else
                    ShouldReportPartialResults = false
#endif
            }
            );

            

            if (maxListenTimeMilliSeconds != null)
            {
                await Task.Delay(maxListenTimeMilliSeconds.Value);
                await _speechToText.StopListenAsync();
                _speechToText.RecognitionResultUpdated -= OnSpeechToTextOnRecognitionResultUpdated;

                _recognitionDoneHandler?.Invoke(_recognizedText);
            }
        }, Logging.CreateCoreLogger());
    }

    private void OnSpeechToTextOnRecognitionResultCompleted(object? sender,
        SpeechToTextRecognitionResultCompletedEventArgs e)
    {
#if !IOS
        _speechToText.RecognitionResultCompleted -= OnSpeechToTextOnRecognitionResultCompleted;
        if (e.RecognitionResult?.Text?.IsNotEmpty()??false)
        {
            _recognizedText = e.RecognitionResult?.Text??string.Empty;
            _recognitionDoneHandler?.Invoke(_recognizedText);
        }
#endif
    }

    private void OnSpeechToTextOnRecognitionResultUpdated(object? sender,
        SpeechToTextRecognitionResultUpdatedEventArgs e)
    {
#if IOS
        if (e.RecognitionResult.IsNotEmpty())
        {
            if (_recognizedText.IsNotEmpty() && e.RecognitionResult.IsNotEmpty() && _defaultWordSeperator.IsNotEmpty())
            {
                _recognizedText += _defaultWordSeperator;
            }
            
            _recognizedText += e.RecognitionResult;
        }
#endif
    }
}