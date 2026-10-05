using Service.Shared.Contracts.DTOs;
using Service.UI.CMS.Blazor.Components.Shared;

namespace Service.UI.CMS.Blazor.Components.Pages.AIChatbot;

public partial class Index
{
    private AssistantChat? chat;
    private AssistantStatusDto status = new();
    private string classifyText = "", classifyError = "";
    private bool classifying;
    private RequestClassifyResponseDto? classifyResult;

    protected override void OnInitialized() => status = AIService.GetAssistantStatus();
    private void ClearClassification() { classifyResult = null; classifyError = ""; }
    private Task LearnMore(string category) => chat?.AskQuestionAsync(category) ?? Task.CompletedTask;

    private async Task ClassifyText()
    {
        if (classifying || string.IsNullOrWhiteSpace(classifyText)) return;
        ClearClassification(); classifying = true;
        try
        {
            var result = await AIService.ClassifyRequestAsync(new() { NoiDungYeuCau = classifyText });
            if (result.Success) classifyResult = result.Data;
            else classifyError = result.Message;
        }
        catch (Exception) { classifyError = "Chưa thể phân loại yêu cầu. Hãy thử lại."; }
        finally { classifying = false; }
    }
}
