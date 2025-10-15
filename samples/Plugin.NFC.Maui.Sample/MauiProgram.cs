using Plugin.NFC.Hosting;
using Plugin.NFC.Maui.Sample.Services;
using Plugin.NFC.Maui.Sample.ViewModels;
using Plugin.NFC.Maui.Sample.Views;

namespace Plugin.NFC.Maui.Sample;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseNfc(options =>
            {
                options.LegacyMode = false;
                options.Configuration = new Configuration.NfcConfiguration
                {
                    DefaultLanguageCode = "fr",
                    Messages = new Configuration.UserDefinedMessages
                    {
                        NFCSessionInvalidated = "Session invalidée",
                        NFCSessionInvalidatedButton = "OK",
                        NFCWritingNotSupported = "L'écriture des TAGs NFC n'est pas supportée sur cet appareil",
                        NFCDialogAlertMessage = "Approchez votre appareil du tag NFC",
                        NFCErrorRead = "Erreur de lecture. Veuillez réessayer",
                        NFCErrorEmptyTag = "Ce tag est vide",
                        NFCErrorReadOnlyTag = "Ce tag n'est pas accessible en écriture",
                        NFCErrorCapacityTag = "La capacité de ce TAG est trop basse",
                        NFCErrorMissingTag = "Aucun tag trouvé",
                        NFCErrorMissingTagInfo = "Aucune information à écrire sur le tag",
                        NFCErrorNotSupportedTag = "Ce tag n'est pas supporté",
                        NFCErrorNotCompliantTag = "Ce tag n'est pas compatible NDEF",
                        NFCErrorWrite = "Aucune information à écrire sur le tag",
                        NFCSuccessRead = "Lecture réussie",
                        NFCSuccessWrite = "Ecriture réussie",
                        NFCSuccessClear = "Effaçage réussi"
                    }
                };
            })
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton<IAlertService, AlertService>();
        builder.Services.AddTransient<NfcInteractionViewModel>();
        builder.Services.AddTransient<NfcInteractionPage>();
        builder.Services.AddSingleton<AppShell>();

        return builder.Build();
    }
}
