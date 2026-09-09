using Microsoft.AspNetCore.Components;
using MudBlazor;
using PlayX.Components.PopupDialog;

namespace PlayX.Pages
{
    public partial class Selection : ComponentBase
    {
        [Inject]
        protected NavigationManager Navigation { get; set; } = default!;

        [Inject]
        protected IDialogService DialogService { get; set; } = default!;

        protected void NavigateToLobby()
        {
            Navigation.NavigateTo("/lobby");
        }

        protected async Task OpenSoloGameSelectionDialog()
        {
            var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small };
            var dialog = await DialogService.ShowAsync<SelectGameDialog>("Select Solo Game", options);
            var result = await dialog.Result;

            if (!result.Canceled && result.Data is string chosenGame)
            {
                var slug = chosenGame.ToLowerInvariant().Replace(" ", "-");
                Navigation.NavigateTo($"/game/offline/{slug}");
            }
        }

        protected async Task OpenLocalGameSelectionDialog()
        {
            var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small };
            var dialog = await DialogService.ShowAsync<SelectGameDialog>("Select Local Co-Op Game", options);
            var result = await dialog.Result;

            if (!result.Canceled && result.Data is string chosenGame)
            {
                var slug = chosenGame.ToLowerInvariant().Replace(" ", "-");
                Navigation.NavigateTo($"/game/pass-and-play/{slug}");
            }
        }

        protected async Task OpenRandomGameSelectionDialog()
        {
            var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small };
            var dialog = await DialogService.ShowAsync<SelectGameDialog>("Select Matchmaking Game", options);
            var result = await dialog.Result;

            if (!result.Canceled && result.Data is string chosenGame)
            {
                var slug = chosenGame.ToLowerInvariant().Replace(" ", "-");
                var randomRoom = new Random().Next(100000, 999999).ToString();
                Navigation.NavigateTo($"/game/online/{slug}/{randomRoom}");
            }
        }
    }
}
