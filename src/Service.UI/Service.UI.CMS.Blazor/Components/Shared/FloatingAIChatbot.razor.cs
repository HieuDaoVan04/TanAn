namespace Service.UI.CMS.Blazor.Components.Shared;

public partial class FloatingAIChatbot
{
    private bool isOpen, hasOpened;
    private void ToggleChat() { isOpen = !isOpen; hasOpened = true; }
}
