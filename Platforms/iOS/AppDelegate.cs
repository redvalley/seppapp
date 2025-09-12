using Foundation;
using Google.MobileAds;

using Microsoft.Extensions.Logging;
using RedValley;
using RedValley.Helper;

namespace SeppApp
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp()
        {
            MauiApp? app = null;
            var logger = Logging.CreateLogger(Logging.CategoryBootstrapping);
            ExceptionHelper.Try("AppDelegate.CreateMauiApp", () =>
            {

                AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
                {
                    if (e.ExceptionObject is Exception ex)
                    {
                        logger.LogError(ex, $"Unhandled exception: {e.ExceptionObject}");
                    }
                    else
                    {
                        logger.LogError($"Unhandled exception: {e.ExceptionObject?.ToString() ?? "-"}");
                    }
                };

                app = MauiProgram.CreateMauiApp();

                if (MobileAds.SharedInstance != null)
                {
                    Google.MobileAds.MobileAds.SharedInstance.Start(completionHandler: null);
                }
            }, logger);

            if (app == null)
            {
                throw new InvalidOperationException("App is null, it seems that the app could not be initialized correctly!");
            }

            return app;
        }
    }
}