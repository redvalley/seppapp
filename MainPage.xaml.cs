using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Media;
using Plugin.Maui.Audio;
using RedValley;
using RedValley.Helper;
using RedValley.Mobile.Services;
using SeppApp.Models;
using System.Globalization;
using RedValley.Extensions;
using SeppApp.Services;
using TalkingSepp.Models;

namespace SeppApp
{
    public partial class MainPage : ContentPage
    {
        private readonly ISpeechToTextService _speechToTextService;
        private readonly IAudioManager _audioManager;
        private readonly IRedValleyInterstitualAdService _redValleyInterstitualAdService;

        private IDispatcherTimer _characterAnimationTimer;
        private bool _isFirstTimeOpened = true;
        private bool _characterIsBusy = false;
        private bool _isPageActive = false;
        private IDispatcherTimer _heartMinusDispatcher;
        public bool IsInterstitualAdShowing { get; set; }
        private bool _lastCallWeisswurst = false;
        private bool _seppIsWegWeisswurst = false;
        private bool _seppMechtWegWeisswurst = false;
        private bool _wrongFeelingFulFilled = false;

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

        private List<IAudioPlayer> _audioPlayerWeisswurstMitKetchup = new List<IAudioPlayer>();

        private IAudioPlayer _audioPlayerWeisswurstMitKetchupLastCall;
        private IAudioPlayer? _audioPlayerWeisswurstMitKetchupCurrent;

        private List<CharacterFeelings> _characterFeelings = [CharacterFeelings.Hungry, CharacterFeelings.Thirsty, CharacterFeelings.Bored];

        private DebounceDispatcher _characterFeelingsDebounceDispatcher = new DebounceDispatcher(30000);
        private CharacterFeelings _characterFeeling = CharacterFeelings.None;
        private CharacterStates _characterState = CharacterStates.Awake;

        private readonly Random _random = new();
        private Task _lmGenerationTask;
        private IAudioPlayer _audioPlayerWeisswurstMitKetchupSeppWeg;
        private IAudioPlayer? _seppTalkingCurrent;


        private const long DefaultAnimiationInterval = 150;
        private const long SnoringAnimiationInterval = 2000;
        private const long DrinkingEatingAnimiationInterval = 1000;

        public MainPage(ISpeechToTextService speechToTextService, IAudioManager audioManager, IRedValleyInterstitualAdService redValleyInterstitualAdService)
        {

            _speechToTextService = speechToTextService;
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
            ShowAndTalkFeeling();
        }

