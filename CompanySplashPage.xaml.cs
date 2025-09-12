using RedValley;
using RedValley.Helper;
using TalkingSepp;

namespace SeppApp;

public partial class CompanySplashPage : ContentPage
{
    public CompanySplashPage()
    {
        InitializeComponent();
    }
    
    protected override async void OnAppearing()
    {
        await ExceptionHelper.TryAsync("CompanySplashPage.OnAppearing", async () =>
        {
            base.OnAppearing();
            await Task.Delay(2000);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (Application.Current?.Windows != null)
                {
                    Application.Current.Windows[0].Page = new NavigationPage(Resolver.Resolve<MainPage>());
                }
            });
        }, Logging.CreateLogger(Logging.CategoryBootstrapping))!;

    }
}