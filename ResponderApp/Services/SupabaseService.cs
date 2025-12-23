using Supabase;
using static Supabase.Postgrest.Constants;  // ✅ Use for the Operator enum
using ResponderApp.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Supabase.Postgrest.Constants;

namespace ResponderApp.Services
{
    public class SupabaseService
    {
        private readonly Supabase.Client _supabaseClient;

        public SupabaseService(Supabase.Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

        // Get a responder by email
        public async Task<Responder?> GetResponderByEmailAsync(string email)
        {
            try
            {
                email = email.ToLower().Trim();

                var response = await _supabaseClient
                    .From<Responder>()
                    .Filter("responderEmail", Operator.Equals, email)  // ✅ Corrected use of Operator.Equals
                    .Get();

                // Check if the response contains any models
                var responder = response.Models.FirstOrDefault();
                return responder;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching responder: {ex.Message}");
                return null;
            }
        }

        // Update responder location in the database
        public async Task UpdateResponderLocationAsync(Guid responderID, float latitude, float longitude, string locationAddress)
        {
            try
            {
                var response = await _supabaseClient
                    .From<Responder>()
                    .Filter("responderID", Operator.Equals, responderID.ToString())  // ✅ Corrected use of Operator.Equals
                    .Get();

                var responder = response.Models.FirstOrDefault();

                if (responder == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Responder not found for update.");
                    return;
                }

                responder.ResponderLat = latitude;
                responder.ResponderLng = longitude;
                responder.ResponderLocation = locationAddress;

                var updateResponse = await _supabaseClient.From<Responder>().Update(responder);

                if (updateResponse.Models != null && updateResponse.Models.Any())
                {
                    System.Diagnostics.Debug.WriteLine("Location updated successfully.");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Failed to update location.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating location: {ex.Message}");
            }
        }

        public async Task UpdateMissionStatusAsync(Guid missionId, string status, Guid[]? responderIds = null)
        {
            try
            {
                var missionResponse = await _supabaseClient
                    .From<Missions>()
                    .Filter("missionID", Operator.Equals, missionId.ToString())
                    .Get();

                var mission = missionResponse.Models.FirstOrDefault();

                if (mission == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Mission with ID {missionId} not found.");
                    return;
                }

                mission.MissionStatus = status;

                if (responderIds != null && responderIds.Length > 0)
                {
                    mission.AssignedResponderID = responderIds;
                }

                var updateResponse = await _supabaseClient
                    .From<Missions>()
                    .Update(mission);

                if (updateResponse.Models != null && updateResponse.Models.Any())
                {
                    System.Diagnostics.Debug.WriteLine($"Mission status updated to {status}.");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Failed to update mission status.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating mission status: {ex.Message}");
            }
        }
    }
}