        private void ShowAndTalkFeeling()
        {
            switch (_characterFeeling)
            {
                case CharacterFeelings.Thirsty:
                    FeelingBubbleImage.IsVisible = true;
                    FeelingBubbleImage.Source = ImageSource.FromFile("feeling_bubble_limo.png");
                    LetSeppSpeak(_audioPlayerThirsty);
                    break;
                case CharacterFeelings.Hungry:
                    FeelingBubbleImage.IsVisible = true;
                    FeelingBubbleImage.Source = ImageSource.FromFile("feeling_bubble_leberkaesesemmel.png");
                    LetSeppSpeak(_audioPlayerHungry);
                    break;
                case CharacterFeelings.Bored:
                    FeelingBubbleImage.IsVisible = true;
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

        public TEntry GetNextEntry<TEntry>(IEnumerable<TEntry> listEntries, TEntry? currentEntry)
        {
            var list = listEntries.ToList();

            if (currentEntry == null ||
                list.IndexOf(currentEntry) == list.Count - 1)
            {
                return list.First();
            }

            return list[list.IndexOf(currentEntry) + 1];
        }


        private void InitializeSound()
        {
            _audioPlayerGreeting.Add(CreateAudioPlayer("servus.mp3"));
            _audioPlayerGreeting.Add(CreateAudioPlayer("hawedere.mp3"));

            _audioPlayerNotUnderstand.Add(CreateAudioPlayer("ha.mp3"));
            _audioPlayerNotUnderstand.Add(CreateAudioPlayer("woos.mp3"));
            _audioPlayerNotUnderstand.Add(CreateAudioPlayer("iverstehnix.mp3"));

            _audioPlayerIntroSound = CreateAudioPlayer("intro.mp3");

            _audioPlayerSchnarchen = CreateAudioPlayer("schnarchen.mp3");


            _audioPlayerEating = CreateAudioPlayer("schmatzen.mp3");
            _audioBackground = CreateAudioPlayer("background_sound_wiese.mp3");

            _audioPlayerThirsty = CreateAudioPlayer("ihobdurscht.mp3");
            _audioPlayerHungry = CreateAudioPlayer("ihobhunger.mp3");
            _audioPlayerBored = CreateAudioPlayer("mirissolangweilig.mp3");

            _audioPlayerNo = CreateAudioPlayer("naaa.mp3");
            _audioPlayerThanks = CreateAudioPlayer("merce.mp3");

            _audioPlayerHowAreYouAnswer.Add(CreateAudioPlayer("sepp_says_bastscho.mp3"));
            _audioPlayerHowAreYouAnswer.Add(CreateAudioPlayer("guadunssaiba.mp3"));

            _audioPlayerTalking.Add(CreateAudioPlayer("sepp_says_a_bissl_wos.mp3"));
            _audioPlayerTalking.Add(CreateAudioPlayer("sepp_says_back_mas.mp3"));
            _audioPlayerTalking.Add(CreateAudioPlayer("sepp_says_bastscho.mp3"));
            _audioPlayerTalking.Add(CreateAudioPlayer("sepp_says_birnbacherl.mp3"));
            _audioPlayerTalking.Add(CreateAudioPlayer("sepp_says_bluadszacke.mp3"));
            _audioPlayerTalking.Add(CreateAudioPlayer("sepp_says_des_is_a_gmade_wisn.mp3"));
            _audioPlayerTalking.Add(CreateAudioPlayer("sepp_says_desbasdscho.mp3"));
            _audioPlayerTalking.Add(CreateAudioPlayer("sepp_says_es_gibt_nix_bessers.mp3"));
            _audioPlayerTalking.Add(CreateAudioPlayer("sepp_says_gschmeidigbleim.mp3"));
            _audioPlayerTalking.Add(CreateAudioPlayer("sepp_says_hau_di_hera.mp3"));
            _audioPlayerTalking.Add(CreateAudioPlayer("sepp_says_ja_do_legst_di_nida.mp3"));
            _audioPlayerTalking.Add(CreateAudioPlayer("sepp_says_oans_zwoa_gsuffa.mp3"));
            _audioPlayerTalking.Add(CreateAudioPlayer("sepp_says_schau_ma_moi.mp3"));
            _audioPlayerTalking.Add(CreateAudioPlayer("sepp_says_scheiss_da_nix.mp3"));
            _audioPlayerTalking.Add(CreateAudioPlayer("sepp_says_sepp_sei_app.mp3"));
            _audioPlayerTalking.Add(CreateAudioPlayer("sepp_says_wennd_wurst.mp3"));
            _audioPlayerTalking.Add(CreateAudioPlayer("sepp_says_wer_ko.mp3"));
            _audioPlayerTalking.Add(CreateAudioPlayer("sepp_says_wia_d_sau.mp3"));



            _audioPlayerWeisswurstMitKetchup.Add(CreateAudioPlayer("weiss_wurscht_mit_ketchup_verbrecha.mp3"));
            _audioPlayerWeisswurstMitKetchup.Add(CreateAudioPlayer("weiss_wurscht_mit_ketchup_vergiss_i_mi.mp3"));
            _audioPlayerWeisswurstMitKetchup.Add(CreateAudioPlayer("weiss_wurscht_mit_ketchup_maibam.mp3"));
            _audioPlayerWeisswurstMitKetchup.Add(CreateAudioPlayer("weiss_wurscht_mit_ketchup_brennt_da_huad.mp3"));
            _audioPlayerWeisswurstMitKetchup.Add(CreateAudioPlayer("weiss_wurscht_mit_ketchup_meng_dua_i_ned.mp3"));
            _audioPlayerWeisswurstMitKetchup.Add(CreateAudioPlayer("weiss_wurscht_mit_ketchup_schleich_di.mp3"));
            _audioPlayerWeisswurstMitKetchup.Add(CreateAudioPlayer("weiss_wurscht_mit_ketchup_pfuideife.mp3"));

            _audioPlayerWeisswurstMitKetchupLastCall = CreateAudioPlayer("weiss_wurscht_mit_ketchup_birschal.mp3");
            _audioPlayerWeisswurstMitKetchupSeppWeg = CreateAudioPlayer("weiss_wurscht_mit_ketchup_sepp_is_weg.mp3");


            _audioPlayerDrinking = CreateAudioPlayer("drinking.mp3");
        }

        private IAudioPlayer CreateAudioPlayer(string soundFile)
        {
            return _audioManager.CreatePlayer(FileSystem.OpenAppPackageFileAsync(soundFile).Result);
        }

        private void CharacterAnimationTimerOnTick(object? sender, EventArgs e)
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

        private void CharacterButton_OnClicked(object? sender, EventArgs e)
        {
            if (_characterState == CharacterStates.Awake)
            {
                CharacterHeartRegenerate();
            }
            StopCharacterSleeping();
            DebounceCharacterFeelings();

            _seppTalkingCurrent = GetNextEntry(_audioPlayerTalking, _seppTalkingCurrent);

            LetSeppSpeak(_seppTalkingCurrent);
            DebounceCharacterFeelings();

        }

        private async Task StartListening()
        {
            CharacterImage.IsVisible = true;
            TalkNowBorder.IsVisible = true;
            CharacterImage.Source = ImageSource.FromFile("sepp_listening");

            await _speechToTextService.StartListeningAsync(CancellationToken.None, recognizedText =>
            {
                TalkNowBorder.IsVisible = false;

                RecognizeTextAndReplay(recognizedText);
            }, async () =>
            {
                TalkNowBorder.IsVisible = false;
                await Toast.Make(Properties.Resources.ToastErrorMicrophoneAccessMissing).Show(CancellationToken.None);
            }, " ", SpeechToTextService.DefaultMaxListenTimeMilliSeconds);

        }

        private void RecognizeTextAndReplay(string recognizedText)
        {
            DebounceCharacterFeelings();

            if (recognizedText.IsEmpty())
            {
                CharacterImage.Source = ImageSource.FromFile("sepp_transparent_1");
                return;
            }

            if (recognizedText.ContainsAny("hallo", "servus"))
            {
                CharacterHeartRegenerate();
                LetSeppSpeak(
                    GetRandomEntry(_audioPlayerGreeting));
            }
            else if (recognizedText.ContainsAny("wie geht es dir", "how are you", "wie geht's dir", "mir geht's da", "wia geht's da", "wie geht's da"))
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
            CharacterImage.IsVisible = true;

            if (!_isPageActive)
            {
                return;
            }

            if (_characterIsBusy)
            {
                return;
            }

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

            CharacterImage.IsVisible = true;
            DebounceCharacterFeelings();
            CharacterImage.Source = ImageSource.FromFile("sepp_transparent_1");
            _characterAnimationTimer.Stop();
            _characterIsBusy = false;
            _characterAnimationTimer.Interval = TimeSpan.FromMilliseconds(DefaultAnimiationInterval);
            _currentAnimationImageIndex = 0;

            if (_wrongFeelingFulFilled)
            {
                _wrongFeelingFulFilled = false;
                ShowAndTalkFeeling();
            }

        }


        private async Task FadeInScene()
        {
            FadeInBorder.IsVisible = true;
            FadeInBorder.BackgroundColor = Colors.Black;
            await FadeInBorder.BackgroundColorTo(Colors.Transparent, length: 2000);
            FadeInBorder.IsVisible = false;
        }

        private async Task FadeOutScene()
        {
            FadeInBorder.IsVisible = true;
            FadeInBorder.BackgroundColor = Colors.Transparent;
            await FadeInBorder.BackgroundColorTo(Colors.Black, length: 2000);
        }

        private void LetSeppSleep()
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
                CharacterHeartRegenerate();

                CharacterImage.Source = ImageSource.FromFile("sepp_grantig");
                await Task.Delay(2000);


                if (_seppMechtWegWeisswurst)
                {
                    _audioPlayerWeisswurstMitKetchupCurrent = _audioPlayerWeisswurstMitKetchupSeppWeg;
                    _seppIsWegWeisswurst = true;
                    _seppMechtWegWeisswurst = false;
                }
                else if (_lastCallWeisswurst)
                {
                    _audioPlayerWeisswurstMitKetchupCurrent = _audioPlayerWeisswurstMitKetchupLastCall;
                    _lastCallWeisswurst = false;
                    _seppMechtWegWeisswurst = true;
                }
                else
                {
                    _audioPlayerWeisswurstMitKetchupCurrent = GetNextEntry(_audioPlayerWeisswurstMitKetchup,
                        _audioPlayerWeisswurstMitKetchupCurrent);
                }

                    

                if (_audioPlayerWeisswurstMitKetchupCurrent == _audioPlayerWeisswurstMitKetchup.Last())
                {
                    _lastCallWeisswurst = true;
                }



                LetSeppSpeak(_audioPlayerWeisswurstMitKetchupCurrent);

                _audioPlayerWeisswurstMitKetchupCurrent.PlaybackEnded += AudioPlayerWeisswurstMitKetchupPlaybackEnded;

            }
        }

