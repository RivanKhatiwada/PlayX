using Microsoft.AspNetCore.Components;
using MudBlazor;
using PlayX.Services;

namespace PlayX.Pages;

public class LobbyComponentBase : ComponentBase
{
    [Inject] protected NavigationManager Navigation { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    [Inject] protected MultiplayerService Multiplayer { get; set; } = default!;

    protected string RoomCodeInput { get; set; } = string.Empty;
    protected bool _isLoading;

    protected async Task HostNewRoomAsync()
    {
        Console.WriteLine("--- HOST ROOM BUTTON CLICKED ---"); // <-- Add this line first
    
        _isLoading = true;
        StateHasChanged(); // Force UI to update

        try
        {
            var hostId = Guid.NewGuid().ToString("N");
            var room = await Multiplayer.CreateRoomAsync("CustomParty", hostId);

            if (room != null && !string.IsNullOrEmpty(room.RoomCode))
            {
                Console.WriteLine($"--- ROOM CREATED: {room.RoomCode} ---");
                Snackbar.Add($"Room created successfully! Code: {room.RoomCode}", Severity.Success);
                Navigation.NavigateTo($"/game/{room.RoomCode}");
            }
            else
            {
                Console.WriteLine("--- ROOM CREATION RETURNED NULL ---");
                Snackbar.Add("Failed to create room. Check console.", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"--- EXCEPTION IN LOBBY: {ex.Message} ---");
            Snackbar.Add("An error occurred.", Severity.Error);
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    protected async Task JoinExistingRoomAsync()
    {
        if (string.IsNullOrWhiteSpace(RoomCodeInput)) return;

        _isLoading = true;
        try
        {
            var cleanCode = RoomCodeInput.Trim().ToUpper();
            var room = await Multiplayer.JoinRoomAsync(cleanCode);

            if (room != null)
            {
                Snackbar.Add("Joined room successfully!", Severity.Success);
                Navigation.NavigateTo($"/game/{room.RoomCode}");
            }
            else
            {
                Snackbar.Add("Room not found or game already finished.", Severity.Warning);
            }
        }
        catch (Exception)
        {
            Snackbar.Add("Could not connect to the match server.", Severity.Error);
        }
        finally
        {
            _isLoading = false;
        }
    }
}