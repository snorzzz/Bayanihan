using Supabase;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Dispatching;
using ResponderApp.Services;

namespace ResponderApp;

public partial class App : Application
{
    public static Supabase.Client SupabaseClient { get; private set; }
    public static SupabaseService SupabaseService { get; private set; }

    public App()
    {
        InitializeComponent();

        // Initialize Supabase and SupabaseService
        InitializeSupabaseAsync().Wait();
        SupabaseService = new SupabaseService(SupabaseClient);

        MainPage = new AppStartupPage(); // 👈 This goes to your AppStartupPage (probably LoginPage first)
    }

    public static async Task InitializeSupabaseAsync()
    {
        if (SupabaseClient is not null)
            return;

        var supabaseUrl = "https://kdvlodzvpytmjimwuvvt.supabase.co";
        var supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImtkdmxvZHp2cHl0bWppbXd1dnZ0Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDQ3NzQyMTMsImV4cCI6MjA2MDM1MDIxM30.aEN6mC_jXZJhwnXeDwD_96eb4s3eEqA214eI9SVT7tc";
        var options = new SupabaseOptions
        {
            AutoRefreshToken = true,
            AutoConnectRealtime = false
        };

        SupabaseClient = new Supabase.Client(supabaseUrl, supabaseKey, options);
        await SupabaseClient.InitializeAsync();
    }

    public static async Task LogoutAndRestartAppAsync()
    {
        try
        {
            SecureStorage.Remove("auth_token");
            Preferences.Remove("savedUsername");
            Preferences.Remove("savedPassword");
            Preferences.Remove("ResponderName");
            Preferences.Set("rememberMe", false);

            await Task.Delay(100);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Application.Current.MainPage = new NavigationPage(new LoginPage());
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Logout Error] {ex.Message}");
        }
    }
}
