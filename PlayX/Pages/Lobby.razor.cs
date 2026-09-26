using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using PlayX.Components.PopupDialog;
using PlayX.Services;

namespace PlayX.Pages
{
    public partial class Lobby : ComponentBase, IDisposable
    {
        [Inject]
        protected PeerJsService PeerService { get; set; } = default!;

        [Inject]
        protected IDialogService DialogService { get; set; } = default!;

        [Inject]
        protected NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        protected IJSRuntime JSRuntime { get; set; } = default!;

        [Parameter]
        [SupplyParameterFromQuery(Name = "room")]
        public string? RoomQuery { get; set; }

        protected string RoomCode { get; set; } = string.Empty;
        protected string InputCode { get; set; } = string.Empty;
        protected string SelectedGame { get; set; } = string.Empty;
        protected string StatusMessage { get; set; } = "Choose an option to begin";
        protected bool IsInLobby { get; set; } = false;
        
        protected int ConnectedPlayersCount { get; set; } = 1;
        protected int MinPlayers { get; set; } = 2;
        protected int MaxPlayers { get; set; } = 2;
        protected bool CanStartGame => _isHost && ConnectedPlayersCount >= MinPlayers && ConnectedPlayersCount <= MaxPlayers;

        private bool _isHost = false;
        private bool _hasSentJoinNotification = false;

        protected override async Task OnInitializedAsync()
        {
            PeerService.OnPeerConnected += HandlePeerConnected;
            PeerService.OnPeerDisconnected += HandlePeerDisconnected;
            PeerService.OnGameInfoReceived += HandleGameInfoReceived;
            PeerService.OnDataReceived += HandleDataReceived;

            if (!string.IsNullOrWhiteSpace(RoomQuery))
            {
                InputCode = RoomQuery.Trim();
                await JoinRoom();
            }
        }

        protected async Task OpenGameSelectionDialog()
        {
            var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small };
            var dialog = await DialogService.ShowAsync<SelectGameDialog>("Select Game", options);
            var result = await dialog.Result;

            if (!result.Canceled && result.Data is string chosenGame)
            {
                SelectedGame = chosenGame;
                SetGameLimits(chosenGame);
                await CreateRoom();
            }
        }

        private void SetGameLimits(string gameName)
        {
            switch (gameName)
            {
                case "Tic-Tac-Toe":
                case "Chess":
                    MinPlayers = 2; MaxPlayers = 2; break;
                case "Ludo":
                    MinPlayers = 2; MaxPlayers = 4; break;
                case "Auction":
                    MinPlayers = 3; MaxPlayers = 8; break;
                case "Midnight Murder":
                    MinPlayers = 5; MaxPlayers = 15; break;
                case "Who is Imposter?":
                    MinPlayers = 3; MaxPlayers = 16; break;
                default:
                    MinPlayers = 2; MaxPlayers = 4; break;
            }
        }

        private async Task CreateRoom()
        {
            _isHost = true;
            ConnectedPlayersCount = 1; 
            var randomCode = new Random().Next(100000, 999999).ToString();
            RoomCode = await PeerService.InitializeAsync(randomCode);
            IsInLobby = true;
            StatusMessage = $"Hosting {SelectedGame} (Room: {RoomCode}). Waiting for players...";
            StateHasChanged();
        }

        protected async Task JoinRoom()
        {
            if (string.IsNullOrWhiteSpace(InputCode)) return;
            
            _isHost = false;
            RoomCode = InputCode.Trim();
            await PeerService.InitializeAsync();
            
            StatusMessage = $"Connecting to room {RoomCode}...";
            IsInLobby = true;
            StateHasChanged();

            await PeerService.ConnectAsync(RoomCode);
        }

