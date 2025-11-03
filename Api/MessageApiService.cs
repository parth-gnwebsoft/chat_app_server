// Api/MessageApiService.cs
using Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Api
{
    public class MessageApiService : IMessageRepository
    {
        // The HttpClient is static and long-lived
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly string _apiUrl = "https://api-demo.gnerp.in/api/Chat/Message/Insert";
        private readonly string _updateApiUrl = "https://api-demo.gnerp.in/api/Chat/Message/Update";
        private readonly string _reactionApiUrl = "https://api-demo.gnerp.in/api/Chat/Reaction/Insert";
        public MessageApiService() 
        {
            // --- The hardcoded token is GONE from here ---
            // We no longer set DefaultRequestHeaders
        }

        // The token is now passed in as an argument
        public async Task<ChatMessageResponse> SaveMessageAsync(ChatMessageRequest message, string authToken)
        {
            try
            {
                // ... (string jsonPayload, var content, var request are all the same) ...
                var request = new HttpRequestMessage(HttpMethod.Post, _apiUrl);
                request.Content = new StringContent(JsonSerializer.Serialize(message), Encoding.UTF8, "application/json-patch+json");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

                HttpResponseMessage response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {

                    Console.WriteLine("[MessageApiService] Message successfully saved to API.");

                    // --- NEW: Read and return the API's response ---
                    string jsonResponse = await response.Content.ReadAsStringAsync();

                    // Deserialize the string back into our model
                    var savedMessage = JsonSerializer.Deserialize<ChatMessageResponse>(jsonResponse);

                    if (savedMessage == null)
                    {
                        throw new Exception("Failed to deserialize API response.");
                    }

                    return savedMessage; // <-- RETURN the saved object
                }
                else
                {
                    // ... (your error logging) ...
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[MessageApiService] API Error: {response.StatusCode}");
                    Console.WriteLine($"[MessageApiService] Response: {errorResponse}");

                    response.EnsureSuccessStatusCode(); // This will throw an exception
                    throw new HttpRequestException("API call failed."); // Failsafe
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MessageApiService] Exception: {ex.Message}");
                throw;
            }
        } 

        // --- ADD THIS ENTIRE NEW METHOD ---
        public async Task<ChatMessageResponse> UpdateMessageAsync(ChatMessageRequest messageUpdate, string authToken)
        {
            try
            {
                string jsonPayload = JsonSerializer.Serialize(messageUpdate);
                // Use the 'application/json-patch+json' content type as requested
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json-patch+json");

                // Create a new request message to set the token
                var request = new HttpRequestMessage(HttpMethod.Put, _updateApiUrl); // <-- Use PUT
                request.Content = content;
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", authToken);

                HttpResponseMessage response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("[MessageApiService] Message successfully updated via API.");
                    string jsonResponse = await response.Content.ReadAsStringAsync();

                    var updatedMessage = JsonSerializer.Deserialize<ChatMessageResponse>(jsonResponse);

                    if (updatedMessage == null)
                    {
                        throw new Exception("Failed to deserialize API update response.");
                    }

                    return updatedMessage; // Return the full object from the API
                }
                else
                {
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[MessageApiService] API Error: {response.StatusCode}");
                    Console.WriteLine($"[MessageApiService] Response: {errorResponse}");

                    response.EnsureSuccessStatusCode();
                    throw new HttpRequestException("API update call failed.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MessageApiService] Exception: {ex.Message}");
                throw;
            }
        }


        public async Task<ReactionResponse> AddReactionAsync(ReactionRequest reaction, string authToken)
        {
            try
                {
                string jsonPayload = JsonSerializer.Serialize(reaction);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json-patch+json");

                // Create a new request message to set the token
                var request = new HttpRequestMessage(HttpMethod.Post, _reactionApiUrl); // <-- Use POST
                request.Content = content;
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", authToken);

                HttpResponseMessage response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("[MessageApiService] Reaction successfully saved via API.");
                    string jsonResponse = await response.Content.ReadAsStringAsync();
 
                    var savedReaction = JsonSerializer.Deserialize<ReactionResponse>(jsonResponse);
 
                    if (savedReaction == null)
                    {
                        throw new Exception("Failed to deserialize API reaction response.");
                    }

                    return savedReaction; // Return the full object from the API
                }
                else
                {
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[MessageApiService] API Error: {response.StatusCode}");
                    Console.WriteLine($"[MessageApiService] Response: {errorResponse}");

                    response.EnsureSuccessStatusCode();
                    throw new HttpRequestException("API reaction call failed.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MessageApiService] Exception: {ex.Message}");
                throw;
            }
        }
    }
}