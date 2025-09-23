using AVFoundation;
using Foundation;
using Speech;
using System.Globalization;

namespace SeppApp;

public class IOSSpeechRecognition : ISpeechRecognition
{
    private AVAudioEngine audioEngine;
    private SFSpeechAudioBufferRecognitionRequest liveSpeechRequest;
    private SFSpeechRecognizer speechRecognizer;
    private SFSpeechRecognitionTask recognitionTask;

    public async Task<string> Listen(CultureInfo culture, IProgress<string> recognitionResult,
        CancellationToken cancellationToken)
    {
        speechRecognizer = new SFSpeechRecognizer(NSLocale.FromLocaleIdentifier(culture.Name));

        if (!speechRecognizer.Available)
        {
            throw new ArgumentException("Speech recognizer is not available");
        }

        if (SFSpeechRecognizer.AuthorizationStatus != SFSpeechRecognizerAuthorizationStatus.Authorized)
        {
            throw new Exception("Permission denied");
        }

        var audioSession = AVAudioSession.SharedInstance();
        audioSession.SetCategory(AVAudioSessionCategory.PlayAndRecord, AVAudioSessionMode.Measurement, AVAudioSessionCategoryOptions.DuckOthers);
        audioSession.SetActive(true, AVAudioSessionSetActiveOptions.NotifyOthersOnDeactivation);

        audioEngine = new AVAudioEngine();
        liveSpeechRequest = new SFSpeechAudioBufferRecognitionRequest();
        liveSpeechRequest.ShouldReportPartialResults = true;

        // Toggle to use online or offline
        //if (OperatingSystem.IsIOSVersionAtLeast(13))
        //    liveSpeechRequest.RequiresOnDeviceRecognition = true;


        var node = audioEngine.InputNode;
        var recordingFormat = node.GetBusOutputFormat(new UIntPtr(0));
        node.InstallTapOnBus(new UIntPtr(0), 1024, recordingFormat,
            (buffer, _) => { liveSpeechRequest.Append(buffer); });

        audioEngine.Prepare();
        audioEngine.StartAndReturnError(out var error);

        if (error is not null)
        {
            throw new ArgumentException("Error starting audio engine - " + error.LocalizedDescription);
        }

        var currentIndex = 0;
        var taskResult = new TaskCompletionSource<string>();
        recognitionTask = speechRecognizer.GetRecognitionTask(liveSpeechRequest, (result, err) =>
        {
            if (err != null)
            {
                StopRecording();
                taskResult.TrySetException(new Exception(err.LocalizedDescription));
            }
            else
            {
                if (result.Final)
                {
                    currentIndex = 0;
                    StopRecording();
                    taskResult.TrySetResult(result.BestTranscription.FormattedString);
                }
                else
                {
                    for (var i = currentIndex; i < result.BestTranscription.Segments.Length; i++)
                    {
                        var s = result.BestTranscription.Segments[i].Substring;
                        currentIndex++;
                        recognitionResult?.Report(s);
                    }
                }
            }
        });

        await using (cancellationToken.Register(() =>
                     {
                         StopRecording();
                         taskResult.TrySetCanceled();
                     }))
        {
            try
            {
                return await taskResult.Task;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return string.Empty;
        }
    }

    public void StopRecording()
    {
        audioEngine?.InputNode.RemoveTapOnBus(new UIntPtr(0));
        audioEngine?.Stop();
        liveSpeechRequest?.EndAudio();
        recognitionTask?.Cancel();
    }

    public ValueTask DisposeAsync()
    {
        audioEngine?.Dispose();
        speechRecognizer?.Dispose();
        liveSpeechRequest?.Dispose();
        recognitionTask?.Dispose();
        return ValueTask.CompletedTask;
    }

    public Task<bool> RequestPermissions()
    {
        var taskResult = new TaskCompletionSource<bool>();
        SFSpeechRecognizer.RequestAuthorization(status =>
        {
            taskResult.SetResult(status == SFSpeechRecognizerAuthorizationStatus.Authorized);
        });

        return taskResult.Task;
    }
}