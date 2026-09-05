using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using PlayX.Models;

namespace PlayX.Services;

public class MultiplayerService
{
    private readonly HttpClient _http;
    private readonly string _supabaseUrl;
    private readonly string _supabaseKey;

    public MultiplayerService(HttpClient http, IConfiguration configuration)
    {
        _http = http;
        _supabaseUrl = configuration["Supabase:Url"] ?? string.Empty;
        _supabaseKey = configuration["Supabase:Key"] ?? string.Empty;
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string endpoint)
    {
        var request = new HttpRequestMessage(method, $"{_supabaseUrl}/rest/v1/{endpoint}");
        request.Headers.Add("apikey", _supabaseKey);
        request.Headers.Add("Authorization", $"Bearer {_supabaseKey}");
        return request;
    }

    public async Task<Room?> CreateRoomAsync(string mode, string hostId)
    {
        try
        {
            var roomCode = GenerateRoomCode();
            var payload = new
            {
                room_code = roomCode,
                mode = mode,
                host_id = hostId,
                is_active = true
            };

            var request = CreateRequest(HttpMethod.Post, "rooms");
            request.Headers.Add("Prefer", "return=representation");
            request.Content = JsonContent.Create(payload);

            var response = await _http.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var createdRooms = await response.Content.ReadFromJsonAsync<List<Room>>();
                return createdRooms?.FirstOrDefault();
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CreateRoomAsync Error] {ex.Message}");
            return null;
        }
    }

    public async Task<Room?> JoinRoomAsync(string roomCode)
    {
        try
        {
            var request = CreateRequest(HttpMethod.Get, $"rooms?room_code=eq.{roomCode}&is_active=eq.true");
            var response = await _http.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                var rooms = await response.Content.ReadFromJsonAsync<List<Room>>();
                return rooms?.FirstOrDefault();
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[JoinRoomAsync Error] {ex.Message}");
            return null;
        }
    }

    public async Task<Room?> FindOrCreateRandomRoomAsync(string hostId)
    {
        try
        {
            // 1. Look for an existing active room in Random mode
            var getRequest = CreateRequest(HttpMethod.Get, "rooms?mode=eq.Random&is_active=eq.true&limit=1");
            var getResponse = await _http.SendAsync(getRequest);

            if (getResponse.IsSuccessStatusCode)
            {
                var rooms = await getResponse.Content.ReadFromJsonAsync<List<Room>>();
                var openRoom = rooms?.FirstOrDefault();

                if (openRoom != null)
                {
                    return openRoom;
                }
            }

            // 2. Fall back to creating a new Random room if none exist
            return await CreateRoomAsync("Random", hostId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FindOrCreateRandomRoomAsync Error] {ex.Message}");
            return null;
        }
    }

    public async Task CloseRoomAsync(string roomCode)
    {
        try
        {
            var request = CreateRequest(new HttpMethod("PATCH"), $"rooms?room_code=eq.{roomCode}");
            request.Content = JsonContent.Create(new { is_active = false });
            await _http.SendAsync(request);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CloseRoomAsync Error] {ex.Message}");
        }
    }

    private string GenerateRoomCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}