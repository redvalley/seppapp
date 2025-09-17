using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Media;
using Plugin.Maui.Audio;
using RedValley;
using RedValley.Helper;
using RedValley.Mobile.Services;
using SeppApp.Models;
using System.Globalization;
using TalkingSepp.Models;

namespace SeppApp
{
    public partial class MainPage : ContentPage
    {
        private readonly ISpeechToText _speechToText;
        private readonly IAudioManager _audioManager;
        private readonly IRedValleyInterstitualAdService _redValleyInterstitualAdService;

        private IDispatcherTimer _characterAnimationTimer;
        private bool _isFirstTimeOpened = true;
        private bool _characterIsBusy = false;
        private IDispatcherTimer _heartMinusDispatcher;
        public bool IsInterstitualAdShowing { get; set; }

        private List<string> _characterAnimationTalkingImageList =
        [
            "sepp_transparent_1",
            "sepp_transparent_2",
            "sepp_transparent_3",
            "sepp_transparent_2",
        ];

        private List<string> _characterAnimationDrinkingImageList =
        [
            "drinking_animation_1",
            "drinking_animation_2"
        ];

        private List<string> _characterAnimationEatingImageList =
        [
            "eating_animation_1",
            "eating_animation_2"
        ];

        private List<string> _characterAnimationSnoringImageList =
        [
            "sepp_schnarchend_1",
            "sepp_schnarchend_2",
            "sepp_schnarchend_3",
            "sepp_schnarchend_1",
        ];

        private List<string> _currentcharacterAnimationImageList = new List<string>();

        private int _currentAnimationImageIndex = 0;
        private IAudioPlayer _audioPlayerPfuiDeifeSound;
        private IAudioPlayer _audioPlayerIntroSound;
        private IAudioPlayer _audioPlayerSchnarchen;
        private IAudioPlayer _audioPlayerThirsty;
        private IAudioPlayer _audioPlayerHungry;
        private IAudioPlayer _audioPlayerBored;
        private IAudioPlayer _audioPlayerNo;
        private IAudioPlayer _audioPlayerThanks;
        private IAudioPlayer _audioBackground;
        private IAudioPlayer _audioPlayerDrinking;
        private IAudioPlayer _audioPlayerEating;

        private List<IAudioPlayer> _audioPlayerGreeting = new List<IAudioPlayer>();
        private List<IAudioPlayer> _audioPlayerNotUnderstand = new List<IAudioPlayer>();
        private List<IAudioPlayer> _audioPlayerHowAreYouAnswer = new List<IAudioPlayer>();

        private List<IAudioPlayer> _audioPlayerTalking = new List<IAudioPlayer>();


        private List<CharacterFeelings> _characterFeelings = [CharacterFeelings.Hungry, CharacterFeelings.Thirsty, CharacterFeelings.Bored];

        private DebounceDispatcher _characterFeelingsDebounceDispatcher = new DebounceDispatcher(30000);
        private CharacterFeelings _characterFeeling = CharacterFeelings.None;
        private CharacterStates _characterState = CharacterStates.Awake;

        private readonly Random _random = new();


        private const long DefaultAnimiationInterval = 150;
        private const long SnoringAnimiationInterval = 2000;
        private const long DrinkingEatingAnimiationInterval = 1000;

        public MainPage(ISpeechToText speechToText, IAudioManager audioManager, IRedValleyInterstitualAdService redValleyInterstitualAdService)
        {

            _speechToText = speechToText;
            _audioManager = audioManager;
            _redValleyInterstitualAdService = redValleyInterstitualAdService;

            InitializeComponent();

            _heartMinusDispatcher = Dispatcher.CreateTimer();
            _heartMinusDispatcher.Interval = TimeSpan.FromSeconds(1);
            _heartMinusDispatcher.Tick += HeartMinusDispatcherOnTick;

            _characterAnimationTimer = Dispatcher.CreateTimer();
            _characterAnimationTimer.Tick += CharacterAnimationTimerOnTick;
            _characterAnimationTimer.Interval = TimeSpan.FromMilliseconds(DefaultAnimiationInterval);
            NavigationPage.SetHasNavigationBar(this, false);
            InitializeSound();

        }

