using Plugin.AdMob;
using Plugin.AdMob.Configuration;
using RedValley;
using RedValley.Helper;

namespace SeppApp
{
    public static class MauiProgram
    {
        /// <summary>
        /// This method can be used to configure the services of the app e.g. attaching any required services, adding configurations and so on.
        /// It is actually the main entry point for configuring 'dependency injection'.
        /// </summary>
        /// <param name="mauiAppBuilder">The <see cref="MauiAppBuilder"/> instance for which this method should used.</param>
        /// <param name="configureDelegate">The delegate for configuring the <see cref="IServiceCollection"/> that will be used
        /// to construct the <see cref="IServiceProvider"/>.</param>
        public static MauiAppBuilder ConfigureServices(this MauiAppBuilder mauiAppBuilder, Action<MauiAppBuilder, IServiceCollection> configureDelegate)
        {
            configureDelegate(mauiAppBuilder, mauiAppBuilder.Services);
            return mauiAppBuilder;
        }

        public static MauiApp CreateMauiApp()
        {
            return ExceptionHelper.Try("MauiProgram.CreateMauiApp", () =>
            {
#if DEBUG
                AdConfig.UseTestAdUnitIds = true;
#endif
                MauiAppBuilder builder = MauiApp.CreateBuilder();
                builder
                    .ConfigureServices(((appBuilder, services) =>
                    {
                        services.AddMinimalLogging();
                        services.AddAppServices();
                        services.AddViewModels();
                        services.AddPages();
                    }))
                    .UseMauiApp<App>()
                    .UseAdMob(automaticallyAskForConsent: false)
                    .ConfigureFonts(fonts =>
                    {
                        fonts.AddFont("Fredoka-Medium.ttf", "Fredoka-Medium");
                    });



                var mauiApp = builder.Build();
                Resolver.RegisterServiceProvider(mauiApp.Services);
                return mauiApp;
            }, Logging.CreateLogger(Logging.CategoryBootstrapping))!;
        }
    }
}
