using CommunityToolkit.Maui.Media;
using Plugin.Maui.Audio;
using System.Globalization;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Extensions;
using RedValley.Extensions;
using SeppApp.Services;

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

    readonly ISpeechToTextService _speechToTextService;
    private bool _isWordRecognized;

    private IEnumerable<string> GameWords =
    [
        "oachkatzlschwoaf"
    ];

    private Dictionary<string, IEnumerable<string>> FullyCorrectWordsDictionary = new()
    {
        {
            "oachkatzlschwoaf",
            ["oachkatzlschwoaf", "oachkatzelschwoaf", "oarchkatzelschwoaf", "ohrkatzelschworf", "ohrkatzlschworf"]
        }
    };
   
    
    private Dictionary<string, IEnumerable<IEnumerable<string>>> HalfCorrectWordsDictionary = new()
    {
        {
            "oachkatzlschwoaf",
            [["katzel", "katzl"],
             ["schwoaf", "schweif", "schworf"]
            ]
        }
    };

    private Dictionary<string, IEnumerable<string>> NearlyCorrectWordsDictionary = new()
    {
        {
            "oachkatzlschwoaf",
            ["oach", "katzl", "katzel", "schwoaf", "schweif"]
        }
    };

    public GameOachKatzlSchwoafPage(IAudioManager audioManager, ISpeechToTextService speechToTextService)
    {
        InitializeComponent();
        _speechToTextService = speechToTextService;
        _audioPlayerSayingOachkatzlSchwoaf = AudioHelper.CreateAudioPlayer(audioManager, "oachkatzlschwoaf.mp3");
        _audioPlayerGameResultSuper = AudioHelper.CreateAudioPlayer(audioManager, "game_applause_super.mp3");
        _audioPlayerGameResultGood = AudioHelper.CreateAudioPlayer(audioManager, "game_applause_good.mp3");
        _audioPlayerGameResultSolid = AudioHelper.CreateAudioPlayer(audioManager, "game_applause_solid.MP3");
        _audioPlayerGameResultBad = AudioHelper.CreateAudioPlayer(audioManager, "game_applause_fail.mp3");
        _audioPlayerCoins = AudioHelper.CreateAudioPlayer(audioManager, "coins.mp3");
        _audioBackground = AudioHelper.CreateAudioPlayer(audioManager, "background_sound_oachkatzlschwoaf.mp3");
        FadeInBorder.IsVisible = true;
        NavigationPage.SetHasNavigationBar(this, false);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await FadeInScene();
        TalkNowIcon.IsVisible = false;
        _audioBackground.Loop = true;
        _audioBackground.Volume = 0.1;
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

    private async void AudioPlayerSayingOachkatzlSchwoafOnPlaybackEnded(object? sender, EventArgs e)
    {
        await StartListening(CancellationToken.None);
        _audioPlayerSayingOachkatzlSchwoaf.PlaybackEnded -= AudioPlayerSayingOachkatzlSchwoafOnPlaybackEnded;
    }

    private async Task StartListening(CancellationToken cancellationToken)
    {
        TalkNowIcon.IsVisible = true;
            await _speechToTextService.StartListeningAsync(CancellationToken.None, recognizedText =>
            {
                RecognizeOachkatzlSchwoaf(recognizedText);
            }, async () =>
            {
                await Toast.Make(Properties.Resources.ToastErrorMicrophoneAccessMissing).Show(CancellationToken.None);
            });
        
    }



    private void RecognizeOachkatzlSchwoaf(string recognizedText)
    {
        var userSettings = AppUserSettings.Load();
        IAudioPlayer? audioPlayerGameResult = null;
        GameHintBorder.IsVisible = true;
        _isWordRecognized = true;
        TalkNowIcon.IsVisible = false;
        string gameWord = GameWords.First();

        if (recognizedText.ContainsAny(FullyCorrectWordsDictionary[gameWord]))
        {
            this.BackgroundImage.Source = ImageSource.FromFile("oachkatzl_happy.png");
            userSettings.Coins += 20;
            GameHintLabel.Text = Properties.Resources.LabelGameHintOachkatzlSchwoafTop;
            audioPlayerGameResult = _audioPlayerGameResultSuper;
            
        }
        else if (recognizedText.ContainsAnyAndCombined(HalfCorrectWordsDictionary[gameWord]))

        {
            this.BackgroundImage.Source = ImageSource.FromFile("oachkatzl_ok.png");
            userSettings.Coins += 10;
            GameHintLabel.Text = Properties.Resources.LabelGameHintOachkatzlSchwoafGood;
            audioPlayerGameResult = _audioPlayerGameResultGood;
        }
        else if (recognizedText.ContainsAny(NearlyCorrectWordsDictionary[gameWord]))
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
            _isWordRecognized = false;
        }

        userSettings.Save();
        CoinLabel.Text = userSettings.Coins.ToString();

        audioPlayerGameResult.Play();
        audioPlayerGameResult.PlaybackEnded += AudioPlayerGameResultOnPlaybackEnded;
    }

    private void AudioPlayerGameResultOnPlaybackEnded(object? sender, EventArgs e)
    {
        if (_isWordRecognized)
        {
            _audioPlayerCoins.Play();
        }
        
        this.BackgroundImage.Source = ImageSource.FromFile("background_oachkatzlschwoaf_game.png");
        _audioBackground.Play();
        _audioBackground.Volume = 0.1;
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