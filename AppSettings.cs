using System.Security;

namespace ColorValley;

/// <summary>
/// Holds all app related settings
/// </summary>
public static class AppSettings
{
#if IOS
    public static string AdMobAdUnitIdAppOpener = "ca-app-pub-3940256099942544/5575463023";

    public static string AdMobAdUnitIdInterstitial = "ca-app-pub-3940256099942544/4411468910";

    public static string AdMobAdUnitIdMainBanner = "ca-app-pub-3940256099942544/2435281174";

#else
    public static string AdMobAdUnitIdAppOpener = "ca-app-pub-3940256099942544/9257395921";

    public static string AdMobAdUnitIdInterstitial = "ca-app-pub-3940256099942544/1033173712";

    public static string AdMobAdUnitIdMainBanner = "ca-app-pub-3940256099942544/9214589741";
#endif
}