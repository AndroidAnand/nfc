namespace Plugin.NFC.Maui.Sample.Services;

using System.Threading.Tasks;

public interface IAlertService
{
    Task ShowAlertAsync(string message, string title);

    Task<bool> ShowConfirmationAsync(string title, string message, string accept, string cancel);
}
