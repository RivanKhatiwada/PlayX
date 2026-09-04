namespace PlayX.Pages;

public partial class Home
{
    private void GameSelection() => Navigation.NavigateTo("/selection");
    private void OpenAvatarCustomizer() => Navigation.NavigateTo("/customize-avatar");
    private void OpenOptions() => Navigation.NavigateTo("/options");
}