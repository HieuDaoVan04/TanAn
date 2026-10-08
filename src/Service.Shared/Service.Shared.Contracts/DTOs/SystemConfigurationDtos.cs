using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Service.Shared.Contracts.DTOs;

public enum ParameterDataType { Text, Boolean, Integer }

public sealed record SystemParameterDefinition(string Code, string Name, string Group,
    ParameterDataType DataType, string DefaultValue, int? Minimum = null, int? Maximum = null,
    int MaxLength = 2000, bool Required = false);

/// <summary>Chỉ khai báo các tham số đã có nơi sử dụng trong Tân An. Không chứa khóa bí mật.</summary>
public static class SystemParameterCatalog
{
    public static IReadOnlyList<SystemParameterDefinition> Definitions { get; } = Array.AsReadOnly(new[]
    {
        new SystemParameterDefinition("AppName", "Tên ứng dụng", "Thông tin ứng dụng", ParameterDataType.Text, "Quản trị nội dung - Cổng thông tin & CSDL UBND Xã Tân An", MaxLength: 200, Required: true),
        new SystemParameterDefinition("AppVersion", "Phiên bản hệ thống", "Thông tin ứng dụng", ParameterDataType.Text, "V1.1", MaxLength: 50, Required: true),
        new SystemParameterDefinition("SupportEmail", "Email hỗ trợ", "Thông tin ứng dụng", ParameterDataType.Text, "", MaxLength: 254),
        new SystemParameterDefinition("Hotline", "Điện thoại hỗ trợ", "Thông tin ứng dụng", ParameterDataType.Text, "", MaxLength: 30),
        new SystemParameterDefinition("HeaderEnabled", "Hiển thị thông báo đầu trang", "Banner và footer", ParameterDataType.Boolean, "false"),
        new SystemParameterDefinition("HeaderContent", "Nội dung thông báo đầu trang", "Banner và footer", ParameterDataType.Text, ""),
        new SystemParameterDefinition("FooterEnabled", "Hiển thị footer", "Banner và footer", ParameterDataType.Boolean, "true"),
        new SystemParameterDefinition("FooterContent", "Nội dung footer", "Banner và footer", ParameterDataType.Text, "Cổng CSDL & Hệ Thống Quản Lý UBND Xã Tân An"),
        new SystemParameterDefinition("PasswordMinLength", "Độ dài mật khẩu tối thiểu", "Chính sách mật khẩu", ParameterDataType.Integer, "8", 8, 128),
        new SystemParameterDefinition("PasswordRequireUppercase", "Bắt buộc có chữ hoa", "Chính sách mật khẩu", ParameterDataType.Boolean, "true"),
        new SystemParameterDefinition("PasswordRequireLowercase", "Bắt buộc có chữ thường", "Chính sách mật khẩu", ParameterDataType.Boolean, "true"),
        new SystemParameterDefinition("PasswordRequireDigit", "Bắt buộc có chữ số", "Chính sách mật khẩu", ParameterDataType.Boolean, "true"),
        new SystemParameterDefinition("PasswordRequireSpecialChar", "Bắt buộc có ký tự đặc biệt", "Chính sách mật khẩu", ParameterDataType.Boolean, "true"),
        new SystemParameterDefinition("MinuteExpireToken", "Thời hạn phiên đăng nhập mới (phút)", "Đăng nhập", ParameterDataType.Integer, "60", 5, 1440),
        new SystemParameterDefinition("KhoaTaiKhoan", "Số lần đăng nhập sai liên tiếp trước khi khóa", "Đăng nhập", ParameterDataType.Integer, "5", 3, 20),
        new SystemParameterDefinition("LoginLockoutMinutes", "Thời gian khóa tài khoản (phút)", "Đăng nhập", ParameterDataType.Integer, "15", 1, 1440)
    });

