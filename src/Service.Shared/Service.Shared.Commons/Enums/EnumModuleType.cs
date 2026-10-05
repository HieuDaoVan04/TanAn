// "Một sản phẩm của HieuDV"

using System.ComponentModel;

namespace Service.Shared.Commons.Models
{
    public enum EnumModuleType
    {
        [Description("Không xác định")]
        KhongXacDinh = 0,

        [Description("Hệ thống")]
        HeThong = 1,

        [Description("CMS Portal")]
        CMS = 2,

        [Description("Nghiệp vụ Xã Tân An")]
        NghiepVu = 3
    }
}
