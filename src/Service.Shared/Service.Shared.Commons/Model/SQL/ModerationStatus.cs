// "Một sản phẩm của HieuDV"

using System.ComponentModel;

namespace Service.Shared.Commons.Model.SQL
{
    public enum ModerationStatus
    {
        [Description("Đã duyệt")]
        Approved = 0,

        [Description("Chưa duyệt")]
        Pending = 1,

        [Description("Chưa duyệt")]
        Rejected = 2,

        [Description("Chưa duyệt")]
        PendingReview = 3,

        [Description("Chưa duyệt")]
        PendingApproval = 4,

        [Description("Chưa duyệt")]
        Draft = 5,

        [Description("Chưa duyệt")]
        Cancelled = 6
    }
}