        private void ChangeFeeling()
        {
            _characterFeeling = GetRandomEntry(_characterFeelings);
            switch (_characterFeeling)
            {
                case CharacterFeelings.Thirsty:
                    FeelingBubbleImage.Source = ImageSource.FromFile("feeling_bubble_limo.png");
                    LetSeppSpeak(_audioPlayerThirsty);
                    break;
                case CharacterFeelings.Hungry:
                    FeelingBubbleImage.Source = ImageSource.FromFile("feeling_bubble_leberkaesesemmel.png");
                    LetSeppSpeak(_audioPlayerHungry);
                    break;
                case CharacterFeelings.Bored:
                    FeelingBubbleImage.Source = ImageSource.FromFile("feeling_bubble_bored.png");
                    LetSeppSpeak(_audioPlayerBored);
                    break;
            }
        }

        private void HeartMinusDispatcherOnTick(object? sender, EventArgs e)
        {
            CharacterHeart.Opacity -= 0.01666666;
            CharacterHeart.HeightRequest -= 0.8333333;
            if (CharacterHeart.Opacity <= 0)
            {
                _heartMinusDispatcher.Stop();
                LetSeppSleep();
            }
        }


        public TEntry GetRandomEntry<TEntry>(IEnumerable<TEntry> listEntries)
        {
            var list = listEntries.ToList();

            return list[_random.Next(list.Count)];
        }

        private void InitializeSound()
        {
            _audioPlayerGreeting.Add(CreateAudioPlayer("servus.mp3"));
            _audioPlayerGreeting.Add(CreateAudioPlayer("hawedere.mp3"));

            _audioPlayerNotUnderstand.Add(CreateAudioPlayer("ha.mp3"));
            _audioPlayerNotUnderstand.Add(CreateAudioPlayer("woos.mp3"));
            _audioPlayerNotUnderstand.Add(CreateAudioPlayer("iverstehnix.mp3"));

            _audioPlayerPfuiDeifeSound = CreateAudioPlayer("pfuideife.mp3");
            _audioPlayerIntroSound = CreateAudioPlayer("intro.mp3");

            _audioPlayerSchnarchen = CreateAudioPlayer("schnarchen.mp3");


            _audioPlayerEating = CreateAudioPlayer("schmatzen.mp3");
            _audioBackground = CreateAudioPlayer("background_sound_wiese.mp3");

            _audioPlayerThirsty = CreateAudioPlayer("ihobdurscht.mp3");
            _audioPlayerHungry = CreateAudioPlayer("ihobhunger.mp3");
            _audioPlayerBored = CreateAudioPlayer("mirissolangweilig.mp3");

            _audioPlayerNo = CreateAudioPlayer("naaa.mp3");
            _audioPlayerThanks = CreateAudioPlayer("merce.mp3");

            _audioPlayerHowAreYouAnswer.Add(CreateAudioPlayer("bastscho.mp3"));
            _audioPlayerHowAreYouAnswer.Add(CreateAudioPlayer("guadunssaiba.mp3"));

            _audioPlayerTalking.Add(CreateAudioPlayer("gschmeidigbleim.mp3"));
            _audioPlayerTalking.Add(CreateAudioPlayer("desbasdscho.mp3"));
            _audioPlayerTalking.Add(CreateAudioPlayer("bluadszacke.mp3"));
            _audioPlayerTalking.Add(CreateAudioPlayer("birnbacherl.mp3"));


            _audioPlayerDrinking = CreateAudioPlayer("drinking.mp3");
        }

        private IAudioPlayer CreateAudioPlayer(string soundFile)
        {
            return _audioManager.CreatePlayer(FileSystem.OpenAppPackageFileAsync(soundFile).Result);
        }

        private async void CharacterAnimationTimerOnTick(object? sender, EventArgs e)
        {
            if (_currentAnimationImageIndex == _currentcharacterAnimationImageList.Count - 1)
            {
                _currentAnimationImageIndex = 0;
            }
            else
            {
                _currentAnimationImageIndex++;
                if (_currentAnimationImageIndex > _currentcharacterAnimationImageList.Count - 1)
                {
                    _currentAnimationImageIndex--;
                }
            }
            CharacterImage.Source = ImageSource.FromFile(_currentcharacterAnimationImageList[_currentAnimationImageIndex]);
        }

