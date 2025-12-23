using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace ResponderApp;

public partial class AppStartupPage : ContentPage
{
    public AppStartupPage()
    {
        InitializeComponent();
        StartAppAsync();
    }

    private async void StartAppAsync()
    {
        await App.InitializeSupabaseAsync();

        var token = await SecureStorage.GetAsync("auth_token");
        System.Diagnostics.Debug.WriteLine($"[Startup] Token = {token}");

        // Load correct page
        Application.Current.MainPage = string.IsNullOrEmpty(token)
            ? new NavigationPage(new LoginPage())
            : new AppShell();
    }
}
