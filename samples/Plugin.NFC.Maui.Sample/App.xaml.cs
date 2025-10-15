namespace Plugin.NFC.Maui.Sample;

public partial class App : Application
{
    public App(AppShell shell)
    {
        InitializeComponent();

        // Force light theme
        UserAppTheme = AppTheme.Light;
        MainPage = shell;
    }
}
