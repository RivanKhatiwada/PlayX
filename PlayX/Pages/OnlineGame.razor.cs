using Microsoft.AspNetCore.Components;
using PlayX.Services;

namespace PlayX.Pages
{
    public partial class OnlineGame : ComponentBase, IDisposable
    {
        [Inject]
        protected PeerJsService PeerService { get; set; } = default!;

        [Parameter] 
        public string GameSlug { get; set; } = string.Empty;

        [Parameter] 
        public string RoomCode { get; set; } = string.Empty;

        protected string connectionStatus = "Initializing WebRTC connection...";

        protected override async Task OnInitializedAsync()
        {
            PeerService.OnDataReceived += HandleDataReceived;
            await PeerService.InitializeAsync(RoomCode);
            connectionStatus = $"Connected to room session: {RoomCode}";
            StateHasChanged();
        }

        private void HandleDataReceived(string data)
        {
            StateHasChanged();
        }

        public void Dispose()
        {
            PeerService.OnDataReceived -= HandleDataReceived;
        }
    }
}