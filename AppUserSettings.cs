using System.Text.Json;
using System.Text.Json.Serialization;
using ColorValley.Settings;
using RedValley.Helper;
using RedValley.Settings;

namespace SeppApp;

/// <summary>
/// The App user settings.
/// </summary>
public class AppUserSettings : UserSettings
{
    /// <summary>
    /// The name of the user settings file of this App.
    /// </summary>
    public const string AppUserSettingsFileName = "sepp_app_usersettings.seppapp";

    /// <summary>
    /// The number of coins the user has.
    /// </summary>
    public int Coins { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserSettings"/> class.
    /// </summary>
    public AppUserSettings() : base(AppUserSettingsFileName)
    {
    }

    /// <summary>
    /// Determines if the game was started for the first time.
    /// </summary>
    [JsonInclude]
    public bool IsGameStartedFirstTime { get; set; } = true;

    /// <summary>
    /// Loads the current user settings.
    /// </summary>
    public static AppUserSettings Load()
    {
        return LoadDecrypted<AppUserSettings>(GetUserSettingsFilePath(AppUserSettingsFileName))??new AppUserSettings();
    }

    /// <summary>
    /// Saves the current user settings.
    /// </summary>
    public void Save() 
    {
        this.SaveEncrypted(GetUserSettingsFilePath(AppUserSettingsFileName));
    }
}