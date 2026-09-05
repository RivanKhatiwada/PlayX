using System.Text.Json.Serialization;

namespace PlayX.Models;

public class Room
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("room_code")]
    public string RoomCode { get; set; } = string.Empty;

    [JsonPropertyName("mode")]
    public string Mode { get; set; } = string.Empty;

    [JsonPropertyName("host_id")]
    public string HostId { get; set; } = string.Empty;

    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; } = true;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
}