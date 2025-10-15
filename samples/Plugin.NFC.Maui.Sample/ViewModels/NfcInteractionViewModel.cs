namespace Plugin.NFC.Maui.Sample.ViewModels;

using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Plugin.NFC;
using Plugin.NFC.Abstractions;
using Plugin.NFC.Utils;
using Plugin.NFC.Maui.Sample.Services;

public class NfcInteractionViewModel : ObservableObject
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

    public NfcInteractionViewModel(INFC nfc, IAlertService alertService)
    {
        _nfc = nfc;
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

    public async Task OnAppearingAsync()
    {
        if (!CrossNFC.IsSupported)
        {
            return;
        }

        if (!_nfc.IsAvailable)
        {
            await ShowAlertAsync("NFC is not available");
        }

        NfcIsEnabled = _nfc.IsEnabled;

        if (!NfcIsEnabled)
        {
            await ShowAlertAsync("NFC is disabled");
        }

        if (DeviceInfo.Platform == DevicePlatform.iOS)
        {
            IsDeviceIos = true;
        }

        await AutoStartAsync();
    }

    public void OnDisappearing()
    {
        _ = StopListeningAsync();
    }

    public Task StopListeningAsync()
    {
        return ExecuteSafeAsync(async () =>
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                _nfc.StopListening();
                UnsubscribeEvents();
            });
        });
    }

    private async Task AutoStartAsync()
    {
        await Task.Delay(500);
        await StartListeningIfNotIosAsync();
    }

    private Task BeginListeningAsync()
    {
        return ExecuteSafeAsync(async () =>
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                SubscribeEvents();
                _nfc.StartListening();
            });
        });
    }

    private Task PublishAsync(NFCNdefTypeFormat? type = null)
    {
        return ExecuteSafeAsync(async () =>
        {
            await StartListeningIfNotIosAsync();

            _type = type ?? NFCNdefTypeFormat.Empty;

            if (MakeTagReadOnly)
            {
                var confirmed = await _alertService.ShowConfirmationAsync(
                    "Warning",
                    "Make a Tag read-only operation is permanent and can't be undone. Are you sure you wish to continue?",
                    "Yes",
                    "No");

                if (!confirmed)
                {
                    MakeTagReadOnly = false;
                    return;
                }
                _makeTagReadOnly = true;
            }
            else
            {
                _makeTagReadOnly = false;
            }

            _nfc.StartPublishing(!type.HasValue);
        });
    }

    private async Task ExecuteSafeAsync(Func<Task> task)
    {
        try
        {
            await task();
        }
        catch (Exception ex)
        {
            await ShowAlertAsync(ex.Message);
        }
    }

    private async Task StartListeningIfNotIosAsync()
    {
        if (IsDeviceIos)
        {
            SubscribeEvents();
            return;
        }

        await BeginListeningAsync();
    }

    private void SubscribeEvents()
    {
        if (_eventsAlreadySubscribed)
        {
            UnsubscribeEvents();
        }

        _eventsAlreadySubscribed = true;

        _nfc.OnMessageReceived += OnMessageReceived;
        _nfc.OnMessagePublished += OnMessagePublished;
        _nfc.OnTagDiscovered += OnTagDiscovered;
        _nfc.OnNfcStatusChanged += OnNfcStatusChanged;
        _nfc.OnTagListeningStatusChanged += OnTagListeningStatusChanged;

        if (IsDeviceIos)
        {
            _nfc.OniOSReadingSessionCancelled += OniOsReadingSessionCancelled;
        }
    }

    private void UnsubscribeEvents()
    {
        if (!_eventsAlreadySubscribed)
        {
            return;
        }

        _nfc.OnMessageReceived -= OnMessageReceived;
        _nfc.OnMessagePublished -= OnMessagePublished;
        _nfc.OnTagDiscovered -= OnTagDiscovered;
        _nfc.OnNfcStatusChanged -= OnNfcStatusChanged;
        _nfc.OnTagListeningStatusChanged -= OnTagListeningStatusChanged;

        if (IsDeviceIos)
        {
            _nfc.OniOSReadingSessionCancelled -= OniOsReadingSessionCancelled;
        }

        _eventsAlreadySubscribed = false;
    }

    private void OnTagListeningStatusChanged(bool isListening)
    {
        DeviceIsListening = isListening;
    }

    private async void OnNfcStatusChanged(bool isEnabled)
    {
        NfcIsEnabled = isEnabled;
        await ShowAlertAsync($"NFC has been {(isEnabled ? "enabled" : "disabled")}");
    }

    private async void OnMessageReceived(ITagInfo tagInfo)
    {
        if (tagInfo == null)
        {
            await ShowAlertAsync("No tag found");
            return;
        }

        var identifier = tagInfo.Identifier;
        var serialNumber = NFCUtils.ByteArrayToHexString(identifier, ":");
        var title = !string.IsNullOrWhiteSpace(serialNumber) ? $"Tag [{serialNumber}]" : "Tag Info";

        if (tagInfo.IsFormatable)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Unformatted Tag:");
            sb.Append("Use \"Clear Tag\" to format it.");
            await ShowAlertAsync(sb.ToString(), title);
        }
        else if (!tagInfo.IsSupported)
        {
            await ShowAlertAsync("Unsupported tag (app)", title);
        }
        else if (tagInfo.IsEmpty)
        {
            await ShowAlertAsync("Empty tag", title);
        }
        else
        {
            var first = tagInfo.Records[0];
            await ShowAlertAsync(GetMessage(first), title);
        }
    }

    private void OniOsReadingSessionCancelled(object? sender, EventArgs e)
    {
        Debug("iOS NFC Session has been cancelled");
    }

    private async void OnMessagePublished(ITagInfo tagInfo)
    {
        try
        {
            MakeTagReadOnly = false;
            _nfc.StopPublishing();
            if (tagInfo.IsEmpty)
            {
                await ShowAlertAsync("Formatting tag operation successful");
            }
            else
            {
                await ShowAlertAsync("Writing tag operation successful");
            }
        }
        catch (Exception ex)
        {
            await ShowAlertAsync(ex.Message);
        }
    }

    private async void OnTagDiscovered(ITagInfo tagInfo, bool format)
    {
        if (!_nfc.IsWritingTagSupported)
        {
            await ShowAlertAsync("Writing tag is not supported on this device");
            return;
        }

        try
        {
            NFCNdefRecord? record = null;
            switch (_type)
            {
                case NFCNdefTypeFormat.WellKnown:
                    record = new NFCNdefRecord
                    {
                        TypeFormat = NFCNdefTypeFormat.WellKnown,
                        MimeType = MimeType,
                        Payload = NFCUtils.EncodeToByteArray("Plugin.NFC is awesome!"),
                        LanguageCode = "en"
                    };
                    break;
                case NFCNdefTypeFormat.Uri:
                    record = new NFCNdefRecord
                    {
                        TypeFormat = NFCNdefTypeFormat.Uri,
                        Payload = NFCUtils.EncodeToByteArray("https://github.com/franckbour/Plugin.NFC")
                    };
                    break;
                case NFCNdefTypeFormat.Mime:
                    record = new NFCNdefRecord
                    {
                        TypeFormat = NFCNdefTypeFormat.Mime,
                        MimeType = MimeType,
                        Payload = NFCUtils.EncodeToByteArray("Plugin.NFC is awesome!")
                    };
                    break;
            }

            if (!format && record is null)
            {
                throw new Exception("Record can't be null.");
            }

            tagInfo.Records = record is null ? Array.Empty<NFCNdefRecord>() : new[] { record! };

            if (format)
            {
                _nfc.ClearMessage(tagInfo);
            }
            else
            {
                _nfc.PublishMessage(tagInfo, _makeTagReadOnly);
            }
        }
        catch (Exception ex)
        {
            await ShowAlertAsync(ex.Message);
        }
    }

    private Task ShowAlertAsync(string message, string? title = null)
    {
        return _alertService.ShowAlertAsync(message, string.IsNullOrWhiteSpace(title) ? AlertTitle : title);
    }

    private static string GetMessage(NFCNdefRecord record)
    {
        var message = $"Message: {record.Message}";
        message += Environment.NewLine;
        message += $"RawMessage: {(record.Payload is null ? "N/A" : Encoding.UTF8.GetString(record.Payload))}";
        message += Environment.NewLine;
        message += $"Type: {record.TypeFormat}";

        if (!string.IsNullOrWhiteSpace(record.MimeType))
        {
            message += Environment.NewLine;
            message += $"MimeType: {record.MimeType}";
        }

        return message;
    }

    private static void Debug(string message)
    {
        System.Diagnostics.Debug.WriteLine(message);
    }
}
