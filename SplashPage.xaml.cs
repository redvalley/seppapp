#if !PRO_VERSION
using Plugin.AdMob.Services;
#endif
using ColorValley;
using RedValley;
using RedValley.Helper;
using RedValley.Mobile.Services;

namespace TalkingSepp;

public partial class SplashPage : ContentPage
{
    private bool _isMainPageShowing = false;

#if !PRO_VERSION
    private readonly IAdConsentService? _adConsentService = null;
    private readonly IRedValleyAppOpenAdService _redValleyAppOpenAdService;
    private readonly IRedValleyInterstitualAdService _redValleyInterstitualAdService;
#endif


#if PRO_VERSION
    public SplashPage()
    {
        InitializeComponent();
        this.LabelAppNameColorValleyPro.IsVisible = true;
        this.LabelAppNameColorValley.IsVisible = false;

        this.LabelSplashWelcomePro.IsVisible = true;
        this.LabelSplashWelcome.IsVisible = false;

        this.ImageColorValleyPro.IsVisible = true;
        this.ImageColorValley.IsVisible = false;

        this.BackgroundColor = Color.FromRgb(0x15, 0x13, 0x28);

    }
#else
    public SplashPage(IAdConsentService adConsentService, 
        IRedValleyAppOpenAdService redValleyAppOpenAdService,
        IRedValleyInterstitualAdService redValleyInterstitualAdService)
    {
        InitializeComponent();
        _adConsentService = adConsentService;
        _redValleyAppOpenAdService = redValleyAppOpenAdService;
        _redValleyInterstitualAdService = redValleyInterstitualAdService;


        this.LabelSplashWelcomePro.IsVisible = false;
        this.LabelSplashWelcome.IsVisible = true;

        this.ImageTalkingSeppPro.IsVisible = false;
        this.ImageTalkingSepp.IsVisible = true;
    }
#endif


    protected override async void OnAppearing()
    {
        await ExceptionHelper.TryAsync("SplashPage.OnAppearing", async () =>
        {
            this._isMainPageShowing = false;
            base.OnAppearing();

#if !PRO_VERSION
            await ShowMainPageAfterAd();
#else
            await ShowMainPageForProVersion();
#endif
        }, Logging.CreateLogger(Logging.CategoryBootstrapping));
        
    }



#if !PRO_VERSION
    private async Task ShowMainPageAfterAd()
    {
        if (_adConsentService != null && !_adConsentService.CanRequestAds())
        {
            _adConsentService.LoadAndShowConsentFormIfRequired();
        }

        await Task.Run(async () =>
        {
            await Task.Delay(5000);
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                
                await this._redValleyAppOpenAdService.ShowAd(ShowMainPage);
            });
        });
    }
#endif

#if PRO_VERSION
    private async Task ShowMainPageForProVersion()
    {
        await Task.Run(async () =>
        {
            await Task.Delay(2000);
            await MainThread.InvokeOnMainThreadAsync(ShowMainPage);
        });
    }
#endif
    private void ShowMainPage()
    {
        ExceptionHelper.Try("SplashPage.ShowMainPage", () =>
        {
            if (_isMainPageShowing)
            {
                return;
            }

            _isMainPageShowing = true;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (Application.Current?.Windows != null)
                {
                    Application.Current.Windows[0].Page = Resolver.Resolve<CompanySplashPage>();
                }
            });
        }, Logging.CreateBootstrappingLogger());
        
    }
}