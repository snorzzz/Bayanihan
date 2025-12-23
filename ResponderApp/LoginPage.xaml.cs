using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using ResponderApp.Models;
using ResponderApp.Services;
using Supabase;
using Supabase.Postgrest;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;

namespace ResponderApp
{
    public partial class LoginPage : ContentPage
    {
        private readonly SupabaseService _supabaseService;

        public LoginPage()
        {
            InitializeComponent();
            LoadRememberedCredentials();

            var client = new Supabase.Client(
                "https://kdvlodzvpytmjimwuvvt.supabase.co",
                "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImtkdmxvZHp2cHl0bWppbXd1dnZ0Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDQ3NzQyMTMsImV4cCI6MjA2MDM1MDIxM30.aEN6mC_jXZJhwnXeDwD_96eb4s3eEqA214eI9SVT7tc",
                new SupabaseOptions { AutoConnectRealtime = true }
            );

            _supabaseService = new SupabaseService(client);
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            string email = EmailEntry?.Text?.Trim().ToLower() ?? string.Empty; // Normalize email to lowercase
            string password = PasswordEntry?.Text?.Trim() ?? string.Empty;

            // Log the email and password to see if they're correct
            System.Diagnostics.Debug.WriteLine($"Email: {email}, Password: {password}");

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Error", "Please enter both email and password.", "OK");
                return;
            }

            var user = await GetUserFromSupabase(email, password);

            if (user != null)
            {
                var fullName = $"{user.ResponderFirstName} {user.ResponderLastName}";
                Preferences.Set("ResponderName", fullName);
                Preferences.Set("ResponderID", user.ResponderID.ToString());

                if (rememberMeCheckBox?.IsChecked == true)
                {
                    Preferences.Set("savedUsername", email);
                    Preferences.Set("savedPassword", password);
                    Preferences.Set("rememberMe", true);
                }
                else
                {
                    Preferences.Remove("savedUsername");
                    Preferences.Remove("savedPassword");
                    Preferences.Set("rememberMe", false);
                }

                // Request location permission and track location
                await RequestLocationPermissionAsync();

                // Navigate to AppShell
                if (Application.Current?.Windows?.FirstOrDefault() is Window window)
                {
                    window.Page = new AppShell();
                }
            }
            else
            {
                await DisplayAlert("Error", "Invalid Email or Password", "OK");
            }
        }

        private async Task<Responder?> GetUserFromSupabase(string email, string password)
        {
            try
            {
                // Fetch responder by email
                var responder = await _supabaseService.GetResponderByEmailAsync(email);

                if (responder != null)
                {
                    // Compare the passwords directly
                    if (responder.ResponderPassword.Trim() == password.Trim()) // Ensure no extra spaces
                    {
                        return responder;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Password mismatch.");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("User not found.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Supabase login error: {ex.Message}");
            }

            return null;
        }

        private void LoadRememberedCredentials()
        {
            if (Preferences.Get("rememberMe", false))
            {
                EmailEntry.Text = Preferences.Get("savedUsername", string.Empty);
                PasswordEntry.Text = Preferences.Get("savedPassword", string.Empty);
                rememberMeCheckBox.IsChecked = true;
            }
        }

        private async void OnForgotPasswordClicked(object sender, EventArgs e)
        {
            string email = await DisplayPromptAsync(
                "Forgot Password",
                "Enter your email to reset password:",
                "Send",
                "Cancel",
                "example@email.com"
            );

            if (!string.IsNullOrEmpty(email))
            {
                await DisplayAlert("Password Reset", $"We sent a password reset link to {email}.", "OK");
            }
        }

        private async Task RequestLocationPermissionAsync()
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

                    if (status != PermissionStatus.Granted)
                    {
                        await DisplayAlert("Permission Denied", "Location permission is required for this app to work properly.", "OK");
                        return;
                    }
                }

                await StartLocationTracking();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Error requesting location permission: " + ex.Message, "OK");
            }
        }

        private async Task StartLocationTracking()
        {
            try
            {
                // Get the current location of the device
                var location = await Geolocation.GetLocationAsync(
                    new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10)))
                    ?? await Geolocation.GetLastKnownLocationAsync();

                if (location != null)
                {
                    // Now use the separate lat and lng fields
                    double latitude = location.Latitude;
                    double longitude = location.Longitude;

                    var responderIdStr = Preferences.Get("ResponderID", string.Empty);
                    if (Guid.TryParse(responderIdStr, out Guid responderId))
                    {
                        // Get the location address using Geocoding
                        string locationAddress = "Unknown";
                        try
                        {
                            var placemarks = await Geocoding.Default.GetPlacemarksAsync(latitude, longitude);
                            var placemark = placemarks?.FirstOrDefault();

                            if (placemark != null)
                            {
                                locationAddress = $"{placemark.Thoroughfare} {placemark.SubThoroughfare}, {placemark.Locality}, {placemark.AdminArea}, {placemark.CountryName}";
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Geocoding failed: {ex.Message}");
                        }

                        // Update responder location with separate lat and lng values and location address
                        await _supabaseService.UpdateResponderLocationAsync(responderId, (float)latitude, (float)longitude, locationAddress);
                    }
                    else
                    {
                        await DisplayAlert("Error", "Invalid Responder ID.", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Error", "Unable to get location.", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error tracking location: " + ex.Message);
            }
        }
    }
}