        private async void CharacterButton_OnClicked(object? sender, EventArgs e)
        {
            if (_characterState == CharacterStates.Awake)
            {
                CharacterHeartRegenerate();
            }
            StopCharacterSleeping();
            DebounceCharacterFeelings();
            LetSeppSpeak(
                GetRandomEntry(_audioPlayerTalking));
            DebounceCharacterFeelings();

        }

        async Task StartListening(CancellationToken cancellationToken)
        {
            _speechToText.RecognitionResultCompleted += OnRecognitionTextCompleted;
            var isGranted = await _speechToText.RequestPermissions(cancellationToken);
            if (!isGranted)
            {
                await Toast.Make(Properties.Resources.ToastErrorMicrophoneAccessMissing).Show(CancellationToken.None);
                return;
            }

            
            await _speechToText.StartListenAsync(new SpeechToTextOptions { Culture = CultureInfo.CurrentCulture, ShouldReportPartialResults = true }, CancellationToken.None);

        }

        private async void OnRecognitionTextCompleted(object? sender, SpeechToTextRecognitionResultCompletedEventArgs e)
        {
            _speechToText.RecognitionResultCompleted -= OnRecognitionTextCompleted;
            if (_characterIsBusy)
            {
                return;
            }

            RecognizeTextAndReplay(e);
        }

        private void RecognizeTextAndReplay(SpeechToTextRecognitionResultCompletedEventArgs e)
        {
            DebounceCharacterFeelings();


            if (e.RecognitionResult?.Text != null &&
                (e.RecognitionResult.Text.Contains("hallo", StringComparison.InvariantCultureIgnoreCase) ||
                 e.RecognitionResult.Text.Contains("servus", StringComparison.InvariantCultureIgnoreCase)))
            {
                CharacterHeartRegenerate();
                LetSeppSpeak(
                    GetRandomEntry(_audioPlayerGreeting));
            }
            else if (e.RecognitionResult?.Text != null &&
                       (e.RecognitionResult.Text.Contains("wie geht es dir", StringComparison.InvariantCultureIgnoreCase) ||
                        e.RecognitionResult.Text.Contains("wie geht's dir", StringComparison.InvariantCultureIgnoreCase) ||
                        e.RecognitionResult.Text.Contains("how are you", StringComparison.InvariantCultureIgnoreCase) ||
                        e.RecognitionResult.Text.Contains("mir geht's da", StringComparison.InvariantCultureIgnoreCase) ||
                        e.RecognitionResult.Text.Contains("wia geht's da", StringComparison.InvariantCultureIgnoreCase) ||
                        e.RecognitionResult.Text.Contains("wie geht's da", StringComparison.InvariantCultureIgnoreCase)
                        )
                       )
            {
                CharacterHeartRegenerate();
                LetSeppSpeak(
                    GetRandomEntry(_audioPlayerHowAreYouAnswer));
            }
            else
            {
                CharacterHeartRegenerate();
                LetSeppSpeak(
                    GetRandomEntry(_audioPlayerNotUnderstand));

            }
        }


        private void LetSeppSpeak(IAudioPlayer player)
        {
            LetSeppDoSomething(player, _characterAnimationTalkingImageList);
        }

        private void LetSeppDrink(IAudioPlayer player)
        {
            _characterAnimationTimer.Interval = TimeSpan.FromMilliseconds(DrinkingEatingAnimiationInterval);
            LetSeppDoSomething(player, _characterAnimationDrinkingImageList);
        }

        private void LetSeppEat(IAudioPlayer player)
        {
            _characterAnimationTimer.Interval = TimeSpan.FromMilliseconds(DrinkingEatingAnimiationInterval);
            LetSeppDoSomething(player, _characterAnimationEatingImageList);
        }

        private void LetSeppSnore()
        {
            _audioPlayerSchnarchen.Loop = true;
            _characterAnimationTimer.Interval = TimeSpan.FromMilliseconds(SnoringAnimiationInterval);
            LetSeppDoSomething(_audioPlayerSchnarchen, _characterAnimationSnoringImageList);
        }

        private void LetSeppDoSomething(IAudioPlayer player, List<string> imageList)
        {
            _characterIsBusy = true;
            player.PlaybackEnded += LetSeppDoSomethingPlayerOnPlaybackEnded;
            player.Play();
            _currentcharacterAnimationImageList = imageList;
            _characterAnimationTimer.Start();
        }