    public static SystemParameterDefinition? Find(string? code) => Definitions.FirstOrDefault(
        x => x.Code.Equals(code?.Trim(), StringComparison.OrdinalIgnoreCase));

    public static string NormalizeCode(string? code)
    {
        var value = code?.Trim() ?? "";
        if (value.Length is < 1 or > 50 || value.Any(c => !char.IsAsciiLetterOrDigit(c) && c != '_' && c != '-'))
            throw new ArgumentException("Mã tham số phải có 1–50 ký tự: chữ không dấu, số, _ hoặc -.");
        return Find(value)?.Code ?? value;
    }

    public static string NormalizeValue(string code, string? value)
    {
        var definition = Find(code);
        var text = value?.Trim() ?? "";
        if (text.Length > (definition?.MaxLength ?? 2000)) throw new ArgumentException($"Giá trị {code} quá dài.");
        if (definition?.Required == true && text.Length == 0) throw new ArgumentException($"{definition.Name} không được để trống.");
        if (definition?.DataType == ParameterDataType.Boolean)
        {
            if (!bool.TryParse(text, out var boolean)) throw new ArgumentException($"{definition.Name} phải là true hoặc false.");
            return boolean ? "true" : "false";
        }
        if (definition?.DataType == ParameterDataType.Integer)
        {
            if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number)
                || number < definition.Minimum || number > definition.Maximum)
                throw new ArgumentException($"{definition.Name} phải là số nguyên từ {definition.Minimum} đến {definition.Maximum}.");
            return number.ToString(CultureInfo.InvariantCulture);
        }
        if (code == "SupportEmail" && text.Length > 0 && !new EmailAddressAttribute().IsValid(text))
            throw new ArgumentException("Email hỗ trợ không hợp lệ.");
        return text;
    }
}

public sealed class SystemConfigurationField
{
    public Guid Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Group { get; set; } = "";
    public ParameterDataType DataType { get; set; }
    public string Value { get; set; } = "";
    public string DefaultValue { get; set; } = "";
    public int? Minimum { get; set; }
    public int? Maximum { get; set; }
    public int MaxLength { get; set; }
    public bool Required { get; set; }
    public bool IsDefault { get; set; }
}

public sealed class SystemConfigurationUpdate
{
    public string Code { get; set; } = "";
    public string? Value { get; set; }
    // Giá trị hiệu lực khi form được tải, dùng để phát hiện cấu hình đã bị người khác sửa.
    public string? ExpectedValue { get; set; }
}

public sealed class PasswordPolicyDto
{
    public int MinLength { get; set; } = 8;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireDigit { get; set; } = true;
    public bool RequireSpecialChar { get; set; } = true;

    public void Validate(string? password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < MinLength || password.Length > 128)
            throw new ArgumentException($"Mật khẩu phải có từ {MinLength} đến 128 ký tự.");
        if (RequireUppercase && !password.Any(char.IsUpper)) throw new ArgumentException("Mật khẩu phải có chữ hoa.");
        if (RequireLowercase && !password.Any(char.IsLower)) throw new ArgumentException("Mật khẩu phải có chữ thường.");
        if (RequireDigit && !password.Any(char.IsDigit)) throw new ArgumentException("Mật khẩu phải có chữ số.");
        if (RequireSpecialChar && !password.Any(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c)))
            throw new ArgumentException("Mật khẩu phải có ký tự đặc biệt.");
    }
}

public sealed class PublicSystemConfigurationDto
{
    public string AppName { get; set; } = "";
    public string AppVersion { get; set; } = "";
    public string SupportEmail { get; set; } = "";
    public string Hotline { get; set; } = "";
    public bool HeaderEnabled { get; set; }
    public string HeaderContent { get; set; } = "";
    public bool FooterEnabled { get; set; }
    public string FooterContent { get; set; } = "";
}

public sealed class ChangeOwnPasswordForm
{
    public string CurrentPassword { get; set; } = "";
    public string NewPassword { get; set; } = "";
}
