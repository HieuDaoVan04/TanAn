using Microsoft.AspNetCore.Components;
using Service.Shared.Commons.Model.SQL;
namespace Service.UI.CMS.Blazor.Components.Shared;
public partial class ModerationView
{
    [Parameter] public ModerationStatus Status { get; set; }
    private string Text => Status switch { ModerationStatus.Approved => "Đã duyệt", ModerationStatus.Pending => "Chờ duyệt", ModerationStatus.Rejected => "Từ chối", ModerationStatus.PendingReview => "Chờ xem xét", ModerationStatus.PendingApproval => "Chờ phê duyệt", ModerationStatus.Draft => "Nháp", ModerationStatus.Cancelled => "Hủy hoặc Xóa", _ => "Không xác định" };
    private string Tone => Status switch { ModerationStatus.Approved => "approved", ModerationStatus.Rejected or ModerationStatus.Cancelled => "rejected", ModerationStatus.Draft => "draft", _ => "pending" };
}