        private void AudioPlayerWeisswurstMitKetchupPlaybackEnded(object? sender, EventArgs e)
        {
            if (_audioPlayerWeisswurstMitKetchupCurrent != null)
            {
                _audioPlayerWeisswurstMitKetchupCurrent.PlaybackEnded -= AudioPlayerWeisswurstMitKetchupPlaybackEnded;
            }

            if (_seppIsWegWeisswurst)
            {
                CharacterImage.IsVisible = false;
                _seppIsWegWeisswurst = false;
            }
            else
            {
                CharacterImage.Source = ImageSource.FromFile("sepp_transparent_1");
            }

                
        }

        private void Leberkaese_Clicked(object? sender, EventArgs e)
        {
            _wrongFeelingFulFilled = false;

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
                _wrongFeelingFulFilled = true;
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
            _wrongFeelingFulFilled = false;
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
                _wrongFeelingFulFilled = true;
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
            FeelingBubbleImage.IsVisible = false;
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
        }

        private void StopCharacterSleeping()
        {
            if (_characterState == CharacterStates.Sleeping ||
                _audioPlayerSchnarchen.IsPlaying)
            {
                _audioPlayerSchnarchen.Stop();
                _characterState = CharacterStates.Awake;
                FeelingBubbleImage.IsVisible = true;
            }

        }

