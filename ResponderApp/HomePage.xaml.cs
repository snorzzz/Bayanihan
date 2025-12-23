using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using System;
using System.Linq;
using System.Threading.Tasks;
using ResponderApp.Models;
using ResponderApp.Services;
using Microsoft.Maui.Maps;
using Supabase;
using System.Collections.Generic;
using System.Net.Http;
using System.Diagnostics;

namespace ResponderApp
{
    public partial class HomePage : ContentPage
    {
        private readonly SupabaseService _supabaseService;
        private System.Timers.Timer _pollingTimer;
        private System.Timers.Timer _locationUpdateTimer;
        private readonly LocationService _locationService;
        private Polyline _persistentRoute;
        private Pin _missionPin;
        private Pin _userPin;
        private Location _lastMissionLocation;
        private Location _lastLocation;
        private Location _lastReportedLocation;
        private bool _shouldFollowUser = false;
        private DateTime _lastLocationUpdateTime = DateTime.MinValue;
        private const double DISTANCE_THRESHOLD = 0.01;
        private const int TIME_THRESHOLD = 30;

        public HomePage()
        {
            InitializeComponent();
            _supabaseService = new SupabaseService(App.SupabaseClient);
            _locationService = new LocationService(_supabaseService);

            InitializeMapElements();
        }

        private void InitializeMapElements()
        {
            try
            {
                var defaultLocation = new Location(0, 0);

                _missionPin = new Pin
                {
                    Label = "Mission Location",
                    Location = defaultLocation
                };

                _userPin = new Pin
                {
                    Label = "Your Location",
                    Type = PinType.Generic,
                    Location = defaultLocation
                };

                ResponderMap.Pins.Add(_missionPin);
                ResponderMap.Pins.Add(_userPin);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Map Init Error] {ex}");
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                _shouldFollowUser = true;
                await TryCenterMapOnCurrentLocationAsync();
                await LoadAssignedMissionAsync();
                StartPolling();
                StartLocationUpdating();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[OnAppearing Error] {ex}");
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _shouldFollowUser = false;
            StopPolling();
            StopLocationUpdating();
        }

        private void StartPolling()
        {
            if (_pollingTimer == null)
            {
                _pollingTimer = new System.Timers.Timer(10000);
                _pollingTimer.Elapsed += async (s, e) =>
                {
                    await MainThread.InvokeOnMainThreadAsync(async () => await LoadAssignedMissionAsync());
                };
                _pollingTimer.AutoReset = true;
                _pollingTimer.Start();
            }
        }

        private void StopPolling()
        {
            _pollingTimer?.Stop();
            _pollingTimer?.Dispose();
            _pollingTimer = null;
        }

