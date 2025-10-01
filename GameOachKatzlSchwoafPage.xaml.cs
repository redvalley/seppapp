using CommunityToolkit.Maui.Media;
using Plugin.Maui.Audio;
using System.Globalization;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Extensions;
using RedValley;
using RedValley.Extensions;
using RedValley.Helper;
using SeppApp.Helper;
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
    private bool _isListening = false;

    private IEnumerable<string> GameWords =
    [
        "oachkatzlschwoaf"
    ];

    private Dictionary<string, IEnumerable<string>> FullyCorrectWordsDictionary = new()
    {
        {
            "oachkatzlschwoaf",
            [
                "oachkatzlschwoaf", "oachkatzelschwoaf", "oarchkatzelschwoaf", "ohrkatzelschworf", "ohrkatzlschworf",
                "achkatzlschwoaf"
            ]
        }
    };


    private Dictionary<string, IEnumerable<IEnumerable<string>>> HalfCorrectWordsDictionary = new()
    {
        {
            "oachkatzlschwoaf",
            [
                ["oach", "ach", "katzl", "katzel", "katzen", "schwoaf", "schweif", "schworf", "schorf"],
                ["oach", "ach", "katzl", "katzel", "katzen", "schwoaf", "schweif", "schworf", "schorf"]
            ]
        }
    };

    private Dictionary<string, IEnumerable<string>> NearlyCorrectWordsDictionary = new()
    {
        {
            "oachkatzlschwoaf",
            ["oach", "ach", "katzl", "katzel", "katzen", "schwoaf", "schweif", "schworf", "schorf"]
        }
    };

    private Task _lmGenerationTask;
    private IAudioPlayer? _audioPlayerGameResult;

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

        _lmGenerationTask = Task.Run(async () => { await _speechToTextService.Initialize(); });
        
        var userSettings = AppUserSettings.Load();
        InitializeGame(userSettings);

        await FadeInScene();
    }

    private void InitializeGame(AppUserSettings userSettings)
    {
        GameHintBorder.IsVisible = true;
        TalkNowBorder.IsVisible = false;
        this.CoinLabel.Text = userSettings.Coins.ToString();
        this.BackgroundImage.Source = ImageSource.FromFile("background_oachkatzlschwoaf_game.png");
        PlayAudioBackground();
    }

    private void PlayAudioBackground()
    {
        _audioBackground.Loop = true;
        _audioBackground.Volume = 0.1;
        _audioBackground.Play();
    }

    private void StartGameButton_OnClicked(object? sender, EventArgs e)
    {
        StartGame();
    }

    public void StartGame()
    {
        if (_isListening)
        {
            return;
        }
        _isListening = true;

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
        await ExceptionHelper.TryAsync(this.GetType().Name + " " + nameof(StartListening), async () =>
        {
            this.BackgroundImage.Source = ImageSource.FromFile("oachkatzlschwoaf_sepp_horcht.png");
            TalkNowBorder.IsVisible = true;

            await _speechToTextService.StartListeningAsync(CancellationToken.None, recognizedText =>
                {
                    RecognizeOachkatzlSchwoaf(recognizedText);
                    _isListening = false;
                },
                async () =>
                {
                    await Toast.Make(Properties.Resources.ToastErrorMicrophoneAccessMissing)
                        .Show(CancellationToken.None);
                }, maxListenTimeMilliSeconds: SpeechToTextService.DefaultMaxListenTimeMilliSeconds);
        }, Logging.CreateGameLogger());


    }


    private void RecognizeOachkatzlSchwoaf(string recognizedText)
    {
        ExceptionHelper.Try(this.GetType().Name + " " + nameof(RecognizeOachkatzlSchwoaf), () =>
        {
            var userSettings = AppUserSettings.Load();
            _isWordRecognized = true;
            TalkNowBorder.IsVisible = false;
            string gameWord = GameWords.First();

            if (recognizedText.ContainsAny(FullyCorrectWordsDictionary[gameWord]))
            {
                this.BackgroundImage.Source = ImageSource.FromFile("oachkatzl_happy.png");
                userSettings.Coins += 20;
                GameHintLabel.Text = Properties.Resources.LabelGameHintOachkatzlSchwoafTop;
                _audioPlayerGameResult = _audioPlayerGameResultSuper;
            }
            else if (recognizedText.ContainsAnyAndCombined(HalfCorrectWordsDictionary[gameWord]))

            {
                this.BackgroundImage.Source = ImageSource.FromFile("oachkatzl_ok.png");
                userSettings.Coins += 10;
                GameHintLabel.Text = Properties.Resources.LabelGameHintOachkatzlSchwoafGood;
                _audioPlayerGameResult = _audioPlayerGameResultGood;
            }
            else if (recognizedText.ContainsAny(NearlyCorrectWordsDictionary[gameWord]))
            {
                this.BackgroundImage.Source = ImageSource.FromFile("oachkatzl_ok.png");
                userSettings.Coins += 5;
                GameHintLabel.Text = Properties.Resources.LabelGameHintOachkatzlSchwoafSolid;
                _audioPlayerGameResult = _audioPlayerGameResultSolid;
            }
            else
            {
                this.BackgroundImage.Source = ImageSource.FromFile("oachkatzl_confused.png");
                GameHintLabel.Text = Properties.Resources.LabelGameHintOachkatzlSchwoafBad;
                _audioPlayerGameResult = _audioPlayerGameResultBad;
                _isWordRecognized = false;
            }

            GameHintBorder.IsVisible = true;
            userSettings.Save();
 
            _audioPlayerGameResult.Play();
            _audioPlayerGameResult.PlaybackEnded += AudioPlayerGameResultOnPlaybackEnded;
        }, Logging.CreateGameLogger());
    }

    private void AudioPlayerGameResultOnPlaybackEnded(object? sender, EventArgs e)
    {
        if (_audioPlayerGameResult != null)
        {
            _audioPlayerGameResult.PlaybackEnded -= AudioPlayerGameResultOnPlaybackEnded;
        }

        if (_isWordRecognized)
        {
            _audioPlayerCoins.Play();
        }

        var userSettings = AppUserSettings.Load();
        InitializeGame(userSettings);
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