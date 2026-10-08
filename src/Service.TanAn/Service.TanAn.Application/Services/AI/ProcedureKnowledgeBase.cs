using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Service.TanAn.Application.Services.AI;

// Hướng dẫn sử dụng bộ mô phỏng, không phải danh mục thủ tục pháp lý đã được thẩm định.
internal static class ProcedureKnowledgeBase
{
    public const string Version = "1.0";
    public const string UpdatedOn = "2026-10-02";
    public const string Notice = "Hướng dẫn tham khảo cho hệ thống mô phỏng. Thành phần hồ sơ, điều kiện, phí và thời hạn thực tế cần được bộ phận tiếp nhận xác nhận.";

    internal sealed record Procedure(string Title, string[] Keywords, string Guidance);
    internal sealed record Match(Procedure Procedure, string[] Keywords)
    {
        public int Score => Keywords.Sum(x => x.Split(' ').Length);
    }

    public static readonly Procedure[] Procedures =
    [
        new("Đăng ký khai sinh", ["khai sinh", "giay khai sinh", "giay chung sinh", "moi sinh", "sinh con"],
            "Chuẩn bị thông tin trẻ, ngày sinh, cha mẹ và giấy tờ chứng minh sự kiện sinh để cán bộ kiểm tra. Trong phần mềm, ghi nhận tại Biến động dân cư với loại Khai sinh, liên kết hộ gia đình và nhân khẩu phù hợp; yêu cầu hỗ trợ được tiếp nhận tại Dịch vụ công."),
        new("Đăng ký khai tử", ["khai tu", "giay bao tu", "qua doi", "tu tran", "nguoi mat"],
            "Chuẩn bị thông tin người đã mất, ngày mất và giấy tờ liên quan để bộ phận tiếp nhận kiểm tra. Trong phần mềm, chọn Biến động dân cư → Khai tử, kiểm tra đúng nhân khẩu và thời điểm sự kiện trước khi ghi nhận."),
        new("Đăng ký tạm trú", ["tam tru", "o tro", "thue tro", "thue nha", "chuyen tro"],
            "Chuẩn bị thông tin người đăng ký, địa chỉ ở tạm, thời gian dự kiến và giấy tờ liên quan đến chỗ ở để cán bộ kiểm tra. Trong phần mềm mô phỏng, ghi nhận Biến động dân cư → Tạm trú hoặc gửi yêu cầu tại Dịch vụ công. Thủ tục cư trú thực tế cần được cơ quan phụ trách cư trú hướng dẫn."),
        new("Đăng ký tạm vắng", ["tam vang", "vang mat", "di xa"],
            "Mô tả lý do, địa điểm đến và khoảng thời gian vắng mặt. Trong phần mềm, chọn Biến động dân cư → Tạm vắng và liên kết nhân khẩu. Cơ quan phụ trách cư trú xác nhận trường hợp có cần khai báo và giấy tờ thực tế."),
        new("Đăng ký thường trú", ["thuong tru", "nhap ho khau", "nhap khau", "dang ky ho khau", "tach ho", "mua nha"],
            "Chuẩn bị thông tin người đăng ký, hộ tiếp nhận và địa chỉ chỗ ở để cơ quan phụ trách cư trú kiểm tra. Trong bộ mô phỏng, Sổ hộ khẩu quản lý hộ và thành viên; trường hợp chuyển đến được ghi nhận tại Biến động dân cư. Liên hệ bộ phận tiếp nhận để xác định đúng thủ tục thực tế."),
        new("Xác nhận cư trú", ["xac nhan cu tru", "xac nhan dia chi", "xac nhan noi o", "thong tin cu tru", "giay cu tru"],
            "Mô tả thông tin cư trú cần xác nhận và mục đích sử dụng. Gửi yêu cầu tại Dịch vụ công của bộ mô phỏng; cán bộ kiểm tra thông tin hộ và nhân khẩu. Phần mềm không tự cấp giấy xác nhận cư trú có giá trị pháp lý."),
        new("Chuyển đến", ["chuyen den", "chuyen ve", "den dia phuong"],
            "Kiểm tra hộ tiếp nhận, nhân khẩu, địa chỉ trước khi chuyển và ngày chuyển. Trong phần mềm, chọn Biến động dân cư → Chuyển đến; đối chiếu để tránh tạo trùng nhân khẩu. Thủ tục đăng ký cư trú thực tế do cơ quan phụ trách hướng dẫn."),
        new("Chuyển đi", ["chuyen di", "chuyen khoi", "roi dia phuong"],
            "Chuẩn bị thông tin nhân khẩu, địa chỉ đến, ngày chuyển và lý do. Trong phần mềm, chọn Biến động dân cư → Chuyển đi và kiểm tra hộ đang liên kết trước khi ghi nhận."),
        new("Hộ nghèo / cận nghèo", ["ho ngheo", "can ngheo", "giam ngheo", "ho kho khan"],
            "Mô tả hoàn cảnh hộ, thành viên và nhu cầu đề nghị rà soát. Trong phần mềm, mục An sinh xã hội quản lý phân loại hộ nghèo/cận nghèo theo hộ và thời gian áp dụng. Việc gửi yêu cầu không tự xác lập diện hưởng; cán bộ cần đối chiếu và xét duyệt."),
        new("An sinh người cao tuổi", ["nguoi cao tuoi", "nguoi gia", "cao tuoi", "tro cap tuoi gia"],
            "Chuẩn bị thông tin người cao tuổi, nhân khẩu liên quan và nhu cầu hỗ trợ. Mục An sinh xã hội → Người cao tuổi quản lý đối tượng và lịch sử trợ cấp trong bộ mô phỏng. Điều kiện, mức hưởng và giấy tờ thực tế cần được cán bộ an sinh xác nhận."),
        new("Trợ cấp an sinh xã hội", ["an sinh", "tro cap", "bao tro", "khuyet tat", "nguoi co cong", "chinh sach", "chi tra"],
            "Mô tả đối tượng và nhu cầu hỗ trợ; chuẩn bị giấy tờ liên quan để cán bộ an sinh kiểm tra. Trong phần mềm, An sinh xã hội quản lý đối tượng và lịch sử chi trả. Dữ liệu mô phỏng không xác lập mức hưởng hoặc quyết định trợ cấp thực tế."),
        new("Tiếp nhận hồ sơ dịch vụ công", ["dich vu cong", "nop ho so", "gui ho so", "nop truc tuyen", "gui yeu cau", "nop don"],
            "Mở Dịch vụ công → Thêm mới. Chọn thôn tiếp nhận, nhập thông tin người yêu cầu, loại thủ tục và nội dung; kiểm tra rồi gửi. Lưu mã hồ sơ để tìm lại trong danh sách. Gợi ý của trợ lý không tự tạo hoặc phê duyệt hồ sơ."),
        new("Tra cứu tiến độ hồ sơ", ["tien do", "trang thai ho so", "ma ho so", "tra cuu ho so", "ho so den dau", "ket qua ho so"],
            "Mở Dịch vụ công, tìm theo mã hồ sơ và xem trạng thái, cán bộ xử lý cùng ghi chú trong chi tiết. Trợ lý không truy cập hồ sơ cá nhân và không thể xác nhận một hồ sơ cụ thể đã được giải quyết."),
        new("Quản lý hộ và nhân khẩu", ["nhan khau", "ho gia dinh", "so ho khau", "thanh vien ho", "chu ho", "dan so", "thong ke dan cu"],
            "Sổ hộ khẩu quản lý hộ, chủ hộ và các thành viên; Biến động dân cư quản lý sự kiện thay đổi. Người có quyền có thể mở các phân hệ để tra cứu, hoặc xem Tổng quan để xem thống kê. Trợ lý không đọc dữ liệu cá nhân hay số liệu dân cư trực tiếp.")
    ];

    public static string Normalize(string text)
    {
        var decomposed = text.ToLowerInvariant().Replace('đ', 'd').Normalize(NormalizationForm.FormD);
        var normalized = new string(decomposed.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray());
        return Regex.Replace(normalized, "[^a-z0-9]+", " ").Trim();
    }

    public static List<Match> Search(string text)
    {
        var normalized = " " + Normalize(text) + " ";
        return Procedures.Select(p => new Match(p, p.Keywords.Where(k => normalized.Contains(" " + k + " ", StringComparison.Ordinal)).ToArray()))
            .Where(m => m.Keywords.Length > 0).OrderByDescending(m => m.Score).ToList();
    }

    public static bool IsFollowUp(string question) => Regex.IsMatch(Normalize(question),
        @"\b(giay to|ho so|bao lau|le phi|chi phi|phi|o dau|the nao|can gi|thu tuc nay|viec nay|truong hop nay|con gi|buoc tiep|can chuan bi|noi ro|chi tiet)\b");
}