        private void StartLocationUpdating()
        {
            if (_locationUpdateTimer == null)
            {
                _locationUpdateTimer = new System.Timers.Timer(1000);
                _locationUpdateTimer.Elapsed += async (s, e) =>
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await CheckAndUpdateLocationAsync();
                    });
                };
                _locationUpdateTimer.AutoReset = true;
                _locationUpdateTimer.Start();
            }
        }

        private async Task CheckAndUpdateLocationAsync()
        {
            try
            {
                var currentLocation = await GetCurrentLocationAsync();
                if (currentLocation == null) return;

                bool shouldUpdate = false;
                var timeSinceLastUpdate = (DateTime.Now - _lastLocationUpdateTime).TotalSeconds;

               
                if (_lastReportedLocation == null ||
                    Location.CalculateDistance(_lastReportedLocation, currentLocation, DistanceUnits.Kilometers) > DISTANCE_THRESHOLD)
                {
                    shouldUpdate = true;
                }
               
                else if (timeSinceLastUpdate >= TIME_THRESHOLD)
                {
                    shouldUpdate = true;
                }

             
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _lastLocation = currentLocation;
                    _userPin.Location = currentLocation;

                    if (_shouldFollowUser)
                    {
                        ResponderMap.MoveToRegion(MapSpan.FromCenterAndRadius(
                            currentLocation,
                            Distance.FromKilometers(0.3)));
                    }
                });

                if (shouldUpdate)
                {
                    await UpdateLocationInDatabase(currentLocation);
                    _lastReportedLocation = currentLocation;
                    _lastLocationUpdateTime = DateTime.Now;
                    Debug.WriteLine($"Location updated: {currentLocation.Latitude}, {currentLocation.Longitude}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Location Error] {ex.Message}");
            }
        }

        private async Task UpdateLocationInDatabase(Location location)
        {
            try
            {
                var responderIdStr = Preferences.Get("ResponderID", string.Empty);
                if (Guid.TryParse(responderIdStr, out Guid responderID))
                {
                    await _supabaseService.UpdateResponderLocationAsync(
                        responderID,
                        (float)location.Latitude,
                        (float)location.Longitude,
                        await _locationService.GetAddressFromCoordinatesAsync(location.Latitude, location.Longitude));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Database Update Error] {ex.Message}");
            }
        }

        private void StopLocationUpdating()
        {
            _locationUpdateTimer?.Stop();
            _locationUpdateTimer?.Dispose();
            _locationUpdateTimer = null;
        }

        private async Task LoadAssignedMissionAsync()
        {
            try
            {
                var responderIdStr = Preferences.Get("ResponderID", string.Empty);
                if (!Guid.TryParse(responderIdStr, out Guid responderID))
                {
                    Debug.WriteLine("[Mission Load Error] Invalid Responder ID.");
                    return;
                }

                var responderResponse = await App.SupabaseClient
                    .From<Responder>()
                    .Where(r => r.ResponderID == responderID)
                    .Get();

                var responder = responderResponse.Models
                    .FirstOrDefault(r => r.ResponderStatus == "Assigned" || r.ResponderStatus == "Accepted");

                if (responder != null)
                {
                    UserNameLabel.Text = responder.ResponderFirstName + " " + responder.ResponderLastName;

                    var missionResponse = await App.SupabaseClient
                        .From<Missions>()
                        .Where(m => m.MissionID == responder.AssignedMission)
                        .Get();

                    var mission = missionResponse.Models.FirstOrDefault();
                    if (mission != null)
                    {
                        MissionPanel.BindingContext = mission;
                        MissionPanel.IsVisible = true;
                        NoMissionLabel.IsVisible = false;
                        MissionInfoStack.IsVisible = true;

                        if (responder.ResponderStatus == "Assigned")
                        {
                            AcceptButton.IsVisible = true;
                            FinishButton.IsVisible = false;
                        }
                        else if (responder.ResponderStatus == "Accepted")
                        {
                            AcceptButton.IsVisible = false;
                            FinishButton.IsVisible = true;

                            if (mission.MissionLat.HasValue && mission.MissionLng.HasValue)
                            {
                                var missionLocation = new Location(mission.MissionLat.Value, mission.MissionLng.Value);

                                if (_lastMissionLocation == null ||
                                    Location.CalculateDistance(_lastMissionLocation, missionLocation, DistanceUnits.Kilometers) > 0.01)
                                {
                                    _lastMissionLocation = missionLocation;
                                    _missionPin.Location = missionLocation;

                                    var currentLocation = await GetCurrentLocationAsync();
                                    if (currentLocation != null)
                                    {
                                        await DrawRouteOnceAsync(currentLocation, missionLocation);
                                        _shouldFollowUser = true;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        ShowNoMissionState();
                    }
                }
                else
                {
                    ShowNoMissionState();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Mission Load Error] {ex}");
                ShowNoMissionState();
            }
        }

        private async Task DrawRouteOnceAsync(Location start, Location end)
        {
            try
            {
                if (start == null || end == null) return;

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    try
                    {
                        if (_persistentRoute != null)
                        {
                            ResponderMap.MapElements.Remove(_persistentRoute);
                        }

                        string apiKey = "AIzaSyCO0TTo1COCTKnbAluUZPdkCBjL792NHEM";
                        string url = $"https://maps.googleapis.com/maps/api/directions/json?" +
                                     $"origin={start.Latitude},{start.Longitude}&destination={end.Latitude},{end.Longitude}&mode=driving&key={apiKey}";

                        using var httpClient = new HttpClient();
                        var response = httpClient.GetStringAsync(url).Result;
                        var json = Newtonsoft.Json.Linq.JObject.Parse(response);

                        if (json["status"]?.ToString() != "OK") return;

                        var points = json["routes"]?[0]?["overview_polyline"]?["points"]?.ToString();
                        if (string.IsNullOrEmpty(points)) return;

                        var polyline = new Polyline
                        {
                            StrokeColor = Colors.Blue,
                            StrokeWidth = 8
                        };

                        foreach (var location in DecodePolyline(points))
                        {
                            if (location != null) polyline.Geopath.Add(location);
                        }

                        _persistentRoute = polyline;
                        ResponderMap.MapElements.Add(_persistentRoute);

                        // Initial zoom to show both points
                        var minLat = Math.Min(start.Latitude, end.Latitude);
                        var maxLat = Math.Max(start.Latitude, end.Latitude);
                        var minLng = Math.Min(start.Longitude, end.Longitude);
                        var maxLng = Math.Max(start.Longitude, end.Longitude);
                        var center = new Location((minLat + maxLat) / 2, (minLng + maxLng) / 2);
                        var distance = Location.CalculateDistance(
                            new Location(minLat, minLng),
                            new Location(maxLat, maxLng),
                            DistanceUnits.Kilometers);

                        ResponderMap.MoveToRegion(MapSpan.FromCenterAndRadius(
                            center,
                            Distance.FromKilometers(Math.Max(distance * 1.3, 0.5))));
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[Route Drawing Failed] {ex}");
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Route Error] {ex}");
            }
        }

        private async void OnAcceptMission(object sender, EventArgs e)
        {
            try
            {
                var responderIdStr = Preferences.Get("ResponderID", string.Empty);
                if (!Guid.TryParse(responderIdStr, out Guid responderID))
                {
                    await DisplayAlert("Error", "Invalid responder ID", "OK");
                    return;
                }

                var currentLocation = await GetCurrentLocationAsync();
                if (currentLocation == null)
                {
                    await DisplayAlert("Error", "Could not get current location", "OK");
                    return;
                }

                var updateResponse = await App.SupabaseClient
                    .From<Responder>()
                    .Where(r => r.ResponderID == responderID)
                    .Set(r => r.ResponderStatus, "Accepted")
                    .Update();

                if (updateResponse.ResponseMessage.IsSuccessStatusCode)
                {
                    var mission = MissionPanel.BindingContext as Missions;
                    if (mission?.MissionLat.HasValue == true && mission.MissionLng.HasValue == true)
                    {
                        await DrawRouteOnceAsync(
                            new Location(currentLocation.Latitude, currentLocation.Longitude),
                            new Location(mission.MissionLat.Value, mission.MissionLng.Value));

                        _shouldFollowUser = true;
                        await LoadAssignedMissionAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to accept mission: {ex.Message}", "OK");
            }
        }

        private async Task TryCenterMapOnCurrentLocationAsync()
        {
            try
            {
                var location = await Geolocation.GetLastKnownLocationAsync();
                if (location != null)
                {
                    var center = new Location(location.Latitude, location.Longitude);
                    ResponderMap.MoveToRegion(MapSpan.FromCenterAndRadius(center, Distance.FromKilometers(1)));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Geolocation Error] {ex}");
            }
        }

        private async void OnProfileIconClicked(object sender, EventArgs e)
        {
            bool confirm = await Shell.Current.DisplayAlert("Logout", "Are you sure you want to log out?", "Yes", "Cancel");
            if (confirm)
            {
                await App.LogoutAndRestartAppAsync();
            }
        }
        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            bool confirm = await Shell.Current.DisplayAlert("Logout", "Are you sure you want to log out?", "Yes", "Cancel");
            if (confirm)
            {
                await App.LogoutAndRestartAppAsync();
            }
        }

        private async Task<Location> GetCurrentLocationAsync()
        {
            try
            {
                return await Geolocation.GetLocationAsync(new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Best,
                    Timeout = TimeSpan.FromSeconds(15)
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Location Error] {ex}");
                return null;
            }
        }

        private List<Location> DecodePolyline(string encodedPoints)
        {
            if (string.IsNullOrWhiteSpace(encodedPoints))
                return new List<Location>();

            var poly = new List<Location>();
            var index = 0;
            var currentLat = 0;
            var currentLng = 0;

            try
            {
                while (index < encodedPoints.Length)
                {
                    
                    var sum = 0;
                    var shifter = 0;
                    int next5Bits;
                    do
                    {
                        next5Bits = encodedPoints[index++] - 63;
                        sum |= (next5Bits & 31) << shifter;
                        shifter += 5;
                    } while (next5Bits >= 32 && index < encodedPoints.Length);

                    currentLat += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                    
                    sum = 0;
                    shifter = 0;
                    do
                    {
                        next5Bits = encodedPoints[index++] - 63;
                        sum |= (next5Bits & 31) << shifter;
                        shifter += 5;
                    } while (next5Bits >= 32 && index < encodedPoints.Length);

                    currentLng += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                    poly.Add(new Location(currentLat * 1E-5, currentLng * 1E-5));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Polyline Decode Error] {ex}");
            }

            return poly;
        }

        private void ShowNoMissionState()
        {
            MissionPanel.BindingContext = null;
            MissionPanel.IsVisible = false;
            MissionInfoStack.IsVisible = false;
            NoMissionLabel.IsVisible = true;
            AcceptButton.IsVisible = false;
            FinishButton.IsVisible = false;
        }

        private void OnHamburgerMenuClick(object sender, EventArgs e)
        {
            HamburgerMenu.IsVisible = !HamburgerMenu.IsVisible;
        }

        private async void OnReportClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ReportPage());
        }

        private async void OnAssessmentClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AssessmentPage());
        }

        private async void OnFinishMission(object sender, EventArgs e)
        {
            try
            {
                var responderIdStr = Preferences.Get("ResponderID", string.Empty);
                if (!Guid.TryParse(responderIdStr, out Guid responderID))
                {
                    await DisplayAlert("Error", "Invalid responder ID", "OK");
                    return;
                }

                var updateResponse = await App.SupabaseClient
                    .From<Responder>()
                    .Where(r => r.ResponderID == responderID)
                    .Set(r => r.ResponderStatus, "Done")
                    .Update();

                if (updateResponse.ResponseMessage.IsSuccessStatusCode)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        if (_persistentRoute != null)
                        {
                            ResponderMap.MapElements.Remove(_persistentRoute);
                            _persistentRoute = null;
                        }
                        _missionPin.Location = new Location(0, 0);
                    });

                    await DisplayAlert("Success", "Mission completed successfully", "OK");
                    await LoadAssignedMissionAsync();
                }
                else
                {
                    await DisplayAlert("Error", "Failed to update responder status", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Finish Mission Error] {ex}");
                await DisplayAlert("Error", $"Failed to complete mission: {ex.Message}", "OK");
            }
        }
    }
}