using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace PlayX.Pages
{
    public partial class Selection : ComponentBase
    {
        [Inject]
        private NavigationManager Navigation { get; set; } = default!;

        [Inject]
        private ISnackbar Snackbar { get; set; } = default!;

        private void SelectMode(string mode)
        {
            switch (mode)
            {
                case "solo":
                    Snackbar.Add("Entering Solo Mode...", Severity.Info);
                    break;
                case "local":
                    Snackbar.Add("Setting up Same-Screen match...", Severity.Info);
                    break;
                case "friends":
                    Navigation.NavigateTo("/room-selection");
                    break;
                case "random":
                    Snackbar.Add("Searching for active queue...", Severity.Warning);
                    break;
            }
        }
    }
}