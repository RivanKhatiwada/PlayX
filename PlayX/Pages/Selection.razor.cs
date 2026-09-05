using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using PlayX.Services;

namespace PlayX.Pages;

public partial class Selection : ComponentBase
{
    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    [Inject]
    private MultiplayerService Multiplayer { get; set; } = default!;

    private async Task SelectModeAsync(string mode)
    {
        switch (mode)
        {
            case "solo":
                Snackbar.Add("Starting Offline Solo Session...", Severity.Info);
                Navigation.NavigateTo("/game/solo");
                break;

            case "local":
                Snackbar.Add("Setting up Same-Screen Match...", Severity.Info);
                Navigation.NavigateTo("/game/local");
                break;

            case "friends":
                Snackbar.Add("Opening Party Hub...", Severity.Info);
                Navigation.NavigateTo("/lobby");
                break;
            
            case "random":
                Snackbar.Add("Searching for active match...", Severity.Info);
                var playerId = Guid.NewGuid().ToString("N");
                var room = await Multiplayer.FindOrCreateRandomRoomAsync(playerId);
                if (room != null)
                {
                    Snackbar.Add($"Joined match: {room.RoomCode}", Severity.Success);
                    Navigation.NavigateTo($"/game/{room.RoomCode}");
                }
                else
                {
                    Snackbar.Add("Matchmaking unavailable.", Severity.Error);
                }
                break;
        }
    }
}