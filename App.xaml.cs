using RedValley;
using RedValley.Helper;
using RedValley.Mobile.Services;


namespace SeppApp
{
    public partial class App : Application
    {
#if !PRO_VERSION
        private readonly IRedValleyAppOpenAdService _colorValleyAppOpenAdService;
        private readonly IRedValleyInterstitualAdService _colorValleyInterstitualAdService;
        private bool _appWasDeactivated = false;
#endif

#if !PRO_VERSION
        public App(IRedValleyAppOpenAdService colorValleyAppOpenAdService, IRedValleyInterstitualAdService colorValleyInterstitualAdService)
        {
            _colorValleyAppOpenAdService = colorValleyAppOpenAdService;
            _colorValleyInterstitualAdService = colorValleyInterstitualAdService;

            InitializeComponent();
        }
#else
        public App()
        {
            InitializeComponent();
        }
#endif


        protected override Window CreateWindow(IActivationState? activationState)
        {
            return ExceptionHelper.Try("App.CreateWindow", () =>
            {
#if !PRO_VERSION
                InitializeAds();
#endif

                var splashPage = activationState?.Context.Services?.GetRequiredService<SplashPage>();
                if (splashPage == null) return base.CreateWindow(activationState);
                var mainWindow = new Window(splashPage);

#if !PRO_VERSION
                mainWindow.Activated += MainWindowOnActivated;
                mainWindow.Deactivated += MainWindowOnDeactivated;
#endif
                return mainWindow;
            }, Logging.CreateLogger(Logging.CategoryBootstrapping)) ?? new Window();
        }

#if !PRO_VERSION
        private void MainWindowOnDeactivated(object? sender, EventArgs e)
        {
            if (Windows[0].Page is not NavigationPage navigationPage) return;

            if (!(navigationPage.CurrentPage is MainPage { IsInterstitualAdShowing: true }))
            {
                _appWasDeactivated = true;
            }
        }

        private void InitializeAds()
        {
            _colorValleyAppOpenAdService.LoadAd();
            _colorValleyInterstitualAdService.LoadAd();

        }

        private async void MainWindowOnActivated(object? sender, EventArgs e)
        {
            if (Windows[0].Page is not NavigationPage || !_appWasDeactivated) return;
            _appWasDeactivated = false;
            await _colorValleyAppOpenAdService.ShowAd(() => { });

        }
#endif
    }
}