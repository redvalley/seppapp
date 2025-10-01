namespace SeppApp;

/// <summary>
/// Holds all app related settings
/// </summary>
public static class AppSettings
{

    public static bool AreAdsEnabled = false;

#if IOS
#if DEBUG
    public static string AdMobAdUnitIdAppOpener = "ca-app-pub-3940256099942544/5575463023";

    public static string AdMobAdUnitIdInterstitial = "ca-app-pub-3940256099942544/4411468910";

    public static string AdMobAdUnitIdMainBanner = "ca-app-pub-3940256099942544/2435281174";
#else
    public static string AdMobAdUnitIdAppOpener = "ca-app-pub-6864374918270893/7034774374";

    public static string AdMobAdUnitIdInterstitial = "ca-app-pub-6864374918270893/8487456845";
    
#endif

#else
#if DEBUG
    public static string AdMobAdUnitIdAppOpener = "ca-app-pub-3940256099942544/9257395921";

    public static string AdMobAdUnitIdInterstitial = "ca-app-pub-3940256099942544/1033173712";

    public static string AdMobAdUnitIdMainBanner = "ca-app-pub-3940256099942544/9214589741";
#else
    public static string AdMobAdUnitIdAppOpener = "ca-app-pub-6864374918270893/3426701856";

    public static string AdMobAdUnitIdInterstitial = "ca-app-pub-6864374918270893/2029859707";
#endif
#endif

    public const string SocialMediaUrlFacebook = "https://www.facebook.com/profile.php?id=61578104993770";

    public const string SocialMediaUrlInstagram = "https://www.instagram.com/red_valley_software/";

    public const string SocialMediaUrlYoutube= "https://www.youtube.com/@RedValleySoftware";

    public const string SocialMediaUrlTikTok = "https://www.tiktok.com/@redvalley_software";
}