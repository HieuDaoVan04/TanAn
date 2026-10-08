using System.ComponentModel.DataAnnotations;
using Service.TanAn.Domain.Enums;

namespace Service.Shared.Contracts.DTOs;

public static class KhaiSinhDraftValidation
{
    // Nháp được thiếu dữ liệu; mọi giá trị đã nhập vẫn phải đúng định dạng.
    public static IReadOnlyList<string> Errors(KhaiSinhForm form)
    {
        var errors = new List<string>();
        Validate(form);
        if (form.Me != null) Validate(form.Me);
        if (form.Cha != null) Validate(form.Cha);
        CheckDate(form.NgaySinh, "Ngày sinh trẻ");
        CheckDate(form.NgayCapGiayTo, "Ngày cấp giấy tờ");
        CheckDate(form.NgayDangKy, "Ngày đăng ký");
        foreach (var parent in new[] { form.Me, form.Cha }.OfType<KhaiSinhParentForm>())
        {
            CheckDate(parent.NgaySinh, "Ngày sinh cha/mẹ");
            if (parent.NgaySinh.HasValue && form.NgaySinh.HasValue && parent.NgaySinh.Value.Date >= form.NgaySinh.Value.Date)
                errors.Add("Ngày sinh cha/mẹ phải trước ngày sinh của trẻ.");
        }
        if (form.NgayDangKy.HasValue && form.NgaySinh.HasValue && form.NgayDangKy.Value.Date < form.NgaySinh.Value.Date)
            errors.Add("Ngày đăng ký không được trước ngày sinh.");
        if (form.NgayCapGiayTo.HasValue && form.NgayDangKy.HasValue && form.NgayCapGiayTo.Value.Date > form.NgayDangKy.Value.Date)
            errors.Add("Ngày cấp giấy tờ không được sau ngày đăng ký.");
        if (form.RequestId == Guid.Empty) errors.Add("Mã hồ sơ không hợp lệ.");
        if (form.Me?.NhanKhauId.HasValue == true && form.Me.NhanKhauId == form.Cha?.NhanKhauId)
            errors.Add("Cha và mẹ không thể là cùng một nhân khẩu.");
        return errors.Distinct().ToList();

        void CheckDate(DateTime? date, string label)
        {
            if (date.HasValue && date.Value.Date > DateTime.Today) errors.Add($"{label} không được nằm trong tương lai.");
        }
        void Validate(object value)
        {
            foreach (var property in value.GetType().GetProperties())
            {
                var data = property.GetValue(value);
                foreach (var rule in property.GetCustomAttributes(typeof(ValidationAttribute), true).Cast<ValidationAttribute>().Where(x => x is not RequiredAttribute))
                    if (!rule.IsValid(data)) errors.Add(rule.FormatErrorMessage(property.Name));
                if (property.PropertyType.IsEnum && data != null && !Enum.IsDefined(property.PropertyType, data))
                    errors.Add($"Giá trị {property.Name} không hợp lệ.");
            }
        }
    }
}
