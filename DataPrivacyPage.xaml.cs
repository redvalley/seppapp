using Plugin.AdMob.Services;

namespace SeppApp;

public partial class DataPrivacyPage : ContentPage
{
    public DataPrivacyPage()
    {
        InitializeComponent();
#if PRO_VERSION
        this.ConsentRemoveButton.IsVisible = false;
#endif
    }

    private async void RemoveConsentButton_OnClicked(object? sender, EventArgs e)
    {
#if !PRO_VERSION
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
#endif
    }

}