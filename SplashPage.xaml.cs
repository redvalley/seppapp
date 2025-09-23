
using Plugin.AdMob.Services;
using RedValley;
using RedValley.Helper;
using RedValley.Mobile.Services;

#if IOS
using AppTrackingTransparency;
using UIKit;
#endif

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
        this.ImageTalkingSepp.IsVisible = true;
    }



    protected override async void OnAppearing()
    {
        await ExceptionHelper.TryAsync("SplashPage.OnAppearing", async () =>
        {
            this._isMainPageShowing = false;
            base.OnAppearing();

            if (!AppSettings.AreAdsEnabled)
            {
                await HandleAppTrackingAndConsent(async ()=>
                {
                    await ShowMainPageWithoutAd();
                });
                return;
            }

            AppUserSettings currentSettings = AppUserSettings.Load();
            var isGameStartedFirstTime = currentSettings.IsGameStartedFirstTime;
            if (isGameStartedFirstTime)
            {
                currentSettings.IsGameStartedFirstTime = false;
                currentSettings.Save();
                
                await HandleAppTrackingAndConsent(async ()=>
                {
                    await ShowMainPageWithoutAd();
                });
            }
            else
            {
                await HandleAppTrackingAndConsent(async ()=>
                {
                    await ShowMainPageAfterAd();
                });
            }
        }, Logging.CreateLogger(Logging.CategoryBootstrapping));
        
    }

    private async Task HandleAppTrackingAndConsent(Func<Task> completed)
    {
#if IOS
        while (UIApplication.SharedApplication.ApplicationState != UIApplicationState.Active)
        {
            await Task.Delay(2000);
        }

        await ATTrackingManager.RequestTrackingAuthorizationAsync();
        CheckAdConsent();
        await completed();
#else
        CheckAdConsent();
        await completed();
#endif

    }

    private void CheckAdConsent()
    {
        if (_adConsentService != null && !_adConsentService.CanRequestAds())
        {
            _adConsentService.LoadAndShowConsentFormIfRequired();
        }
    }

    private async Task ShowMainPageAfterAd()
    {
        await Task.Run(async () =>
        {
            await Task.Delay(5000);
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await this._redValleyAppOpenAdService.ShowAd(ShowMainPage);
            });
        });
    }

    private async Task ShowMainPageWithoutAd()
    {
        await Task.Run(async () =>
        {
            await Task.Delay(2000);
            await MainThread.InvokeOnMainThreadAsync(ShowMainPage);
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