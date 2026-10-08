using Microsoft.AspNetCore.Components;
using Service.Shared.Commons.Model.SQL;
namespace Service.UI.CMS.Blazor.Components.Shared;
public partial class ModerationView
{
    [Parameter] public ModerationStatus Status { get; set; }
    private string Text => Status == ModerationStatus.Approved ? "Đã duyệt" : "Chưa duyệt";
    private string Tone => Status == ModerationStatus.Approved ? "approved" : "pending";
}
