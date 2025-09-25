using System.Diagnostics;
using CommunityToolkit.Maui.Media;
using Plugin.Maui.Audio;
using System.Globalization;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Extensions;
using RedValley.Extensions;
using SeppApp.Models;

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
    private bool _isGameSuccessReached = false;
    private GameSuccessGrade _currentGameSuccessGrade = GameSuccessGrade.None;
    
    public GameMaibaumKraxelnPage(IAudioManager audioManager)
    {
        InitializeComponent();
        _audioManager = audioManager;

        _audioPlayerGameResultSuper = AudioHelper.CreateAudioPlayer(_audioManager, "game_applause_super.mp3");
        _audioPlayerGameResultGood = AudioHelper.CreateAudioPlayer(_audioManager, "game_applause_good.mp3");
        _audioPlayerGameResultSolid = AudioHelper.CreateAudioPlayer(_audioManager, "game_applause_solid.MP3");
        _audioPlayerGameResultBad = AudioHelper.CreateAudioPlayer(_audioManager, "game_applause_fail.mp3");
        _audioPlayerCoins = AudioHelper.CreateAudioPlayer(_audioManager, "coins.mp3");
        _audioBackground = AudioHelper.CreateAudioPlayer(_audioManager, "background_sound_maibaum.mp3");
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
        StartBackgroundSound();
        var userSettings = AppUserSettings.Load();
        this.CoinLabel.Text = userSettings.Coins.ToString();
    }

    private void StartBackgroundSound()
    {
        _audioBackground.Loop = true;
        _audioBackground.Volume = 0.1;
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

    private async void LeftTappedKraxelnButton_OnClicked(object? sender, EventArgs e)
    {
        if (_isGameSuccessReached)
        {
            return;
        }

        StartGame();

        Kraxeln(LeftTap);

        await CheckGameSuccess();
    }
    
    private async void RightTappedKraxelnButton_OnClicked(object? sender, EventArgs e)
    {
        if (_isGameSuccessReached)
        {
            return;
        }

        StartGame();

        Kraxeln(RightTap);

        await CheckGameSuccess();
    }
    
    public void StartGame()
    {
        if (_isGameRunning)
        {
            return;
        }
        _currentGameSuccessGrade = GameSuccessGrade.None;
        _isGameRunning = true;
        GameHintBorder.IsVisible = false;
        StartBackgroundSound();
        _currentGameTime = TimeSpan.Zero;
        _gameTimer.Start();
        _currentKraxelImageNo = 0;
        KraxelImage.Source = ImageSource.FromFile(_kraxelImagePrefix + 1);
        KraxelImage.WidthRequest = 80;
        KraxelImage.HeightRequest = 100;
        _currentKraxelImageBottomMargin = 50;
        KraxelImage.Margin = Thickness.Zero;
    }

    private async Task CheckGameSuccess()
    {
        var backgroundImageHeight = BackgroundImage.Height;

        if (_currentKraxelImageBottomMargin >= backgroundImageHeight - 150)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _isGameSuccessReached = true;
                GameHintBorder.IsVisible = true;
                _gameTimer.Stop();
                _audioBackground.Stop();
                var userSettings = AppUserSettings.Load();
                
                if (_currentGameTime.TotalSeconds <= 20)
                {
                    _currentGameSuccessGrade = GameSuccessGrade.Top;
                    GameHintLabel.Text = Properties.Resources.LabelGameHintMaibaumKraxelnSuper;
                    userSettings.Coins += 20;
       
                    _audioPlayerGameResultSuper.Play();
                    _audioPlayerGameResultSuper.PlaybackEnded += AudioPlayerGameResultSuperOnPlaybackEnded;
                } else if (_currentGameTime.TotalSeconds <= 30)
                {
                    _currentGameSuccessGrade = GameSuccessGrade.Good;
                    GameHintLabel.Text = Properties.Resources.LabelGameHintMaibaumKraxelnOk;
                    userSettings.Coins += 10;
                    
                    _audioPlayerGameResultGood.Play();
                    _audioPlayerGameResultGood.PlaybackEnded += AudioPlayerGameResultGoodOnPlaybackEnded;
                }
                else
                {
                    _currentGameSuccessGrade = GameSuccessGrade.Solid;
                    GameHintLabel.Text = Properties.Resources.LabelGameHintMaibaumKraxelnBad;
                    userSettings.Coins += 5;
                    
                    _audioPlayerGameResultSolid.Play();
                    _audioPlayerGameResultSolid.PlaybackEnded += AudioPlayerGameResultSolidOnPlaybackEnded;
                }

                userSettings.Save();
            });
            
            await Task.Delay(3000);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _currentKraxelImageBottomMargin = 50;
                KraxelImage.Margin = new Thickness(- 100, 0, 0, _currentKraxelImageBottomMargin);
                KraxelImage.WidthRequest = 160;
                KraxelImage.HeightRequest = 200;
                
                
                switch (_currentGameSuccessGrade)
                {
                    case GameSuccessGrade.Top:
                        KraxelImage.Source = ImageSource.FromFile("sepp_happy.png");
                        break;
                    case GameSuccessGrade.Good:
                        KraxelImage.Source = ImageSource.FromFile("sepp_transparent_1.png");
                        break;
                    default:
                        KraxelImage.Source = ImageSource.FromFile("sepp_grantig.png");
                        break;
                }
            });
            
            
            _isGameRunning = false;
            _isGameSuccessReached = false;
        }
    }

    private void AudioPlayerGameResultSolidOnPlaybackEnded(object? sender, EventArgs e)
    {
        _audioPlayerGameResultSolid.PlaybackEnded -= AudioPlayerGameResultSolidOnPlaybackEnded;
        _audioPlayerCoins.Play();
        UpdateCoinLabel();
    }
    
    private void AudioPlayerGameResultGoodOnPlaybackEnded(object? sender, EventArgs e)
    {
        _audioPlayerGameResultGood.PlaybackEnded -= AudioPlayerGameResultGoodOnPlaybackEnded;
        _audioPlayerCoins.Play();
        UpdateCoinLabel();
    }

    private void AudioPlayerGameResultSuperOnPlaybackEnded(object? sender, EventArgs e)
    {
        _audioPlayerGameResultSuper.PlaybackEnded -= AudioPlayerGameResultSuperOnPlaybackEnded;
        _audioPlayerCoins.Play();
        UpdateCoinLabel();
    }
    
    private void UpdateCoinLabel()
    {
        var userSettings = AppUserSettings.Load();
        CoinLabel.Text = userSettings.Coins.ToString();
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
}