namespace Plugin.NFC.Maui.Sample.Services;

using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

public class AlertService : IAlertService
{
    public Task ShowAlertAsync(string message, string title)
    {
        return MainThread.InvokeOnMainThreadAsync(() =>
        {
            var page = Application.Current?.MainPage;
            if (page is null)
            {
                return Task.CompletedTask;
            }

            return page.DisplayAlert(title, message, "OK");
        });
    }

    public Task<bool> ShowConfirmationAsync(string title, string message, string accept, string cancel)
    {
        return MainThread.InvokeOnMainThreadAsync(() =>
        {
            var page = Application.Current?.MainPage;
            if (page is null)
            {
                return Task.FromResult(false);
            }

            return page.DisplayAlert(title, message, accept, cancel);
        });
    }
}
