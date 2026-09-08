using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using PlayX.Components;
using PlayX.Services;

namespace PlayX.Pages
{
    public partial class Lobby : ComponentBase, IDisposable
    {
        [Inject]
        protected PeerJsService PeerService { get; set; } = default!;

        [Inject]
        protected IDialogService DialogService { get; set; } = default!;

        protected string RoomCode { get; set; } = string.Empty;
        protected string InputCode { get; set; } = string.Empty;
        protected string SelectedGame { get; set; } = string.Empty;
        protected string StatusMessage { get; set; } = "Choose an option to begin";
        protected bool IsInLobby { get; set; } = false;
        protected bool IsGameStarted { get; set; } = false;
        private bool _isHost = false;

        protected override void OnInitialized()
        {
            PeerService.OnPeerConnected += HandlePeerConnected;
            PeerService.OnGameInfoReceived += HandleGameInfoReceived;
        }

        protected async Task OpenGameSelectionDialog()
        {
            var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small };
            var dialog = await DialogService.ShowAsync<SelectGameDialog>("Select Game", options);
            var result = await dialog.Result;

            if (!result.Canceled && result.Data is string chosenGame)
            {
                SelectedGame = chosenGame;
                await CreateRoom();
            }
        }

        private async Task CreateRoom()
        {
            _isHost = true;
            var randomCode = new Random().Next(100000, 999999).ToString();
            RoomCode = await PeerService.InitializeAsync(randomCode);
            IsInLobby = true;
            StatusMessage = $"Hosting {SelectedGame} (Room: {RoomCode}). Waiting for peer...";
            StateHasChanged();
        }

        protected async Task JoinRoom()
        {
            if (string.IsNullOrWhiteSpace(InputCode)) return;
            
            _isHost = false;
            RoomCode = InputCode.Trim();
            await PeerService.InitializeAsync();
            
            StatusMessage = $"Connecting to room {RoomCode}...";
            StateHasChanged();

            await PeerService.ConnectAsync(RoomCode);
        }

        private async void HandlePeerConnected(string peerId)
        {
            if (_isHost)
            {
                StatusMessage = $"Player connected! Preparing room info...";
                StateHasChanged();
                
                await Task.Delay(500);
                await PeerService.SendAsync($"GAME:{SelectedGame}");
                
                IsGameStarted = true;
                StateHasChanged();
            }
        }

        private void HandleGameInfoReceived(string gameName)
        {
            InvokeAsync(async () =>
            {
                SelectedGame = gameName;
                StateHasChanged();

                var parameters = new DialogParameters<JoinConfirmationDialog> { 
                    { x => x.GameName, gameName },
                    { x => x.RoomCode, RoomCode }
                };
                
                var dialog = await DialogService.ShowAsync<JoinConfirmationDialog>("Join Room Request", parameters);
                var result = await dialog.Result;

                if (result.Canceled)
                {
                    await PeerService.DisconnectAsync();
                    IsInLobby = false;
                    StatusMessage = "Declined room invitation.";
                    StateHasChanged();
                }
                else
                {
                    IsInLobby = true;
                    IsGameStarted = true;
                    StatusMessage = $"Joined {gameName} room successfully!";
                    StateHasChanged();
                }
            });
        }

        public void Dispose()
        {
            PeerService.OnPeerConnected -= HandlePeerConnected;
            PeerService.OnGameInfoReceived -= HandleGameInfoReceived;
        }
    }
}