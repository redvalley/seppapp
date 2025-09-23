using System.Diagnostics;
using CommunityToolkit.Maui.Media;
using Plugin.Maui.Audio;
using System.Globalization;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Extensions;
using RedValley.Extensions;

namespace SeppApp;

public partial class GameMaibaumKraxelnPage : ContentPage
{
    private IAudioPlayer _audioPlayerSayingOachkatzlSchwoaf;
    private IAudioPlayer _audioPlayerGameResultSuper;
    private IAudioPlayer _audioPlayerGameResultGood;
    private IAudioPlayer _audioPlayerGameResultSolid;
    private IAudioPlayer _audioPlayerGameResultBad;
    private IAudioPlayer _audioPlayerCoins;
    private IAudioPlayer _audioBackground;
    private IDispatcherTimer _gameTimer;

    readonly IAudioManager _audioManager;
    readonly ISpeechToText _speechToText;
    private bool _isOachKatzlSchwoafRecognized;
    private int _currentKraxelImageNo;
    private string _kraxelImagePrefix = "maibaum_kraxeln_";
    private int _currentKraxelImageBottomMargin;
    private const string LeftTap = "LeftTap";
    private const string RightTap = "RightTap";
    private string? _currentTapPosition;
    private TimeSpan _currentGameTime;
    private bool _isGameRunning;

    public GameMaibaumKraxelnPage(IAudioManager audioManager)
    {
        InitializeComponent();
        _audioManager = audioManager;

        _audioPlayerGameResultSuper = AudioHelper.CreateAudioPlayer(_audioManager, "game_applause_super.mp3");
        _audioPlayerGameResultGood = AudioHelper.CreateAudioPlayer(_audioManager, "game_applause_good.mp3");
        _audioPlayerGameResultSolid = AudioHelper.CreateAudioPlayer(_audioManager, "game_applause_solid.MP3");
        _audioPlayerGameResultBad = AudioHelper.CreateAudioPlayer(_audioManager, "game_applause_fail.mp3");
        _audioPlayerCoins = AudioHelper.CreateAudioPlayer(_audioManager, "coins.mp3");
        _audioBackground = AudioHelper.CreateAudioPlayer(_audioManager, "background_sound_oachkatzlschwoaf.mp3");
        FadeInBorder.IsVisible = true;
        NavigationPage.SetHasNavigationBar(this, false);
        _gameTimer = Dispatcher.CreateTimer();
        _gameTimer.Tick += GameTimerTick;
        _currentGameTime = TimeSpan.Zero;
        _gameTimer.Interval = TimeSpan.FromSeconds(1);
    }

    private void GameTimerTick(object? sender, EventArgs e)
    {
        _currentGameTime = _currentGameTime.Add(TimeSpan.FromSeconds(1));
        StopWatchLabel.Text = _currentGameTime.ToString("g");
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await FadeInScene();
        _currentKraxelImageNo = 1;
        _currentKraxelImageBottomMargin = 0;
        _audioBackground.Loop = true;
        _audioBackground.Play();
        var userSettings = AppUserSettings.Load();
        this.CoinLabel.Text = userSettings.Coins.ToString();
    }



    public void StartGame()
    {
        if (_isGameRunning)
        {
            return;
        }
        _isGameRunning = true;
        _audioBackground.Stop();
        GameHintBorder.IsVisible = false;

        _currentGameTime = TimeSpan.Zero;
        _gameTimer.Start();
        _currentKraxelImageNo = 0;
        KraxelImage.Source = ImageSource.FromFile(_kraxelImagePrefix + _currentKraxelImageNo);
        _currentKraxelImageBottomMargin = 0;
        KraxelImage.Margin = Thickness.Zero;
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

    private void LeftTappedKraxelnButton_OnClicked(object? sender, EventArgs e)
    {
        StartGame();

        Kraxeln(LeftTap);

        CheckGameSuccess();
    }

    private void CheckGameSuccess()
    {
        var backgroundImageHeight = BackgroundImage.Height;
        
        if (_currentKraxelImageBottomMargin >= backgroundImageHeight - 125)
        {
            GameHintBorder.IsVisible = true;
            _gameTimer.Stop();
            _isGameRunning = false;

            if (_currentGameTime.TotalSeconds <= 60)
            {
                GameHintLabel.Text = Properties.Resources.LabelGameHintMaibaumKraxelnSuper;
                KraxelImage.Source = ImageSource.FromFile("sepp_happy");
                _audioPlayerGameResultSuper.Play();

            } else if (_currentGameTime.TotalSeconds <= 120)
            {
                GameHintLabel.Text = Properties.Resources.LabelGameHintMaibaumKraxelnOk;
                KraxelImage.Source = ImageSource.FromFile("sepp_1");
                _audioPlayerGameResultGood.Play();
            }
            else
            {
                GameHintLabel.Text = Properties.Resources.LabelGameHintMaibaumKraxelnBad;
                KraxelImage.Source = ImageSource.FromFile("sepp_grantig");
                _audioPlayerGameResultSolid.Play();
            }


        }
    }

    private void Kraxeln(string tapPosition)
    {


        if (_currentTapPosition != null && _currentTapPosition == tapPosition)
        {
            return;
        }

        

        if (_currentKraxelImageNo == 3)
        {
            _currentKraxelImageNo = 1;
        }
        else
        {
            _currentKraxelImageNo++;
        }

        _currentKraxelImageBottomMargin += 5;

        KraxelImage.Source = ImageSource.FromFile(_kraxelImagePrefix + _currentKraxelImageNo);
        KraxelImage.Margin = new Thickness(0, 0, 0, _currentKraxelImageBottomMargin);
        if (tapPosition == LeftTap)
        {
            TapLeftButton.BackgroundColor = Colors.Transparent;
            TapRightButton.BackgroundColor = Color.FromRgba(0xFF, 0xFF, 0xFF, 0x80);
        }
        else
        {
            TapRightButton.BackgroundColor = Colors.Transparent;
            TapLeftButton.BackgroundColor = Color.FromRgba(0xFF, 0xFF, 0xFF, 0x80);
        }

        _currentTapPosition = tapPosition;
    }

    private void RightTappedKraxelnButton_OnClicked(object? sender, EventArgs e)
    {
        StartGame();

        Kraxeln(RightTap);

        CheckGameSuccess();
    }
}