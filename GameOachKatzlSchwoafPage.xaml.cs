using CommunityToolkit.Maui.Media;
using Plugin.Maui.Audio;
using System.Globalization;
using CommunityToolkit.Maui.Alerts;

namespace SeppApp;

public partial class GameOachKatzlSchwoafPage : ContentPage
{
    private IAudioPlayer _audioPlayerSayingOachkatzlSchwoaf;
    readonly IAudioManager _audioManager;
    readonly ISpeechToText _speechToText;

    public GameOachKatzlSchwoafPage(IAudioManager audioManager, ISpeechToText speechToText)
    {
        InitializeComponent();
        _audioManager = audioManager;
        _speechToText = speechToText;
        _audioPlayerSayingOachkatzlSchwoaf = AudioHelper.CreateAudioPlayer(_audioManager, "oachkatzlschwoaf.mp3");
    }

    private void StartGameButton_OnClicked(object? sender, EventArgs e)
    {
        StartGame();
    }

    public void StartGame()
    {
        _audioPlayerSayingOachkatzlSchwoaf.Play();
        _audioPlayerSayingOachkatzlSchwoaf.PlaybackEnded += AudioPlayerSayingOachkatzlSchwoafOnPlaybackEnded;
    }

    private void AudioPlayerSayingOachkatzlSchwoafOnPlaybackEnded(object? sender, EventArgs e)
    {
        StartListening(CancellationToken.None);
        _audioPlayerSayingOachkatzlSchwoaf.PlaybackEnded -= AudioPlayerSayingOachkatzlSchwoafOnPlaybackEnded;
    }

    async Task StartListening(CancellationToken cancellationToken)
    {
        this.BackgroundImage.Source = ImageSource.FromFile("oachkatzl_listening.png");

        var isGranted = await _speechToText.RequestPermissions(cancellationToken);
        if (!isGranted)
        {
            await Toast.Make(Properties.Resources.ToastErrorMicrophoneAccessMissing).Show(CancellationToken.None);
            return;
        }


        _speechToText.RecognitionResultCompleted += OnRecognitionTextCompleted;
        await _speechToText.StartListenAsync(new SpeechToTextOptions { Culture = CultureInfo.CurrentCulture, ShouldReportPartialResults = true }, CancellationToken.None);

    }

    private void OnRecognitionTextCompleted(object? sender, SpeechToTextRecognitionResultCompletedEventArgs e)
    {
        if (e.RecognitionResult?.Text != null &&
            (e.RecognitionResult.Text.Contains("oachkatzlschwoaf", StringComparison.InvariantCultureIgnoreCase)))
        {
            this.BackgroundImage.Source = ImageSource.FromFile("oachkatzl_happy.png");
            var userSettings = AppUserSettings.Load();
            userSettings.Coins += 20;
            userSettings.Save();

        }
        else if (e.RecognitionResult?.Text != null &&
                   (e.RecognitionResult.Text.Contains("oach", StringComparison.InvariantCultureIgnoreCase) &&
                    e.RecognitionResult.Text.Contains("katzl", StringComparison.InvariantCultureIgnoreCase)))

        {
            this.BackgroundImage.Source = ImageSource.FromFile("oachkatzl_ok.png");
            var userSettings = AppUserSettings.Load();
            userSettings.Coins += 10;
            userSettings.Save();
        }
        else if (e.RecognitionResult?.Text != null &&
                 (e.RecognitionResult.Text.Contains("oach", StringComparison.InvariantCultureIgnoreCase) ||
                  e.RecognitionResult.Text.Contains("katzl", StringComparison.InvariantCultureIgnoreCase)))
        {
            this.BackgroundImage.Source = ImageSource.FromFile("oachkatzl_ok.png");
            var userSettings = AppUserSettings.Load();
            userSettings.Coins += 5;
            userSettings.Save();
        }
        else
        {
            this.BackgroundImage.Source = ImageSource.FromFile("oachkatzl_confused.png");
        }
    }
}