using Microsoft.AspNetCore.Components;

namespace PlayX.Pages
{
    public partial class Home : ComponentBase
    {
        [Inject]
        protected NavigationManager Navigation { get; set; } = null!;

        protected void NavigateToSelection()
        {
            Navigation.NavigateTo("/selection");
        }
    }
}