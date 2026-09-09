using Microsoft.AspNetCore.Components;

namespace PlayX.Pages
{
    public partial class SoloGame : ComponentBase
    {
        [Parameter] 
        public string GameSlug { get; set; } = string.Empty;
    }
}