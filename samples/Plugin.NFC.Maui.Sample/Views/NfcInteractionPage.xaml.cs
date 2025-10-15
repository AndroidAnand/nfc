namespace Plugin.NFC.Maui.Sample.Views;

using Plugin.NFC.Maui.Sample.ViewModels;

public partial class NfcInteractionPage : ContentPage
{
    public NfcInteractionViewModel ViewModel { get; }

    public NfcInteractionPage(NfcInteractionViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        BindingContext = ViewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ViewModel.OnAppearingAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        ViewModel.OnDisappearing();
    }

    protected override bool OnBackButtonPressed()
    {
        _ = ViewModel.StopListeningAsync();
        return base.OnBackButtonPressed();
    }
}
