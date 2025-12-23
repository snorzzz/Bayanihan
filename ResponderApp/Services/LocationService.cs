namespace ResponderApp.Services
{
    public class LocationService
    {
        private readonly SupabaseService _supabaseService;

        public LocationService(SupabaseService supabaseService)
        {
            _supabaseService = supabaseService;
        }

        public async Task StartLocationTracking(Guid responderID)
        {
            // Request permission for location and background location
            var locationPermissionStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            if (locationPermissionStatus != PermissionStatus.Granted)
            {
                // Handle the case where location permission is not granted
                throw new UnauthorizedAccessException("Location permission is required to track your location.");
            }

            var backgroundPermissionStatus = await Permissions.RequestAsync<Permissions.LocationAlways>();

            if (backgroundPermissionStatus != PermissionStatus.Granted)
            {
                // Handle the case where background location permission is not granted
                throw new UnauthorizedAccessException("Background location permission is required.");
            }

            // Once permissions are granted, start tracking the location
            var location = await Geolocation.GetLastKnownLocationAsync();

            if (location != null)
            {
                // Convert latitude and longitude from double to float before passing to Supabase
                var locationAddress = await GetAddressFromCoordinatesAsync(location.Latitude, location.Longitude);
                await _supabaseService.UpdateResponderLocationAsync(responderID, (float)location.Latitude, (float)location.Longitude, locationAddress);
            }
        }

        // Method to get address from latitude and longitude using Geocoding API
        public async Task<string> GetAddressFromCoordinatesAsync(double latitude, double longitude)
        {
            try
            {
                // Use Geocoding API to get the address
                var placemarks = await Geocoding.GetPlacemarksAsync(latitude, longitude);
                var placemark = placemarks?.FirstOrDefault();

                // Check for available properties and construct the address string
                if (placemark != null)
                {
                    string address = string.Empty;

                    // Try using available properties to construct the address
                    if (!string.IsNullOrWhiteSpace(placemark.SubLocality))
                    {
                        address += placemark.SubLocality + ", ";
                    }

                    if (!string.IsNullOrWhiteSpace(placemark.Locality))
                    {
                        address += placemark.Locality + ", ";
                    }

                    if (!string.IsNullOrWhiteSpace(placemark.CountryName))
                    {
                        address += placemark.CountryName;
                    }
                    else if (!string.IsNullOrWhiteSpace(placemark.CountryCode))
                    {
                        address += placemark.CountryCode;
                    }

                    // If the address is still empty, return "Address not found"
                    return string.IsNullOrWhiteSpace(address) ? "Address not found" : address;
                }

                return "Address not found";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting address: {ex.Message}");
                return "Address not found";
            }
        }
    }
}
