using ResponderApp;

public static class MauiProgram
{
    public static Task<Supabase.Client> SupabaseClient { get; private set; }

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();
        builder.UseMauiMaps();
        var options = new Supabase.SupabaseOptions
        {
            AutoConnectRealtime = true
        };

        var client = new Supabase.Client(
            "https://kdvlodzvpytmjimwuvvt.supabase.co",
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImtkdmxvZHp2cHl0bWppbXd1dnZ0Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDQ3NzQyMTMsImV4cCI6MjA2MDM1MDIxM30.aEN6mC_jXZJhwnXeDwD_96eb4s3eEqA214eI9SVT7tc",
            options);

        SupabaseClient = client.InitializeAsync().ContinueWith(_ => client);

        return builder.Build();
    }
}