        private void LetSeppDoSomethingPlayerOnPlaybackEnded(object? sender, EventArgs e)
        {
            if (sender is IAudioPlayer currentAudioPlayer)
            {
                currentAudioPlayer.PlaybackEnded -= LetSeppDoSomethingPlayerOnPlaybackEnded;
            }
            DebounceCharacterFeelings();
            CharacterImage.Source = ImageSource.FromFile("sepp_transparent_1");
            _characterAnimationTimer.Stop();
            _characterIsBusy = false;
            _characterAnimationTimer.Interval = TimeSpan.FromMilliseconds(DefaultAnimiationInterval);
            _currentAnimationImageIndex = 0;
        }


        private async Task FadeInScene()
        {
            FadeInBorder.IsVisible = true;
            FadeInBorder.BackgroundColor = Colors.Black;
            await FadeInBorder.BackgroundColorTo(Colors.Transparent, length: 5000);
            FadeInBorder.IsVisible = false;
        }

        private async Task FadeOutScene()
        {
            FadeInBorder.IsVisible = true;
            FadeInBorder.BackgroundColor = Colors.Transparent;
            await FadeInBorder.BackgroundColorTo(Colors.Black, length: 5000);
        }

        private async void LetSeppSleep()
        {
            MainThread.BeginInvokeOnMainThread(async ()
                =>
            {
                FadeInBorder.IsVisible = true;
                FadeInBorder.BackgroundColor = Colors.Transparent;
                await FadeInBorder.BackgroundColorTo(Colors.Black, length: 5000);
                LetSeppSnore();
                _characterState = CharacterStates.Sleeping;
            });

        }



        private void CharacterHeartRegenerate()
        {
            DebounceCharacterFeelings();
            CharacterHeart.Animate("CharacterHeartReset", (d) =>
            {
                if (CharacterHeart.Opacity <= 1)
                {
                    CharacterHeart.Opacity += 0.005;
                }

                if (CharacterHeart.HeightRequest <= 50)
                {
                    CharacterHeart.HeightRequest += 0.25;
                }
            }, rate: 10, length: 2000, finished: (t, d) =>
            {
                CharacterHeart.HeightRequest = 50;
                CharacterHeart.Opacity = 1;
            });
        }

        private async void WeisswurstMitKetchup_Clicked(object? sender, EventArgs e)
        {
            if (_characterIsBusy)
            {
                return;
            }

            if (BuyItem(ItemPrices.WeißwurstMitKetchupPrice))
            {
                CharacterImage.Source = ImageSource.FromFile("sepp_grantig");
                await Task.Delay(2000);
                LetSeppSpeak(_audioPlayerPfuiDeifeSound);
                _audioPlayerPfuiDeifeSound.PlaybackEnded += (o, args) =>
                {
                    CharacterImage.Source = ImageSource.FromFile("sepp_transparent_1");
                };
            }
        }

        private void Leberkaese_Clicked(object? sender, EventArgs e)
        {
            if (_characterIsBusy)
            {
                return;
            }

            if (_characterFeeling is CharacterFeelings.Hungry or CharacterFeelings.None)
            {
                if (BuyItem(ItemPrices.LeberkaeseSemmelPrice))
                {
                    StopCharacterSleeping();
                    FeelingFulFilled();
                    _audioPlayerThanks.PlaybackEnded += AudioPlayerThanksEatingOnPlaybackEnded;
                }
            }
            else
            {
                DebounceCharacterFeelings();
                LetSeppSpeak(_audioPlayerNo);
            }
        }

        private async void AudioPlayerThanksEatingOnPlaybackEnded(object? sender, EventArgs e)
        {
            _audioPlayerThanks.PlaybackEnded -= AudioPlayerThanksEatingOnPlaybackEnded;
            CharacterImage.Source = ImageSource.FromFile("eating");
            await Task.Delay(1500);

            LetSeppEat(_audioPlayerEating);
            DebounceCharacterFeelings();
        }

