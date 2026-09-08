using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace PlayX.Services
{
    public class PeerJsService : IAsyncDisposable
    {
        private readonly IJSRuntime _jsRuntime;
        private DotNetObjectReference<PeerJsService>? _dotNetRef;

        public event Action<string>? OnPeerConnected;
        public event Action? OnPeerDisconnected;
        public event Action<string>? OnGameInfoReceived;
        public event Action<string>? OnDataReceived;

        public PeerJsService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<string> InitializeAsync(string? customId = null)
        {
            _dotNetRef ??= DotNetObjectReference.Create(this);
            return await _jsRuntime.InvokeAsync<string>("peerManager.initPeer", customId, _dotNetRef);
        }

        public async Task ConnectAsync(string peerId)
        {
            _dotNetRef ??= DotNetObjectReference.Create(this);
            await _jsRuntime.InvokeVoidAsync("peerManager.connectToPeer", peerId, _dotNetRef);
        }

        public async Task SendAsync(string message)
        {
            await _jsRuntime.InvokeVoidAsync("peerManager.sendData", message);
        }

        public async Task DisconnectAsync()
        {
            await _jsRuntime.InvokeVoidAsync("peerManager.disconnect");
        }

        [JSInvokable]
        public void OnPeerConnectedCallback(string peerId)
        {
            OnPeerConnected?.Invoke(peerId);
        }

        [JSInvokable]
        public void OnPeerDisconnectedCallback()
        {
            OnPeerDisconnected?.Invoke();
        }

        [JSInvokable]
        public void OnDataReceivedCallback(string data)
        {
            if (data.StartsWith("GAME:"))
            {
                var gameName = data.Substring(5);
                OnGameInfoReceived?.Invoke(gameName);
            }
            else
            {
                OnDataReceived?.Invoke(data);
            }
        }

        public async ValueTask DisposeAsync()
        {
            await DisconnectAsync();
            _dotNetRef?.Dispose();
        }
    }
}