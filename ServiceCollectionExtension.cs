using CommunityToolkit.Maui.Media;
using Plugin.AdMob.Services;
using Plugin.Maui.Audio;
using RedValley.Mobile.Services;

namespace SeppApp;

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
        services.AddSingleton<IAudioManager>(AudioManager.Current);
    }

    public static void AddPages(this IServiceCollection services)
    {
        services.AddScoped<SplashPage>();
        services.AddScoped<CompanySplashPage>();
        services.AddScoped<MainPage>();
        services.AddScoped<GameOachKatzlSchwoafPage>();
    }

    public static void AddViewModels(this IServiceCollection services)
    {
    }
}