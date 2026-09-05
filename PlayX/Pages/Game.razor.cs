using Microsoft.AspNetCore.Components;
using MudBlazor;
using PlayX.Models;
using PlayX.Services;

namespace PlayX.Pages;

public class GameComponentBase : ComponentBase
{
    [Parameter] public string ModeOrCode { get; set; } = string.Empty;

    [Inject] protected NavigationManager Navigation { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    [Inject] protected MultiplayerService Multiplayer { get; set; } = default!;

    protected bool _isLoading = true;
    protected bool IsOfflineMode => ModeOrCode.Equals("solo", StringComparison.OrdinalIgnoreCase) || 
                                    ModeOrCode.Equals("local", StringComparison.OrdinalIgnoreCase);
    
    protected string DisplayMode => IsOfflineMode ? ModeOrCode.ToUpper() : "MULTIPARTY ROOM";
    protected Room? CurrentRoom { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (IsOfflineMode)
        {
            _isLoading = false;
            return;
        }

        // Validate and fetch online room details
        try
        {
            CurrentRoom = await Multiplayer.JoinRoomAsync(ModeOrCode);
            if (CurrentRoom == null)
            {
                Snackbar.Add("Room not found or session closed.", Severity.Error);
                Navigation.NavigateTo("/lobby");
            }
        }
        catch (Exception)
        {
            Snackbar.Add("Failed to load room data.", Severity.Error);
            Navigation.NavigateTo("/lobby");
        }
        finally
        {
            _isLoading = false;
        }
    }

    protected void LeaveGame()
    {
        if (!IsOfflineMode && CurrentRoom != null)
        {
            // Optional: Close room or exit
        }
        Navigation.NavigateTo("/selection");
    }
}