        private void Limo_OnClicked(object? sender, EventArgs e)
        {
            if (_characterIsBusy)
            {
                return;
            }

            if (_characterFeeling is CharacterFeelings.Thirsty or CharacterFeelings.None)
            {
                if (BuyItem(ItemPrices.LimoPrice))
                {
                    StopCharacterSleeping();
                    FeelingFulFilled();

                    _audioPlayerThanks.PlaybackEnded += AudioPlayerThanksDrinkingOnPlaybackEnded;
                }
            }
            else
            {
                DebounceCharacterFeelings();
                LetSeppSpeak(_audioPlayerNo);
            }
        }

        private async void AudioPlayerThanksDrinkingOnPlaybackEnded(object? sender, EventArgs e)
        {
            _audioPlayerThanks.PlaybackEnded -= AudioPlayerThanksDrinkingOnPlaybackEnded;
            CharacterImage.Source = ImageSource.FromFile("drinking");
            await Task.Delay(1500);

            LetSeppDrink(_audioPlayerDrinking);

            DebounceCharacterFeelings();
        }

        private bool BuyItem(int itemPrice)
        {
            AppUserSettings appUserSettings = AppUserSettings.Load();

            if (appUserSettings.Coins >= itemPrice)
            {
                appUserSettings.Coins -= itemPrice;
                ChangeBuyableItemsState(appUserSettings);
                appUserSettings.Save();
                CoinLabel.Text = appUserSettings.Coins.ToString();
                return true;
            }

            return false;
        }

        private void FeelingFulFilled()
        {
            FeelingBubbleImage.Source = string.Empty;
            _characterFeeling = CharacterFeelings.None;
            CharacterHeartRegenerate();
            _heartMinusDispatcher.Stop();
            LetSeppSpeak(_audioPlayerThanks);
        }

        private void DebounceCharacterFeelings()
        {
            _characterFeelingsDebounceDispatcher.Debounce(() =>
            {
                if (_characterIsBusy)
                {
                    DebounceCharacterFeelings();
                    return;
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (_characterFeeling == CharacterFeelings.None)
                    {
                        ChangeFeeling();
                        _heartMinusDispatcher.Start();
                    }
                });

            });
        }



        private async void FadeInBorder_OnClicked(object? sender, EventArgs e)
        {
            await FadeInScene();
            StopCharacterSleeping();
        }

        private void StopCharacterSleeping()
        {
            if (_characterState == CharacterStates.Sleeping ||
                _audioPlayerSchnarchen.IsPlaying)
            {
                _audioPlayerSchnarchen.Stop();
                _characterState = CharacterStates.Awake;
            }

        }

        private async void TalkToSepp_OnClicked(object? sender, EventArgs e)
        {
            StopCharacterSleeping();
            await StartListening(CancellationToken.None);
        }


        protected override async void OnAppearing()
        {
            base.OnAppearing();
            var userSettings = AppUserSettings.Load();

            ChangeBuyableItemsState(userSettings);
            _heartMinusDispatcher.Start();
            
            this.CoinLabel.Text = userSettings.Coins.ToString();
            DebounceCharacterFeelings();
            StopCharacterSleeping();
            CharacterHeartRegenerate();
            HomeButtonBorder.IsVisible = false;
            _audioBackground.Volume = 0.5;

            //VolksfestButtonBorder.IsVisible = true;
            //MaibaumButtonBorder.IsVisible = true;
            _characterFeelingsDebounceDispatcher.Debounce(() => { });
            OchkatzlSchwoafGameBorder.IsVisible = true;
            if (_isFirstTimeOpened)
            {
                _audioPlayerIntroSound.Play();
                _audioPlayerIntroSound.PlaybackEnded += AudioPlayerIntroSoundOnPlaybackEnded;
            }
            else
            {
                CharacterHeartRegenerate();
                DebounceCharacterFeelings();
                _audioBackground.Loop = true;
                _audioBackground.Play();
            }
            _isFirstTimeOpened = false;
            _characterIsBusy = false;
            await FadeInScene();
        }

        private void AudioPlayerIntroSoundOnPlaybackEnded(object? sender, EventArgs e)
        {

            CharacterHeartRegenerate();

            _audioBackground.Loop = true;
            _audioBackground.Play();

            LetSeppSpeak(
                GetRandomEntry(_audioPlayerGreeting));
            DebounceCharacterFeelings();
            _audioPlayerIntroSound.PlaybackEnded -= AudioPlayerIntroSoundOnPlaybackEnded;
        }

