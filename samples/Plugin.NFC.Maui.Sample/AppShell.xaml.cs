namespace Plugin.NFC.Maui.Sample;

using Microsoft.Maui.Controls;
using Plugin.NFC.Maui.Sample.Views;

public partial class AppShell : Shell
{
    public AppShell(NfcInteractionPage nfcInteractionPage)
    {
        InitializeComponent();

        var nfcContent = new ShellContent
        {
            Title = "Plugin.NFC Maui Sample",
            Content = nfcInteractionPage
        };

        Items.Add(nfcContent);
    }
}
