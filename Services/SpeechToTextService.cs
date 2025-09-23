using System.Globalization;
using CommunityToolkit.Maui.Media;
using RedValley.Extensions;
using SeppApp.Models;

namespace SeppApp.Services;

public class SpeechToTextService : ISpeechToTextService
{
    private readonly ISpeechToText _speechToText;

    private string _recognizedText;
    private Action<string>? _recognitionDoneHandler;
    public const int DefaultMaxListenTimeMilliSeconds = 5000;
    private string _defaultWordSeperator;

    public SpeechToTextService(ISpeechToText speechToText)
    {
        _speechToText = speechToText;
    }

    public async Task StartListeningAsync(CancellationToken cancellationToken, 
        Action<string> recognitionDone, 
        Action permissionMissing,
        string? defaultWordSeperator = null,
        int? maxListenTimeMilliSeconds = null)
    {
        var isGranted = await _speechToText.RequestPermissions(cancellationToken);
        if (!isGranted)
        {
            permissionMissing?.Invoke();
            return;
        }

        _defaultWordSeperator = defaultWordSeperator??string.Empty;
        
        _recognizedText = string.Empty;
        _recognitionDoneHandler = recognitionDone;
        var currentMaxListenTimeMilliSeconds = maxListenTimeMilliSeconds;
#if IOS
        if (currentMaxListenTimeMilliSeconds == null)
        {
            currentMaxListenTimeMilliSeconds = DefaultMaxListenTimeMilliSeconds;
        }

        _speechToText.RecognitionResultUpdated -= OnSpeechToTextOnRecognitionResultUpdated;
        _speechToText.RecognitionResultUpdated += OnSpeechToTextOnRecognitionResultUpdated;
#else
      _speechToText.RecognitionResultCompleted -= OnSpeechToTextOnRecognitionResultCompleted;
      _speechToText.RecognitionResultCompleted += OnSpeechToTextOnRecognitionResultCompleted;
#endif


        await _speechToText.StartListenAsync(new SpeechToTextOptions()
            {
                Culture = CultureInfo.CurrentCulture,
#if IOS
                ShouldReportPartialResults = true,
#endif
            }
        );


        if (currentMaxListenTimeMilliSeconds != null)
        {
            await Task.Delay(currentMaxListenTimeMilliSeconds.Value);
            await _speechToText.StopListenAsync();
            _speechToText.RecognitionResultUpdated -= OnSpeechToTextOnRecognitionResultUpdated;

            _recognitionDoneHandler?.Invoke(_recognizedText);
        }
    }


    private void OnSpeechToTextOnRecognitionResultCompleted(object? sender,
        SpeechToTextRecognitionResultCompletedEventArgs e)
    {
#if !IOS
        _speechToText.RecognitionResultCompleted -= OnSpeechToTextOnRecognitionResultCompleted;
        if (e.RecognitionResult?.Text?.IsNotEmpty()??false)
        {
            _recognizedText = e.Result.Text;
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