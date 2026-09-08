namespace PlayX.Models
{
    public class RoomState
    {
        public string RoomCode { get; set; } = string.Empty;
        public string HostId { get; set; } = string.Empty;
        public bool IsGameStarted { get; set; }
    }
}
