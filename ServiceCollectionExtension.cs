using ColorValley;
using CommunityToolkit.Maui.Media;
using Plugin.AdMob.Services;
using RedValley.Mobile.Services;

namespace TalkingSepp;

public static class ServiceCollectionExtension
{
    public static void AddAppServices(this IServiceCollection services)
    {
#if !PRO_VERSION
        services.AddSingleton<IRedValleyAppOpenAdService>(
            provider => new RedValleyAppOpenAdService(provider.GetRequiredService<IAppOpenAdService>(), AppSettings.AdMobAdUnitIdAppOpener) );

        services.AddSingleton<IRedValleyInterstitualAdService>(
            provider => new RedValleyInterstitualAdService(provider.GetRequiredService<IInterstitialAdService>(), AppSettings.AdMobAdUnitIdInterstitial));
#endif
        services.AddSingleton<ISpeechToText>(SpeechToText.Default);
    }

    public static void AddPages(this IServiceCollection services)
    {
        services.AddScoped<SplashPage>();
        services.AddScoped<CompanySplashPage>();
        services.AddScoped<MainPage>();
    }

    public static void AddViewModels(this IServiceCollection services)
    {
    }
}