        private void HandlePeerConnected(string peerId)
        {
            InvokeAsync(async () =>
            {
                if (_isHost)
                {
                    ConnectedPlayersCount = Math.Min(MaxPlayers, ConnectedPlayersCount + 1);
                    StatusMessage = $"Player connected! Total players: {ConnectedPlayersCount}";
                    StateHasChanged();
                    
                    await Task.Delay(200);
                    // Broadcast configuration and new count to all clients immediately
                    await PeerService.SendAsync($"GAME:{SelectedGame}|{MinPlayers}|{MaxPlayers}");
                    await PeerService.SendAsync($"SYNC_PLAYERS:{ConnectedPlayersCount}");
                }
                else if (!_hasSentJoinNotification)
                {
                    _hasSentJoinNotification = true;
                    await PeerService.SendAsync("CLIENT_JOINED");
                }
            });
        }

        private void HandlePeerDisconnected()
        {
            InvokeAsync(async () =>
            {
                if (_isHost)
                {
                    ConnectedPlayersCount = Math.Max(1, ConnectedPlayersCount - 1);
                    StatusMessage = $"A player disconnected. Total players: {ConnectedPlayersCount}";
                    
                    // Broadcast updated reduction count to everyone
                    await PeerService.SendAsync($"SYNC_PLAYERS:{ConnectedPlayersCount}");
                    StateHasChanged();
                }
            });
        }

        private void HandleDataReceived(string data)
        {
            InvokeAsync(() =>
            {
                if (_isHost)
                {
                    // If Host receives data/signals from a client, broadcast that exact data payload to everyone else
                    if (data == "CLIENT_JOINED")
                    {
                        // Handled by connection callback bounds, but safe to re-sync
                    }
                    else
                    {
                        // Reflect/broadcast client messages to all other peers
                        _ = PeerService.SendAsync(data);
                    }
                }
                else
                {
                    // Client handles broadcasts from Host
                    if (data.StartsWith("SYNC_PLAYERS:"))
                    {
                        if (int.TryParse(data.Substring(13), out var count))
                        {
                            ConnectedPlayersCount = count;
                            StatusMessage = $"Synchronized room. Players: {ConnectedPlayersCount}/{MaxPlayers}";
                            StateHasChanged();
                        }
                    }
                    else if (data.StartsWith("START_GAME:"))
                    {
                        var targetGame = data.Substring(11);
                        NavigateToGame(targetGame, isHost: false);
                    }
                }
            });
        }

        private void HandleGameInfoReceived(string payload)
        {
            InvokeAsync(() =>
            {
                var parts = payload.Split('|');
                if (parts.Length == 3)
                {
                    SelectedGame = parts[0];
                    if (int.TryParse(parts[1], out var min)) MinPlayers = min;
                    if (int.TryParse(parts[2], out var max)) MaxPlayers = max;
                }
                else
                {
                    SelectedGame = payload;
                }

                StatusMessage = $"Joined {SelectedGame} room successfully!";
                StateHasChanged();
            });
        }

        protected async Task HostStartGame()
        {
            if (!CanStartGame) return;

            // Broadcast start command to all clients
            await PeerService.SendAsync($"START_GAME:{SelectedGame}");
            NavigateToGame(SelectedGame, isHost: true);
        }

        private void NavigateToGame(string gameName, bool isHost)
        {
            NavigationManager.NavigateTo($"/game/online/{Uri.EscapeDataString(gameName)}/{RoomCode}?isHost={isHost.ToString().ToLower()}");
        }

        protected async Task CopyInviteLink()
        {
            var inviteUrl = $"{NavigationManager.BaseUri}lobby?room={RoomCode}";
            await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", inviteUrl);
            StatusMessage = "Invite link copied to clipboard!";
            StateHasChanged();
        }

        public void Dispose()
        {
            PeerService.OnPeerConnected -= HandlePeerConnected;
            PeerService.OnPeerDisconnected -= HandlePeerDisconnected;
            PeerService.OnGameInfoReceived -= HandleGameInfoReceived;
            PeerService.OnDataReceived -= HandleDataReceived;
        }
    }
}