namespace YourApp.Models;

public class GameState
{
    public string SessionId { get; set; } = Guid.NewGuid().ToString("N");
    public string CurrentRoomCode { get; set; } = string.Empty;
    public bool IsHost { get; set; }
    public bool IsOnline { get; set; }
    public List<Player> Players { get; set; } = new();
}

public class Player
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsReady { get; set; }
}