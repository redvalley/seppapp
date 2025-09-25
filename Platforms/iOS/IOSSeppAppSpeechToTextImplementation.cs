using System.Diagnostics.CodeAnalysis;
using AVFoundation;
using CommunityToolkit.Maui.Media;
using Foundation;
using SeppApp.Services;
using Speech;
using UIKit;

namespace SeppApp;

public class IOSSeppAppSpeechToTextImplementation : SeppAppSpeechToTextImplementation
{
    AVAudioEngine? _audioEngine;
    SFSpeechRecognizer? _speechRecognizer;
    SFSpeechRecognitionTask? _recognitionTask;
    SFSpeechAudioBufferRecognitionRequest? _liveSpeechRequest;
    private readonly SFSpeechLanguageModelConfiguration _languageModelConfiguration;
    private bool _customLmGenerated = false;

    /// <inheritdoc/>
    public override SpeechToTextState CurrentState => _recognitionTask?.State is SFSpeechRecognitionTaskState.Running
        ? SpeechToTextState.Listening
        : SpeechToTextState.Stopped;

    public IOSSeppAppSpeechToTextImplementation()
    {
        _languageModelConfiguration = new SFSpeechLanguageModelConfiguration();
        var _languageModelCacheDirectory =
            NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.CachesDirectory, NSSearchPathDomain.User)[0]
                .Path;
        if (_languageModelCacheDirectory != null)
        {
            var dynamicLanguageModel = Path.Combine(_languageModelCacheDirectory, "LM");
            var dynamicVocabulary = Path.Combine(_languageModelCacheDirectory, "Vocab");

            _languageModelConfiguration =
                new SFSpeechLanguageModelConfiguration(NSUrl.FromFilename(dynamicLanguageModel),
                    NSUrl.FromFilename(dynamicVocabulary));
        }
    }


    public async Task PrepareCustomLm()
    {
        if (_customLmGenerated)
        {
            return;
        }

        var lmUrl = NSBundle.MainBundle.GetUrlForResource("seppapplm", "bin");
        await SFSpeechLanguageModel.PrepareCustomModelAsync(lmUrl, "com.redvalleysoftware.SeppAppLanguageModel",
            _languageModelConfiguration);
        _customLmGenerated = true;
    }


    /// <inheritdoc />
    public override ValueTask DisposeAsync()
    {
        CleanUp();
        return ValueTask.CompletedTask;
    }

    private void CleanUp()
    {
        _audioEngine?.Dispose();
        _speechRecognizer?.Dispose();
        _liveSpeechRequest?.Dispose();
        _recognitionTask?.Dispose();

        _audioEngine = null;
        _speechRecognizer = null;
        _liveSpeechRequest = null;
        _recognitionTask = null;
    }

    internal static SpeechToTextResult Failed(Exception exception) => new SpeechToTextResult(null, exception);

    internal static SpeechToTextResult Success(string text) => new SpeechToTextResult(text, null);

    [MemberNotNull(nameof(_audioEngine), nameof(_recognitionTask), nameof(_liveSpeechRequest))]
    protected override Task InternalStartListeningAsync(SpeechToTextOptions options,
        CancellationToken cancellationToken)
    {
        _speechRecognizer = new SFSpeechRecognizer(NSLocale.FromLocaleIdentifier(options.Culture.Name));

        if (!_speechRecognizer.Available)
        {
            throw new ArgumentException("Speech recognizer is not available");
        }

        _audioEngine = new AVAudioEngine();
        _liveSpeechRequest = new SFSpeechAudioBufferRecognitionRequest()
        {
            ShouldReportPartialResults = options.ShouldReportPartialResults,
            //ContextualStrings = ["oach", "katzl", "kazl", "kazel", "katsl", "kadsl", "katzel", "schwoaf", "oachkatzlschwoaf", "oachkatzelschwoaf","ochkazlschwoaf",
            //    "Oach", "och", "Katzl", "Kazl", "Katzel", "Katsl", "Kazel", "Kadsl", "Schwoaf", "Oachkatzlschwoaf", "Oachkatzelschwoaf", "Ochkazlschwoaf"],
            RequiresOnDeviceRecognition = true
        };

        if (_customLmGenerated && _languageModelConfiguration != null)
        {
            _liveSpeechRequest.CustomizedLanguageModel = _languageModelConfiguration;
        }

        InitializeAvAudioSession(out _);

        var node = _audioEngine.InputNode;
        var recordingFormat = node.GetBusOutputFormat(0);
        node.InstallTapOnBus(0, 1024, recordingFormat, (buffer, _) => _liveSpeechRequest.Append(buffer));

        _audioEngine.Prepare();
        _audioEngine.StartAndReturnError(out var error);

        if (error is not null)
        {
            throw new ArgumentException("Error starting audio engine - " + error.LocalizedDescription);
        }

        cancellationToken.ThrowIfCancellationRequested();

        var currentIndex = 0;
        _recognitionTask = _speechRecognizer.GetRecognitionTask(_liveSpeechRequest, (result, err) =>
        {
            if (err is not null)
            {
                StopRecording();
                OnRecognitionResultCompleted(Failed(new Exception(err.LocalizedDescription)));
            }
            else
            {
                if (result.Final)
                {
                    currentIndex = 0;
                    StopRecording();
                    OnRecognitionResultCompleted(Success(result.BestTranscription.FormattedString));
                }
                else
                {
                    if (currentIndex <= 0)
                    {
                        OnSpeechToTextStateChanged(CurrentState);
                    }

                    for (var i = currentIndex; i < result.BestTranscription.Segments.Length; i++)
                    {
                        var s = result.BestTranscription.Segments[i].Substring;
                        currentIndex++;
                        OnRecognitionResultUpdated(s);
                    }
                }
            }
        });

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override Task<bool> RequestPermissions(CancellationToken cancellationToken = default)
    {
        var taskResult = new TaskCompletionSource<bool>();

        SFSpeechRecognizer.RequestAuthorization(status =>
            taskResult.SetResult(status is SFSpeechRecognizerAuthorizationStatus.Authorized));

        return taskResult.Task.WaitAsync(cancellationToken);
    }

    protected override Task<bool> IsSpeechPermissionAuthorized(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(
            SFSpeechRecognizer.AuthorizationStatus is SFSpeechRecognizerAuthorizationStatus.Authorized);
    }

    static void InitializeAvAudioSession(out AVAudioSession sharedAvAudioSession)
    {
        sharedAvAudioSession = AVAudioSession.SharedInstance();
        if (UIDevice.CurrentDevice.CheckSystemVersion(15, 0))
        {
            sharedAvAudioSession.SetSupportsMultichannelContent(true, out _);
        }

        sharedAvAudioSession.SetCategory(
            AVAudioSessionCategory.PlayAndRecord,
            AVAudioSessionCategoryOptions.DefaultToSpeaker | AVAudioSessionCategoryOptions.AllowBluetooth |
            AVAudioSessionCategoryOptions.AllowAirPlay | AVAudioSessionCategoryOptions.AllowBluetoothA2DP);
    }

    void StopRecording()
    {
        _audioEngine?.InputNode.RemoveTapOnBus(0);
        _audioEngine?.Stop();
        _liveSpeechRequest?.EndAudio();
        _recognitionTask?.Cancel();
        OnSpeechToTextStateChanged(CurrentState);

        CleanUp();
    }

    protected override Task InternalStopListeningAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        StopRecording();
        return Task.CompletedTask;
    }
}