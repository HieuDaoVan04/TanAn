// "Một sản phẩm của HieuDV"

using System.ComponentModel;

namespace Service.Shared.Commons.Model.SQL
{
    public enum ModerationStatus
    {
        [Description("Đã duyệt")]
        Approved = 0,

        [Description("Chờ duyệt")]
        Pending = 1,

        [Description("Từ chối")]
        Rejected = 2,

        [Description("Chờ xem xét")]
        PendingReview = 3,

        [Description("Chờ phê duyệt")]
        PendingApproval = 4,

        [Description("Nháp")]
        Draft = 5,

        [Description("Hủy hoặc Xóa")]
        Cancelled = 6
    }
}
