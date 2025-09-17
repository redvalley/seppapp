using Plugin.AdMob.Services;

namespace SeppApp;

public partial class DataPrivacyPage : ContentPage
{
    public DataPrivacyPage()
    {
        InitializeComponent();

        if (AppSettings.AreAdsEnabled)
        {
            this.ConsentRemoveButton.IsVisible = false;
        }
    }

    private async void RemoveConsentButton_OnClicked(object? sender, EventArgs e)
    {

        var adConsentService = IPlatformApplication.Current?.Services.GetService<IAdConsentService>();
        if (adConsentService != null)
        {
            var hasConsentRemovedDisplayAlert = await this.DisplayAlert(Properties.Resources.AlertTitleDataPrivacyConsentRemove,
                Properties.Resources.AlertMessageDataPrivacyConsentRemove, 
                Properties.Resources.ButtonOkText,
                Properties.Resources.ButtonCancelText);

            if (hasConsentRemovedDisplayAlert)
            {
                adConsentService.Reset();
            }
        }
    }

}