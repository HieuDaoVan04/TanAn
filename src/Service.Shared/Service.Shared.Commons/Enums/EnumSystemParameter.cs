// "Một sản phẩm của HieuDV"

using System.ComponentModel;

namespace Service.Shared.Commons.Enums
{
    public enum EnumSystemParameter
    {
        None = 0,
        [Description("Tên ứng dụng")]
        AppName = 1,
        [Description("Phiên bản hệ thống")]
        AppVersion = 2,
        [Description("Email liên hệ hỗ trợ")]
        SupportEmail = 3,
        [Description("Số điện thoại hotline")]
        Hotline = 4,
        HeaderEnabled = 5,
        HeaderContent = 6,
        FooterEnabled = 7,
        FooterContent = 8,
        PasswordMinLength = 9,
        PasswordRequireUppercase = 10,
        PasswordRequireLowercase = 11,
        PasswordRequireDigit = 12,
        PasswordRequireSpecialChar = 13,
        MinuteExpireToken = 14,
        KhoaTaiKhoan = 15,
        LoginLockoutMinutes = 16
    }
}