        private async void HomeButton_OnClicked(object? sender, EventArgs e)
        {
            HomeButtonBorder.IsVisible = false;
            //VolksfestButtonBorder.IsVisible = true;
            //MaibaumButtonBorder.IsVisible = true;
            OchkatzlSchwoafGameBorder.IsVisible = true;
            await ChangeScene("background_wiese.png", "background_sound_wiese.mp3");
        }

        private async Task ChangeScene(string backgroundImage, string? backgroundSound)
        {
            BackgroundImage.Source = backgroundImage;
            await ChangeScene();

            if (backgroundSound != null)
            {
                _audioBackground = CreateAudioPlayer(backgroundSound);
                _audioBackground.Loop = true;
                _audioBackground.Play();
            }

            LetSeppSpeak(GetRandomEntry(_audioPlayerGreeting));
        }

        private async Task ChangeScene()
        {
            FeelingBubbleImage.Source = string.Empty;
            _characterFeeling = CharacterFeelings.None;
            CharacterHeartRegenerate();
            _heartMinusDispatcher.Stop();
            _audioBackground.Stop();
            _characterIsBusy = true;
            await FadeOutScene();
            CharacterHeartRegenerate();
        }


        /*
        private async void VolksfestButton_OnClicked(object? sender, EventArgs e)
        {
            HomeButtonBorder.IsVisible = true;
            VolksfestButtonBorder.IsVisible = false;
            MaibaumButtonBorder.IsVisible = true;

            await ChangeScene("background_volksfest.png", "volksfest.mp3");

        }

        private async void MaibaumButton_OnClicked(object? sender, EventArgs e)
        {
            HomeButtonBorder.IsVisible = true;
            VolksfestButtonBorder.IsVisible = true;
            MaibaumButtonBorder.IsVisible = false;

            await ChangeScene("background_maibaum.png", "maibaum.mp3");

        }*/

        private void ImprintButton_OnClicked(object? sender, EventArgs e)
        {
            PrepareLeavePage();
            this.Navigation.PushAsync(new ImpressumPage());
        }

        private void DataPrivacyButton_OnClicked(object? sender, EventArgs e)
        {
            PrepareLeavePage();
            this.Navigation.PushAsync(new DataPrivacyPage());
        }

        private async void OchkatzlschwafGameButton_OnClicked(object? sender, EventArgs e)
        {
            PrepareLeavePage();
            await ChangeScene();

            if (AppSettings.AreAdsEnabled)
            {
                await _redValleyInterstitualAdService.ShowAd(() =>
                {
                    IsInterstitualAdShowing = false;
                });
            }
            
            await this.Navigation.PushAsync(Resolver.Resolve<GameOachKatzlSchwoafPage>());
        }

        private void PrepareLeavePage()
        {
            _heartMinusDispatcher.Stop();
            HomeButtonBorder.IsVisible = true;
            OchkatzlSchwoafGameBorder.IsVisible = false;
            _audioBackground.Stop();
            this._characterFeelingsDebounceDispatcher.Debounce(() => { });
        }

        private void ChangeBuyableItemsState(AppUserSettings appUserSettings)
        {
            if (appUserSettings.Coins < ItemPrices.LimoPrice)
            {
                ItemLimoImageButton.Source = ImageSource.FromFile("icon_drink_limo_disabled.png");
            }
            else
            {
                ItemLimoImageButton.Source = ImageSource.FromFile("icon_drink_limo.png");
            }

            if (appUserSettings.Coins < ItemPrices.LeberkaeseSemmelPrice)
            {
                ItemLeberkaeseSemmelImageButton.Source = ImageSource.FromFile("icon_essen_leberkaese_semmel_disabled.png");
            }
            else
            {
                ItemLeberkaeseSemmelImageButton.Source = ImageSource.FromFile("icon_essen_leberkaese_semmel.png");
            }

            if (appUserSettings.Coins < ItemPrices.WeißwurstMitKetchupPrice)
            {
                ItemWeißwurstMitKetchupImageButton.Source = ImageSource.FromFile("icon_essen_weisswurst_mit_ketchup_disabled.png");
            }
            else
            {
                ItemWeißwurstMitKetchupImageButton.Source = ImageSource.FromFile("icon_essen_weisswurst_mit_ketchup.png");
            }
        }
    }

}
