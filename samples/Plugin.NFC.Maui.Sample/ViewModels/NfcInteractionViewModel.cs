namespace Plugin.NFC.Maui.Sample.ViewModels;

using System.Windows.Input;
using Microsoft.Maui.Controls;
using Plugin.NFC;
using Plugin.NFC.Maui.Sample.Services;

public partial class NfcInteractionViewModel : ObservableObject
{
    public const string AlertTitle = "NFC";
    public const string MimeType = "application/com.companyname.nfcsample";

    private readonly INFC _nfc;
    private readonly IAlertService _alertService;

    private NFCNdefTypeFormat _type;
    private bool _eventsAlreadySubscribed;
    private bool _isDeviceIos;
    private bool _deviceIsListening;
    private bool _nfcIsEnabled;
    private bool _makeTagReadOnly;
    
    private int _iosReopenAttempts;

    public NfcInteractionViewModel( IAlertService alertService)
    {
        _nfc = CrossNFC.Current;
        _alertService = alertService;

        StartListeningCommand = new Command(async () => await BeginListeningAsync());
        StopListeningCommand = new Command(async () => await StopListeningAsync());
        WriteTextCommand = new Command(async () => await PublishAsync(NFCNdefTypeFormat.WellKnown));
        WriteUriCommand = new Command(async () => await PublishAsync(NFCNdefTypeFormat.Uri));
        WriteCustomCommand = new Command(async () => await PublishAsync(NFCNdefTypeFormat.Mime));
        FormatTagCommand = new Command(async () => await PublishAsync());
    }

    public ICommand StartListeningCommand { get; }
    public ICommand StopListeningCommand { get; }
    public ICommand WriteTextCommand { get; }
    public ICommand WriteUriCommand { get; }
    public ICommand WriteCustomCommand { get; }
    public ICommand FormatTagCommand { get; }

    public bool IsDeviceIos
    {
        get => _isDeviceIos;
        private set => SetProperty(ref _isDeviceIos, value);
    }

    public bool DeviceIsListening
    {
        get => _deviceIsListening;
        private set => SetProperty(ref _deviceIsListening, value);
    }

    public bool NfcIsEnabled
    {
        get => _nfcIsEnabled;
        private set
        {
            if (SetProperty(ref _nfcIsEnabled, value))
            {
                OnPropertyChanged(nameof(NfcIsDisabled));
            }
        }
    }

    public bool NfcIsDisabled => !NfcIsEnabled;

    public bool MakeTagReadOnly
    {
        get => _makeTagReadOnly;
        set => SetProperty(ref _makeTagReadOnly, value);
    }

}
