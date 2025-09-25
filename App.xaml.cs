using RedValley;
using RedValley.Helper;
using RedValley.Mobile.Services;


namespace SeppApp
{
    public partial class App : Application
    {

        private readonly IRedValleyAppOpenAdService _appOpenAdService;
        private readonly IRedValleyInterstitualAdService _interstitualAdService;

        public App(IRedValleyAppOpenAdService appOpenAdService, IRedValleyInterstitualAdService interstitualAdService)
        {
            _appOpenAdService = appOpenAdService;
            _interstitualAdService = interstitualAdService;

            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return ExceptionHelper.Try("App.CreateWindow", () =>
            {
                if (AppSettings.AreAdsEnabled)
                {
                    InitializeAds();
                }
                

                var splashPage = activationState?.Context.Services?.GetRequiredService<SplashPage>();
                if (splashPage == null) return base.CreateWindow(activationState);
                var mainWindow = new Window(splashPage);

                return mainWindow;
            }, Logging.CreateLogger(Logging.CategoryBootstrapping)) ?? new Window();
        }

        private void InitializeAds()
        {
            _appOpenAdService.LoadAd();
            _interstitualAdService.LoadAd();
        }
    }
}