using Microsoft.AspNetCore.Components;

namespace PlayX.Pages
{
    public partial class Selection : ComponentBase
    {
        [Inject]
        protected NavigationManager Navigation { get; set; } = default!;

        protected void NavigateToLobby()
        {
            Navigation.NavigateTo("/lobby");
        }
    }
}