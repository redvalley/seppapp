namespace SeppApp;

/// <summary>
/// Holds all app related settings
/// </summary>
public static class AppSettings
{
#if IOS
#if DEBUG
    public static string AdMobAdUnitIdAppOpener = "ca-app-pub-3940256099942544/5575463023";

    public static string AdMobAdUnitIdInterstitial = "ca-app-pub-3940256099942544/4411468910";

    public static string AdMobAdUnitIdMainBanner = "ca-app-pub-3940256099942544/2435281174";
#else
    public static string AdMobAdUnitIdAppOpener = "ca-app-pub-6864374918270893/6262661560";

    public static string AdMobAdUnitIdInterstitial = "ca-app-pub-6864374918270893/1009817964";

    public static string AdMobAdUnitIdMainBanner = "ca-app-pub-6864374918270893/8745430657";
#endif

#else
#if DEBUG
    public static string AdMobAdUnitIdAppOpener = "ca-app-pub-3940256099942544/9257395921";

    public static string AdMobAdUnitIdInterstitial = "ca-app-pub-3940256099942544/1033173712";

    public static string AdMobAdUnitIdMainBanner = "ca-app-pub-3940256099942544/9214589741";
#else
    public static string AdMobAdUnitIdAppOpener = "ca-app-pub-6864374918270893/3232269588";

    public static string AdMobAdUnitIdInterstitial = "ca-app-pub-6864374918270893/4065633825";

    public static string AdMobAdUnitIdMainBanner = "ca-app-pub-6864374918270893/6672681529";
#endif
#endif
}