using System.ComponentModel.DataAnnotations;
using Service.TanAn.Domain.Enums;

namespace Service.Shared.Contracts.DTOs;

/// <summary>Snapshot of the declaration confirmed by the officer at registration.</summary>
public class KhaiSinhForm
{
    public KieuNhapKhaiSinh KieuNhap { get; set; }
    public LoaiNoiSinh LoaiNoiSinh { get; set; }
    [StringLength(200)] public string TenCoSoYTe { get; set; } = "";
    [StringLength(100)] public string QuocGiaNoiSinh { get; set; } = "";
    [StringLength(200)] public string BangTinhNoiSinh { get; set; } = "";
    [StringLength(200)] public string ThanhPhoNoiSinh { get; set; } = "";
    public LoaiCuTruKhaiSinh LoaiCuTruTre { get; set; }
    public LoaiCuTruKhaiSinh LoaiCuTruNguoiYeuCau { get; set; }
    public VaiTroNguoiKhaiSinh VaiTroNguoiYeuCau { get; set; }
    [StringLength(100)] public string TenGiayToKhac { get; set; } = "";
    public TinhTrangThongTinChaMe TinhTrangMe { get; set; }
    public TinhTrangThongTinChaMe TinhTrangCha { get; set; }
    public Guid RequestId { get; set; } = Guid.NewGuid();
    public Guid HoGiaDinhId { get; set; }
    public string MaSoHo { get; set; } = "";
    public string TenChuHo { get; set; } = "";
    [Required(ErrorMessage = "Nhập cơ quan đăng ký."), StringLength(200)]
    public string CoQuanDangKy { get; set; } = "UBND xã Tân An";
    [Required(ErrorMessage = "Nhập họ tên trẻ."), StringLength(200)]
    public string HoTen { get; set; } = "";
    [Required(ErrorMessage = "Nhập ngày sinh của trẻ.")]
    public DateTime? NgaySinh { get; set; }
    public string NgaySinhBangChu => NgaySinh.HasValue ? VietnameseDateText.Format(NgaySinh.Value) : "";
    public GioiTinhEnum GioiTinh { get; set; } = GioiTinhEnum.Nam;
    [StringLength(12), RegularExpression(@"^$|^[0-9]{12}$", ErrorMessage = "Số định danh cá nhân gồm 12 chữ số; để trống nếu chưa có.")]
    public string SoDinhDanh { get; set; } = "";
    [Required(ErrorMessage = "Nhập nơi sinh."), StringLength(500)]
    public string NoiSinh { get; set; } = "";
    [Required(ErrorMessage = "Nhập dân tộc của trẻ."), StringLength(100)]
    public string DanToc { get; set; } = "Kinh";
    [Required(ErrorMessage = "Nhập quốc tịch của trẻ."), StringLength(100)]
    public string QuocTich { get; set; } = "Việt Nam";
    [Required(ErrorMessage = "Nhập quê quán của trẻ."), StringLength(500)]
    public string QueQuan { get; set; } = "";
    [Required(ErrorMessage = "Nhập địa chỉ thường trú của trẻ."), StringLength(500)]
    public string ThuongTru { get; set; } = "";
    [Required(ErrorMessage = "Nhập quan hệ của trẻ với chủ hộ."), StringLength(100)]
    public string QuanHeVoiChuHo { get; set; } = "";
    public Guid? NguoiYeuCauId { get; set; }
    [Required(ErrorMessage = "Nhập họ tên người yêu cầu."), StringLength(200)]
    public string HoTenNguoiYeuCau { get; set; } = "";
    [Required, StringLength(50)]
    public string LoaiGiayTo { get; set; } = "CCCD";
    [Required(ErrorMessage = "Nhập số giấy tờ của người yêu cầu."), StringLength(100)]
    public string SoGiayTo { get; set; } = "";
    public DateTime? NgayCapGiayTo { get; set; }
    [StringLength(200)] public string NoiCapGiayTo { get; set; } = "";
    [Required(ErrorMessage = "Nhập nơi cư trú người yêu cầu."), StringLength(500)]
    public string NoiCuTruNguoiYeuCau { get; set; } = "";
    [Required(ErrorMessage = "Nhập quan hệ của người yêu cầu với trẻ."), StringLength(100)]
    public string QuanHeVoiTre { get; set; } = "";
    public KhaiSinhParentForm? Me { get; set; }
    public KhaiSinhParentForm? Cha { get; set; }
    [Required(ErrorMessage = "Nhập ngày đăng ký.")]
    public DateTime? NgayDangKy { get; set; } = DateTime.Today;
    [StringLength(100)] public string SoGiayChungSinh { get; set; } = "";
    [StringLength(2000)] public string GhiChu { get; set; } = "";
}

public class KhaiSinhParentForm
{
    public Guid? NhanKhauId { get; set; }
    [Required(ErrorMessage = "Nhập họ tên cha/mẹ hoặc bỏ chọn mục thông tin này."), StringLength(200)]
    public string HoTen { get; set; } = "";
    public DateTime? NgaySinh { get; set; }
    [StringLength(100)] public string DanToc { get; set; } = "Kinh";
    [StringLength(100)] public string QuocTich { get; set; } = "Việt Nam";
    [StringLength(500)] public string NoiCuTru { get; set; } = "";
}

public static class VietnameseDateText
{
    private static readonly string[] Digits = ["không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín"];
    public static string Format(DateTime date) => $"Ngày {Number(date.Day)} tháng {Number(date.Month)} năm {Number(date.Year)}";
    private static string Number(int value, bool fullHundreds = false)
    {
        if (value >= 1000)
            return Number(value / 1000) + " nghìn" + (value % 1000 == 0 ? "" : " " + Number(value % 1000, true));
        var hundreds = value / 100;
        var remainder = value % 100;
        var prefix = hundreds > 0 || fullHundreds ? Digits[hundreds] + " trăm" : "";
        if (remainder == 0) return prefix.Length > 0 ? prefix : Digits[0];
        var tens = remainder / 10;
        var units = remainder % 10;
        var tail = tens == 0 ? (prefix.Length > 0 ? "linh " : "") + Digits[units]
            : tens == 1 ? "mười" + (units == 0 ? "" : " " + (units == 5 ? "lăm" : Digits[units]))
            : Digits[tens] + " mươi" + (units == 0 ? "" : " " + (units == 1 ? "mốt" : units == 5 ? "lăm" : Digits[units]));
        return prefix.Length > 0 ? prefix + " " + tail : tail;
    }
}
