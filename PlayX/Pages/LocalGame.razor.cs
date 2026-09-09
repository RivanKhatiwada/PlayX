// File: PlayX/Pages/LocalGame.razor.cs
using Microsoft.AspNetCore.Components;

namespace PlayX.Pages
{
    public partial class LocalGame : ComponentBase
    {
        [Parameter] 
        public string GameSlug { get; set; } = string.Empty;
    }
}