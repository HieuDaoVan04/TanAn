// "Một sản phẩm của HieuDV"

using System.ComponentModel;

namespace Service.Shared.Commons.Enums
{
    public enum EnumService
    {
        [Description("-- Chọn dịch vụ --")]
        None = 0,

        [Description("Dịch vụ Hệ thống (Portal)")]
        ServicePortal = 1,

        [Description("Dịch vụ Xã Tân An")]
        ServiceTanAn = 2,

        [Description("Dịch vụ Giao diện (UI CMS)")]
        ServiceUI = 3
    }

    public enum EnumThaoTacHeThong
    {
        [Description("-- Chọn mức độ --")]
        None = 0,

        [Description("Thông tin (Information)")]
        Information = 1,

        [Description("Cảnh báo (Warning)")]
        Warning = 2,

        [Description("Lỗi (Error)")]
        Error = 3,

        [Description("Lỗi nghiêm trọng (Critical)")]
        Critical = 4,

        [Description("Ghi vết (Debug)")]
        Debug = 5
    }
}
