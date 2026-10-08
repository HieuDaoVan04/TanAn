namespace Service.Shared.Commons.Models;

/// <summary>Danh sách thôn do chủ dự án cung cấp, cập nhật ngày 26/09/2026.</summary>
public static class TanAnLocalities
{
    // Số liệu người dùng cung cấp; chưa có văn bản nguồn/ngày thống kê để xác minh.
    // Không dùng thay cho số hồ sơ hộ gia đình đã nhập trong database.
    public static int? ReferenceHouseholds(string village) => village switch
    {
        "Thôn Cổ Bình" => 719,
        "Thôn Phùng Xá" => 728,
        "Thôn Ứng Mộ" => 939,
        "Thôn Kim Chuế" => 761,
        "Thôn Tam Tân" => 772,
        _ => null
    };
    public const string All = "— Tất cả thôn —";
    public static IReadOnlyList<string> Villages { get; } = Array.AsReadOnly(new[]
    {
        "Thôn Cổ Bình", "Thôn Phùng Xá", "Thôn Ứng Mộ", "Thôn Kim Chuế",
        "Thôn Tam Tân", "Thôn Tế Cầu", "Thôn Đồng Lạc", "Thôn Mai Động",
        "Thôn Kim Húc", "Thôn Hữu Chung", "Thôn Tiền Liệt"
    });
}
