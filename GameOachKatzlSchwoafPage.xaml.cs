using CommunityToolkit.Maui.Media;
using Plugin.Maui.Audio;
using System.Globalization;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Extensions;
using RedValley.Extensions;

namespace SeppApp;

public partial class GameOachKatzlSchwoafPage : ContentPage
{
    private IAudioPlayer _audioPlayerSayingOachkatzlSchwoaf;
    private IAudioPlayer _audioPlayerGameResultSuper;
    private IAudioPlayer _audioPlayerGameResultGood;
    private IAudioPlayer _audioPlayerGameResultSolid;
    private IAudioPlayer _audioPlayerGameResultBad;
    private IAudioPlayer _audioPlayerCoins;
    private IAudioPlayer _audioBackground;


    readonly IAudioManager _audioManager;
    readonly ISpeechToText _speechToText;
    private bool _isOachKatzlSchwoafRecognized;

    public GameOachKatzlSchwoafPage(IAudioManager audioManager, ISpeechToText speechToText)
    {
        InitializeComponent();
        _audioManager = audioManager;
        _speechToText = speechToText;
        _audioPlayerSayingOachkatzlSchwoaf = AudioHelper.CreateAudioPlayer(_audioManager, "oachkatzlschwoaf.mp3");
        _audioPlayerGameResultSuper = AudioHelper.CreateAudioPlayer(_audioManager, "game_applause_super.mp3");
        _audioPlayerGameResultGood = AudioHelper.CreateAudioPlayer(_audioManager, "game_applause_good.mp3");
        _audioPlayerGameResultSolid = AudioHelper.CreateAudioPlayer(_audioManager, "game_applause_solid.MP3");
        _audioPlayerGameResultBad = AudioHelper.CreateAudioPlayer(_audioManager, "game_applause_fail.mp3");
        _audioPlayerCoins = AudioHelper.CreateAudioPlayer(_audioManager, "coins.mp3");
        _audioBackground = AudioHelper.CreateAudioPlayer(_audioManager, "background_sound_oachkatzlschwoaf.mp3");
        FadeInBorder.IsVisible = true;
        NavigationPage.SetHasNavigationBar(this, false);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await FadeInScene();
        _audioBackground.Loop = true;
        _audioBackground.Play();
        var userSettings = AppUserSettings.Load();
        this.CoinLabel.Text = userSettings.Coins.ToString();
    }

    private void StartGameButton_OnClicked(object? sender, EventArgs e)
    {
        StartGame();
    }

    public void StartGame()
    {
        _audioBackground.Stop();
        GameHintBorder.IsVisible = false;
        this.BackgroundImage.Source = ImageSource.FromFile("oachkatzl_listening.png");
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
        _speechToText.RecognitionResultCompleted -= OnRecognitionTextCompleted;

        var userSettings = AppUserSettings.Load();
        IAudioPlayer? audioPlayerGameResult = null;
        GameHintBorder.IsVisible = true;
        _isOachKatzlSchwoafRecognized = true;
        if (e.RecognitionResult?.Text != null &&
            e.RecognitionResult.Text.ContainsAny("oachkatzlschwoaf","oachkatzelschwoaf", "oarchkatzelschwoaf", "ohrkatzelschworf", "ohrkatzlschworf"))
        {
            this.BackgroundImage.Source = ImageSource.FromFile("oachkatzl_happy.png");
            userSettings.Coins += 20;
            GameHintLabel.Text = Properties.Resources.LabelGameHintOachkatzlSchwoafTop;
            audioPlayerGameResult = _audioPlayerGameResultSuper;
            
        }
        else if ((e.RecognitionResult?.Text.ContainsAny("katzel", "katzl")??false) &&
                 (e.RecognitionResult?.Text?.ContainsAny("schwoaf", "schweif", "schworf") ?? false)
                 )

        {
            this.BackgroundImage.Source = ImageSource.FromFile("oachkatzl_ok.png");
            userSettings.Coins += 10;
            GameHintLabel.Text = Properties.Resources.LabelGameHintOachkatzlSchwoafGood;
            audioPlayerGameResult = _audioPlayerGameResultGood;
        }
        else if (e.RecognitionResult?.Text != null &&
                 (e.RecognitionResult.Text.ContainsAny("oach", "katzl", "katzel", "schwoaf", "schweif")))
        {
            this.BackgroundImage.Source = ImageSource.FromFile("oachkatzl_ok.png");
            userSettings.Coins += 5;
            GameHintLabel.Text = Properties.Resources.LabelGameHintOachkatzlSchwoafSolid;
            audioPlayerGameResult = _audioPlayerGameResultSolid;
        }
        else

        {
            this.BackgroundImage.Source = ImageSource.FromFile("oachkatzl_confused.png");
            GameHintLabel.Text = Properties.Resources.LabelGameHintOachkatzlSchwoafBad;
            audioPlayerGameResult = _audioPlayerGameResultBad;
            _isOachKatzlSchwoafRecognized = false;
        }

        userSettings.Save();
        CoinLabel.Text = userSettings.Coins.ToString();

        audioPlayerGameResult.Play();
        audioPlayerGameResult.PlaybackEnded += AudioPlayerGameResultOnPlaybackEnded;
    }

    private void AudioPlayerGameResultOnPlaybackEnded(object? sender, EventArgs e)
    {
        if (_isOachKatzlSchwoafRecognized)
        {
            _audioPlayerCoins.Play();
        }
        
        this.BackgroundImage.Source = ImageSource.FromFile("background_oachkatzlschwoaf_game.png");
        _audioBackground.Play();
    }

    private void HomeButton_OnClicked(object? sender, EventArgs e)
    {
        _audioBackground.Stop();
        this.Navigation.PopAsync(true);
    }

    private async Task FadeInScene()
    {
        FadeInBorder.IsVisible = true;
        FadeInBorder.BackgroundColor = Colors.Black;
        await FadeInBorder.BackgroundColorTo(Colors.Transparent, length: 5000);
        FadeInBorder.IsVisible = false;
    }
}