        private async void TalkToSepp_OnClicked(object? sender, EventArgs e)
        {
            StopCharacterSleeping();
            await StartListening();
        }


        protected override async void OnAppearing()
        {
            base.OnAppearing();
            _lmGenerationTask = Task.Run(async () => { await _speechToTextService.Initialize(); });
            _isPageActive = true;
            var userSettings = AppUserSettings.Load();

            ChangeBuyableItemsState(userSettings);
            _heartMinusDispatcher.Start();

            this.CoinLabel.Text = userSettings.Coins.ToString();
            DebounceCharacterFeelings();
            StopCharacterSleeping();
            CharacterHeartRegenerate();
            _audioBackground.Volume = 0.5;

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

        private async Task ChangeScene()
        {
            FeelingBubbleImage.IsVisible = false;
            _characterFeeling = CharacterFeelings.None;
            CharacterHeartRegenerate();
            _heartMinusDispatcher.Stop();
            _audioBackground.Stop();
            _characterIsBusy = true;
            await FadeOutScene();
            CharacterHeartRegenerate();
        }

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

        private async void MaibaumKraxelnGameButton_OnClicked(object? sender, EventArgs e)
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

            await this.Navigation.PushAsync(Resolver.Resolve<GameMaibaumKraxelnPage>());
        }

        private void PrepareLeavePage()
        {
            _isPageActive = false;
            _heartMinusDispatcher.Stop();
            OchkatzlSchwoafGameBorder.IsVisible = false;
            _audioPlayerIntroSound.Stop();
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


        private async void FacebookButtonOnClicked(object? sender, EventArgs e)
        {
            await Launcher.OpenAsync(AppSettings.SocialMediaUrlFacebook);
        }

        private async void InstagramButtonOnClicked(object? sender, EventArgs e)
        {
            await Launcher.OpenAsync(AppSettings.SocialMediaUrlInstagram);
        }

        private async void YoutubeButtonOnClicked(object? sender, EventArgs e)
        {
            await Launcher.OpenAsync(AppSettings.SocialMediaUrlYoutube);
        }

        private async void TikTokButtonOnClicked(object? sender, EventArgs e)
        {
            await Launcher.OpenAsync(AppSettings.SocialMediaUrlTikTok);
        }
    }

}
