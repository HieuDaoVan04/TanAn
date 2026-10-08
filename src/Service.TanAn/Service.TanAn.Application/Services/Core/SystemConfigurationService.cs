using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Service.Shared.Commons.Model.SQL;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using Service.TanAn.Application.Interfaces;
using Service.TanAn.Domain.Entities;

namespace Service.TanAn.Application.Services.Core;

/// <summary>Dùng chung cho API cũ, danh mục quản trị và các nơi đọc cấu hình hiệu lực.</summary>
public sealed class SystemConfigurationService(ITanAnDbContext db, IHttpContextAccessor http)
{
    private static void RequireAdmin(CurrentUserDto actor)
    {
        if (!actor.IsAuthenticated || actor.Role != "Admin") throw new UnauthorizedAccessException("Chỉ quản trị viên được cấu hình hệ thống.");
    }

    public static async Task<Dictionary<string, string>> ReadValuesAsync(ITanAnDbContext db)
    {
        var rows = await db.SystemParameters.AsNoTracking().ToListAsync();
        return EffectiveValues(rows);
    }

    private static Dictionary<string, string> EffectiveValues(List<SystemParameter> rows)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var definition in SystemParameterCatalog.Definitions)
        {
            values[definition.Code] = definition.DefaultValue;
            var matches = rows.Where(x => x.Code.Trim().Equals(definition.Code, StringComparison.OrdinalIgnoreCase)).ToList();
            if (matches.Count != 1 || matches[0].ModerationStatus != ModerationStatus.Approved) continue;
            try { values[definition.Code] = SystemParameterCatalog.NormalizeValue(definition.Code, matches[0].Value); }
            catch (ArgumentException) { /* Cấu hình cũ không hợp lệ: dùng mặc định có kiểm tra. */ }
        }
        return values;
    }

    public async Task<List<SystemConfigurationField>> GetGeneralAsync(CurrentUserDto actor)
    {
        RequireAdmin(actor);
        var rows = await db.SystemParameters.AsNoTracking().ToListAsync();
        var values = EffectiveValues(rows);
        return SystemParameterCatalog.Definitions.Select(d => new SystemConfigurationField
        {
            Id = rows.FirstOrDefault(x => x.Code.Trim().Equals(d.Code, StringComparison.OrdinalIgnoreCase))?.Id ?? Guid.Empty,
            Code = d.Code, Name = d.Name, Group = d.Group, DataType = d.DataType,
            Value = values[d.Code], DefaultValue = d.DefaultValue, Minimum = d.Minimum, Maximum = d.Maximum,
            MaxLength = d.MaxLength, Required = d.Required,
            IsDefault = UsesDefault(d, rows)
        }).ToList();
    }

    private static bool UsesDefault(SystemParameterDefinition definition, List<SystemParameter> rows)
    {
        var matches = rows.Where(x => x.Code.Trim().Equals(definition.Code, StringComparison.OrdinalIgnoreCase)).ToList();
        if (matches.Count != 1 || matches[0].ModerationStatus != ModerationStatus.Approved) return true;
        try { SystemParameterCatalog.NormalizeValue(definition.Code, matches[0].Value); return false; }
        catch (ArgumentException) { return true; }
    }

    public async Task SaveGeneralAsync(List<SystemConfigurationUpdate> updates, CurrentUserDto actor)
    {
        RequireAdmin(actor);
        if (updates == null || updates.Count is < 1 or > 50 || updates.Any(x => x == null)) throw new ArgumentException("Danh sách cấu hình không hợp lệ.");
        var normalized = updates.Select(x =>
        {
            var code = SystemParameterCatalog.NormalizeCode(x.Code);
            return new SystemConfigurationUpdate { Code = code,
                Value = SystemParameterCatalog.NormalizeValue(code, x.Value), ExpectedValue = x.ExpectedValue };
        }).ToList();
        if (normalized.Any(x => SystemParameterCatalog.Find(x.Code) == null)) throw new ArgumentException("Cấu hình chung chỉ nhận các mã tham số đã định nghĩa.");
        if (normalized.Select(x => x.Code).Distinct(StringComparer.OrdinalIgnoreCase).Count() != normalized.Count)
            throw new ArgumentException("Danh sách chứa mã tham số trùng lặp.");

        var rows = await db.SystemParameters.ToListAsync();
        var values = EffectiveValues(rows);
        // Kiểm tra cả danh sách trước khi sửa entity: không lưu dở dang khi một trường lỗi.
        foreach (var update in normalized)
        {
            if (rows.Count(x => x.Code.Trim().Equals(update.Code, StringComparison.OrdinalIgnoreCase)) > 1)
                throw new InvalidOperationException($"Mã {update.Code} bị trùng trong dữ liệu cũ. Hãy xử lý bản ghi trùng trước khi lưu.");
            if (update.ExpectedValue != null && update.ExpectedValue != values[update.Code])
                throw new InvalidOperationException($"{update.Code} đã thay đổi. Hãy tải lại cấu hình trước khi lưu.");
        }
        foreach (var update in normalized)
        {
            var row = rows.SingleOrDefault(x => x.Code.Trim().Equals(update.Code, StringComparison.OrdinalIgnoreCase));
            var before = row == null ? null : Snapshot(row);
            if (row == null)
            {
                row = new SystemParameter { Code = update.Code, IsSync = true, Description = SystemParameterCatalog.Find(update.Code)!.Name };
                db.SystemParameters.Add(row);
            }
            else if (row.Value == update.Value && row.ModerationStatus == ModerationStatus.Approved && row.Code == update.Code) continue;
            row.Code = update.Code; row.Value = update.Value; row.ModerationStatus = ModerationStatus.Approved; row.LastModified = DateTime.UtcNow;
            Audit(actor, "Cập nhật cấu hình chung", row, before);
        }
        await db.SaveChangesAsync();
    }

    public async Task<Guid> SaveParameterAsync(SystemParameterForm form, CurrentUserDto actor, ModerationStatus? status = null)
    {
        RequireAdmin(actor);
        var code = SystemParameterCatalog.NormalizeCode(form.Code);
        var value = SystemParameterCatalog.NormalizeValue(code, form.Value);
        if (form.Description?.Length > 2000) throw new ArgumentException("Mô tả quá dài (tối đa 2000 ký tự).");
        var rows = await db.SystemParameters.ToListAsync();
        if (rows.Any(x => x.Id != form.Id && x.Code.Trim().Equals(code, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException("Mã tham số đã tồn tại.");
        var row = form.Id == Guid.Empty ? null : rows.SingleOrDefault(x => x.Id == form.Id) ?? throw new KeyNotFoundException("Tham số không tồn tại.");
        if (row != null && SystemParameterCatalog.Find(row.Code) != null && !row.Code.Trim().Equals(code, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Không được đổi mã tham số hệ thống đã định nghĩa.");
        var before = row == null ? null : Snapshot(row);
        if (row == null) { row = new SystemParameter(); db.SystemParameters.Add(row); }
        row.Code = code; row.Value = value; row.Description = form.Description?.Trim();
        row.IsSync = SystemParameterCatalog.Find(code) != null;
        row.ModerationStatus = (status ?? row.ModerationStatus) == ModerationStatus.Approved ? ModerationStatus.Approved : ModerationStatus.Pending; row.LastModified = DateTime.UtcNow;
        Audit(actor, before == null ? "Tạo tham số hệ thống" : "Cập nhật tham số hệ thống", row, before);
        await db.SaveChangesAsync();
        return row.Id;
    }

    public async Task<bool> ChangeStatusAsync(Guid id, ModerationStatus status, CurrentUserDto actor)
    {
        RequireAdmin(actor);
        if (status is not (ModerationStatus.Approved or ModerationStatus.Rejected or ModerationStatus.Pending)) throw new ArgumentException("Trạng thái không hợp lệ.");
        var row = await db.SystemParameters.FindAsync(id);
        if (row == null) return false;
        if (status == ModerationStatus.Approved) SystemParameterCatalog.NormalizeValue(SystemParameterCatalog.NormalizeCode(row.Code), row.Value);
        var before = Snapshot(row);
        row.ModerationStatus = status == ModerationStatus.Approved ? ModerationStatus.Approved : ModerationStatus.Pending; row.LastModified = DateTime.UtcNow;
        Audit(actor, status == ModerationStatus.Approved ? "Duyệt tham số hệ thống" : "Hủy duyệt tham số hệ thống", row, before);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CurrentUserDto actor)
    {
        RequireAdmin(actor);
        var row = await db.SystemParameters.FindAsync(id);
        if (row == null) return false;
        if (SystemParameterCatalog.Find(row.Code) != null) throw new InvalidOperationException("Không được xóa tham số hệ thống đã định nghĩa. Có thể hủy duyệt để trở về giá trị mặc định.");
        var before = Snapshot(row);
        db.SystemParameters.Remove(row);
        Audit(actor, "Xóa tham số hệ thống", row, before, deleted: true);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task SyncDefinitionsAsync(CurrentUserDto actor)
    {
        RequireAdmin(actor);
        var existing = await db.SystemParameters.AsNoTracking().Select(x => x.Code).ToListAsync();
        var missing = SystemParameterCatalog.Definitions.Where(d => !existing.Any(x => x.Trim().Equals(d.Code, StringComparison.OrdinalIgnoreCase)))
            .Select(d => new SystemConfigurationUpdate { Code = d.Code, Value = d.DefaultValue }).ToList();
        if (missing.Count > 0) await SaveGeneralAsync(missing, actor);
    }

    public async Task<PublicSystemConfigurationDto> GetPublicAsync()
    {
        var v = await ReadValuesAsync(db);
        // Danh sách trường cố định; không trả tham số tùy chỉnh hoặc bí mật qua API công khai.
        return new PublicSystemConfigurationDto
        {
            AppName = v["AppName"], AppVersion = v["AppVersion"], SupportEmail = v["SupportEmail"], Hotline = v["Hotline"],
            HeaderEnabled = bool.Parse(v["HeaderEnabled"]), HeaderContent = v["HeaderContent"],
            FooterEnabled = bool.Parse(v["FooterEnabled"]), FooterContent = v["FooterContent"]
        };
    }

    public async Task<PasswordPolicyDto> GetPasswordPolicyAsync() => PasswordPolicy(await ReadValuesAsync(db));
    public async Task<(int SessionMinutes, int MaxFailedAttempts, int LockoutMinutes)> GetLoginPolicyAsync()
    {
        var v = await ReadValuesAsync(db);
        return (int.Parse(v["MinuteExpireToken"]), int.Parse(v["KhoaTaiKhoan"]), int.Parse(v["LoginLockoutMinutes"]));
    }
    public static PasswordPolicyDto PasswordPolicy(IReadOnlyDictionary<string, string> v) => new()
    {
        MinLength = int.Parse(v["PasswordMinLength"]), RequireUppercase = bool.Parse(v["PasswordRequireUppercase"]),
        RequireLowercase = bool.Parse(v["PasswordRequireLowercase"]), RequireDigit = bool.Parse(v["PasswordRequireDigit"]),
        RequireSpecialChar = bool.Parse(v["PasswordRequireSpecialChar"])
    };

    private static string Snapshot(SystemParameter row) => JsonSerializer.Serialize(new
    {
        row.Id, row.Code, row.Description, row.ModerationStatus,
        Value = SystemParameterCatalog.Find(row.Code) != null ? row.Value : "[Ẩn giá trị tham số tùy chỉnh]"
    });
    private void Audit(CurrentUserDto actor, string action, SystemParameter row, string? before, bool deleted = false) => db.AuditLogs.Add(new AuditLog
    {
        Username = actor.UserName, Action = action, EntityName = "Parameters", EntityId = row.Id.ToString(),
        OldValues = before, NewValues = deleted ? "{\"Deleted\":true}" : Snapshot(row), IpAddress = http.HttpContext?.Connection.RemoteIpAddress?.ToString()
    });
}
