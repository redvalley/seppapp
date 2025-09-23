using CommunityToolkit.Maui.Media;
using Plugin.AdMob.Services;
using Plugin.Maui.Audio;
using RedValley.Mobile.Services;
using SeppApp.Services;

namespace SeppApp;

public static class ServiceCollectionExtension
{
    public static void AddAppServices(this IServiceCollection services)
    {

        if (AppSettings.AreAdsEnabled)
        {
            services.AddSingleton<IRedValleyAppOpenAdService>(
                provider => new RedValleyAppOpenAdService(provider.GetRequiredService<IAppOpenAdService>(), AppSettings.AdMobAdUnitIdAppOpener));

            services.AddSingleton<IRedValleyInterstitualAdService>(
                provider => new RedValleyInterstitualAdService(provider.GetRequiredService<IInterstitialAdService>(), AppSettings.AdMobAdUnitIdInterstitial));
        }
        else
        {
            services.AddSingleton<IRedValleyAppOpenAdService>(provider => new DummyRedValleyAppOpenAdService());

            services.AddSingleton<IRedValleyInterstitualAdService>(new DummyRedValleyInterstitualAdService());
        }

        services.AddSingleton<ISpeechToText>(SpeechToText.Default);
        services.AddSingleton<IAudioManager>(AudioManager.Current);
        services.AddSingleton<ISpeechToTextService, SpeechToTextService>();
    }

    public static void AddPages(this IServiceCollection services)
    {
        services.AddScoped<SplashPage>();
        services.AddScoped<CompanySplashPage>();
        services.AddScoped<MainPage>();
        services.AddScoped<GameOachKatzlSchwoafPage>();
        services.AddScoped<GameMaibaumKraxelnPage>();
    }

    public static void AddViewModels(this IServiceCollection services)
    {
    }
}