using Plugin.AdMob.Services;
using RedValley;
using RedValley.Helper;
using RedValley.Mobile.Services;

namespace SeppApp;

public partial class SplashPage : ContentPage
{
    private bool _isMainPageShowing = false;


    private readonly IAdConsentService? _adConsentService = null;
    private readonly IRedValleyAppOpenAdService _redValleyAppOpenAdService;


    public SplashPage(IAdConsentService adConsentService, 
        IRedValleyAppOpenAdService redValleyAppOpenAdService)
    {
        InitializeComponent();
        _adConsentService = adConsentService;
        _redValleyAppOpenAdService = redValleyAppOpenAdService;


        this.LabelSplashWelcomePro.IsVisible = false;
        this.LabelSplashWelcome.IsVisible = true;

        this.ImageTalkingSeppPro.IsVisible = false;
        this.ImageTalkingSepp.IsVisible = true;
    }



    protected override async void OnAppearing()
    {
        await ExceptionHelper.TryAsync("SplashPage.OnAppearing", async () =>
        {
            this._isMainPageShowing = false;
            base.OnAppearing();

            await ShowMainPageAfterAd();
        }, Logging.CreateLogger(Logging.CategoryBootstrapping));
        
    }

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