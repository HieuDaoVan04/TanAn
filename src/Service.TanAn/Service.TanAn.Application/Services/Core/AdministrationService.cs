using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Service.Shared.Commons.Helpers;
using Service.Shared.Commons.Model.SQL;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using Service.TanAn.Application.Interfaces;
using Service.TanAn.Domain.Entities;

namespace Service.TanAn.Application.Services.Core;

/// <summary>Nghiệp vụ danh mục quản trị. UI mở scope riêng cho mỗi thao tác.</summary>
public sealed class AdministrationService(ITanAnDbContext db, IHttpContextAccessor httpContextAccessor)
{
    private static void RequireAdmin(CurrentUserDto user)
    {
        if (!user.IsAuthenticated || user.Role != "Admin") throw new UnauthorizedAccessException("Chỉ quản trị viên được quản lý danh mục hệ thống.");
    }
    public async Task<List<AdminRecord>> ListAsync(AdminCatalog catalog, CurrentUserDto actor)
    {
        RequireAdmin(actor);
        return catalog switch
        {
            AdminCatalog.Modules => await db.PhanHes.AsNoTracking().OrderBy(x => x.Ma).Select(x => new AdminRecord { Id = x.Id, Code = x.Ma, Name = x.Ten, Description = x.MoTa, Active = x.HoatDong }).ToListAsync(),
            AdminCatalog.Menus => await db.Modules.AsNoTracking().OrderBy(x => x.ViTri).Select(x => new AdminRecord { Id = x.Id, Name = x.TenModule, Path = x.LienKet, Icon = x.Icon, Order = x.ViTri, Expanded = x.Expands, ParentId = x.ModuleChaId, ModuleId = x.PhanHeId, ModerationStatus = x.ModerationStatus, Active = x.ModerationStatus == ModerationStatus.Approved }).ToListAsync(),
            AdminCatalog.Users => await db.Users.AsNoTracking().OrderBy(x => x.UserName).Select(x => new AdminRecord { Id = x.Id, Code = x.UserName, Name = x.FullName, Email = x.Email, Phone = x.PhoneNumber, AccountRole = x.Role, VillageIds = db.PhuTrachThons.Where(p => p.UserId == x.Id).Select(p => p.ApThonId).ToList(), ModerationStatus = x.ModerationStatus, Active = x.ModerationStatus == ModerationStatus.Approved, AssignedIds = db.UserRoles.Where(r => r.UserId == x.Id).Select(r => r.RoleId).ToList() }).ToListAsync(),
            AdminCatalog.Roles => await db.Roles.AsNoTracking().OrderBy(x => x.RoleName).Select(x => new AdminRecord { Id = x.Id, Code = x.RoleCode, Name = x.RoleName, Description = x.Mota, ModerationStatus = x.ModerationStatus, Active = x.ModerationStatus == ModerationStatus.Approved, AssignedIds = db.RoleModules.Where(r => r.RoleId == x.Id).Select(r => r.ModuleId).ToList() }).ToListAsync(),
            AdminCatalog.Groups => await db.Groups.AsNoTracking().OrderBy(x => x.GroupName).Select(x => new AdminRecord { Id = x.Id, Code = x.GroupCode, Name = x.GroupName, Description = x.Description, ParentId = x.ParentId, UnitType = x.UnitType, Active = x.IsActive }).ToListAsync(),
            AdminCatalog.Parameters => (await db.SystemParameters.AsNoTracking().OrderBy(x => x.Code).ToListAsync()).Select(x => new AdminRecord { Id = x.Id, Code = x.Code, Name = SystemParameterCatalog.Find(x.Code)?.Name ?? x.Code, Description = x.Description, Path = x.Value, ModerationStatus = x.ModerationStatus, Active = x.ModerationStatus == ModerationStatus.Approved }).ToList(),
            _ => throw new ArgumentOutOfRangeException(nameof(catalog))
        };
    }

    public async Task SaveAsync(AdminCatalog catalog, AdminRecord form, CurrentUserDto actor)
    {
        RequireAdmin(actor);
        if (catalog == AdminCatalog.Parameters)
        {
            var current = await db.SystemParameters.AsNoTracking().SingleOrDefaultAsync(x => x.Id == form.Id);
            var parameterStatus = current != null && form.Active == (current.ModerationStatus == ModerationStatus.Approved)
                ? current.ModerationStatus : form.Active ? ModerationStatus.Approved : ModerationStatus.Pending;
            await new SystemConfigurationService(db, httpContextAccessor).SaveParameterAsync(new SystemParameterForm
            { Id = form.Id, Code = form.Code, Value = form.Path, Description = form.Description }, actor,
                parameterStatus);
            return;
        }
        var isCreate = form.Id == Guid.Empty;
        form.Code = form.Code.Trim(); form.Name = form.Name.Trim();
        if (catalog == AdminCatalog.Parameters) form.Name = form.Code;
        if (string.IsNullOrWhiteSpace(form.Name) || form.Name.Length > 200) throw new ArgumentException("Tên phải có từ 1 đến 200 ký tự.");
        if (catalog != AdminCatalog.Menus && (string.IsNullOrWhiteSpace(form.Code) || form.Code.Length > 50)) throw new ArgumentException("Mã/tài khoản phải có từ 1 đến 50 ký tự.");
        var records = await ListAsync(catalog, actor);
        if (form.Id != Guid.Empty && !records.Any(x => x.Id == form.Id)) throw new KeyNotFoundException("Bản ghi không còn tồn tại.");
        if (catalog != AdminCatalog.Menus && records.Any(x => x.Id != form.Id && x.Code.Equals(form.Code, StringComparison.OrdinalIgnoreCase))) throw new ArgumentException("Mã/tài khoản đã tồn tại.");
        var previous = records.FirstOrDefault(x => x.Id == form.Id);
        var status = previous?.Active == form.Active && previous.ModerationStatus.HasValue
            ? previous.ModerationStatus.Value
            : form.Active ? ModerationStatus.Approved : ModerationStatus.Pending;
        var id = form.Id == Guid.Empty ? Guid.NewGuid() : form.Id;
        if (catalog is AdminCatalog.Menus or AdminCatalog.Groups) ValidateParent(records, id, form.ParentId);
        switch (catalog)
        {
            case AdminCatalog.Modules:
            {
                var x = await db.PhanHes.FindAsync(id);
                if (x == null) { x = new PhanHe { Id = id }; db.PhanHes.Add(x); }
                x.Ma = form.Code; x.Ten = form.Name; x.MoTa = form.Description; x.HoatDong = form.Active;
                break;
            }
            case AdminCatalog.Menus:
            {
                var path = string.IsNullOrWhiteSpace(form.Path) ? null : form.Path.Trim();
                if (path != null && (!path.StartsWith('/') || path.StartsWith("//") || path.Contains('\\'))) throw new ArgumentException("Liên kết phải là đường dẫn nội bộ bắt đầu bằng /.");
                if (path != null && records.Any(x => x.Id != id && x.Path == path)) throw new ArgumentException("Liên kết menu đã tồn tại.");
                if (form.ModuleId.HasValue && !await db.PhanHes.AnyAsync(x => x.Id == form.ModuleId)) throw new ArgumentException("Module không tồn tại.");
                if (form.Order < 0) throw new ArgumentException("Vị trí không được âm.");
                var x = await db.Modules.FindAsync(id);
                if (x == null) { x = new Module { Id = id, PhanLoai = EnumModuleType.NghiepVu }; db.Modules.Add(x); }
                x.TenModule = form.Name; x.LienKet = path; x.Icon = form.Icon; x.ModuleChaId = form.ParentId;
                x.PhanHeId = form.ModuleId; x.ViTri = form.Order; x.Expands = form.Expanded; x.ModerationStatus = status;
                break;
            }
            case AdminCatalog.Roles:
            {
                var ids = form.AssignedIds.Distinct().ToList();
                if (await db.Modules.CountAsync(x => ids.Contains(x.Id)) != ids.Count) throw new ArgumentException("Danh sách menu chứa bản ghi không tồn tại.");
                var x = await db.Roles.FindAsync(id);
                if (x == null) { x = new Role { Id = id }; db.Roles.Add(x); }
                x.RoleCode = form.Code; x.RoleName = form.Name; x.Mota = form.Description; x.ModerationStatus = status; x.LastModified = DateTime.UtcNow;
                var old = await db.RoleModules.Where(r => r.RoleId == id).ToListAsync();
                db.RoleModules.RemoveRange(old.Where(r => !ids.Contains(r.ModuleId)));
                db.RoleModules.AddRange(ids.Except(old.Select(r => r.ModuleId)).Select(menuId => new RoleModule { RoleId = id, ModuleId = menuId }));
                break;
            }
            case AdminCatalog.Users:
            {
                var ids = form.AssignedIds.Distinct().ToList();
                if (await db.Roles.CountAsync(x => ids.Contains(x.Id)) != ids.Count) throw new ArgumentException("Vai trò không tồn tại.");
                if (form.Id == Guid.Empty || form.Password.Length > 0)
                    (await new SystemConfigurationService(db, httpContextAccessor).GetPasswordPolicyAsync()).Validate(form.Password);
                if (!string.IsNullOrWhiteSpace(form.Email) && !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(form.Email)) throw new ArgumentException("Email không hợp lệ.");
                var x = await db.Users.FindAsync(id);
                if (x != null && x.UserName.Equals(actor.UserName, StringComparison.OrdinalIgnoreCase) && (!form.Active || x.UserName != form.Code)) throw new ArgumentException("Không thể khóa hoặc đổi tên tài khoản đang sử dụng.");
                if (x == null) { x = new User { Id = id }; db.Users.Add(x); }
                x.UserName = form.Code; x.FullName = form.Name; x.Email = form.Email.Trim(); x.PhoneNumber = form.Phone; x.ModerationStatus = status; x.LastModified = DateTime.UtcNow;
                if (form.Password.Length > 0) x.PasswordHash = PasswordHashing.Hash(form.Password);
                if (!Enum.IsDefined(form.AccountRole)) throw new ArgumentException("Vai trò tài khoản không hợp lệ.");
                if (id == actor.UserId && form.AccountRole != x.Role) throw new ArgumentException("Không được tự thay đổi cấp quyền của tài khoản đang đăng nhập.");
                var villages = form.VillageIds.Distinct().ToList();
                if (form.AccountRole != Service.TanAn.Domain.Enums.RoleEnum.CanBoThon) villages.Clear();
                if (form.AccountRole == Service.TanAn.Domain.Enums.RoleEnum.CanBoThon && villages.Count == 0) throw new ArgumentException("Hãy chọn ít nhất một thôn phụ trách.");
                if (await db.ApThons.CountAsync(t => villages.Contains(t.Id) && t.DangHoatDong && t.XaId != null) != villages.Count) throw new ArgumentException("Thôn không tồn tại, chưa thuộc xã hoặc đã ngừng hoạt động.");
                x.Role = form.AccountRole;
                var oldVillages = await db.PhuTrachThons.Where(p => p.UserId == id).ToListAsync();
                db.PhuTrachThons.RemoveRange(oldVillages.Where(p => !villages.Contains(p.ApThonId)));
                foreach (var villageId in villages.Except(oldVillages.Select(p => p.ApThonId)))
                {
                    db.PhuTrachThons.Add(new PhuTrachThon { UserId = id, ApThonId = villageId });
                    db.ThongBaoThons.Add(new ThongBaoThon { NguoiNhanId = id, ApThonId = villageId, TieuDe = "Phân công phụ trách thôn", NoiDung = "Bạn được giao quản lý dân cư, an sinh và hồ sơ trong thôn này.", NguoiTaoId = actor.UserId });
                }
                var old = await db.UserRoles.Where(r => r.UserId == id).ToListAsync();
                db.UserRoles.RemoveRange(old.Where(r => !ids.Contains(r.RoleId)));
                db.UserRoles.AddRange(ids.Except(old.Select(r => r.RoleId)).Select(roleId => new UserRole { UserId = id, RoleId = roleId }));
                break;
            }
            case AdminCatalog.Groups:
            {
                var x = await db.Groups.FindAsync(id);
                if (x == null) { x = new Groups { Id = id }; db.Groups.Add(x); }
                if (form.UnitType is not (0 or 1)) throw new ArgumentException("Loại đơn vị/hệ thống không hợp lệ.");
                x.UnitType = form.UnitType; x.GroupCode = form.Code; x.GroupName = form.Name; x.Description = form.Description; x.ParentId = form.ParentId; x.IsActive = form.Active;
                break;
            }
        }
        db.AuditLogs.Add(new AuditLog
        {
            Username = actor.UserName,
            Action = isCreate ? "Tạo danh mục quản trị" : "Sửa danh mục quản trị",
            EntityName = catalog.ToString(),
            EntityId = id.ToString(),
            OldValues = previous == null ? null : Snapshot(catalog, previous),
            NewValues = Snapshot(catalog, form, id),
            IpAddress = GetClientIpAddress()
        });
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(AdminCatalog catalog, Guid id, CurrentUserDto actor)
    {
        RequireAdmin(actor);
        if (catalog == AdminCatalog.Parameters)
        {
            if (!await new SystemConfigurationService(db, httpContextAccessor).DeleteAsync(id, actor)) throw new KeyNotFoundException("Tham số không tồn tại.");
            return;
        }
        var previous = (await ListAsync(catalog, actor)).SingleOrDefault(x => x.Id == id);
        switch (catalog)
        {
            case AdminCatalog.Modules:
                if (await db.Modules.AnyAsync(x => x.PhanHeId == id)) throw new InvalidOperationException("Module còn menu liên kết, không thể xóa.");
                db.PhanHes.Remove(await db.PhanHes.FindAsync(id) ?? throw new KeyNotFoundException()); break;
            case AdminCatalog.Menus:
                if (await db.Modules.AnyAsync(x => x.ModuleChaId == id) || await db.RoleModules.AnyAsync(x => x.ModuleId == id)) throw new InvalidOperationException("Menu còn mục con hoặc đã gán vai trò, không thể xóa.");
                db.Modules.Remove(await db.Modules.FindAsync(id) ?? throw new KeyNotFoundException()); break;
            case AdminCatalog.Roles:
                if (await db.UserRoles.AnyAsync(x => x.RoleId == id) || await db.RoleModules.AnyAsync(x => x.RoleId == id)) throw new InvalidOperationException("Vai trò còn người dùng hoặc menu được gán, không thể xóa.");
                db.Roles.Remove(await db.Roles.FindAsync(id) ?? throw new KeyNotFoundException()); break;
            default: throw new InvalidOperationException("Hãy ngừng sử dụng bản ghi để giữ lịch sử thay vì xóa.");
        }
        db.AuditLogs.Add(new AuditLog
        {
            Username = actor.UserName,
            Action = "Xóa danh mục quản trị",
            EntityName = catalog.ToString(),
            EntityId = id.ToString(),
            OldValues = previous == null ? null : Snapshot(catalog, previous),
            NewValues = JsonSerializer.Serialize(new { Deleted = true }),
            IpAddress = GetClientIpAddress()
        });
        await db.SaveChangesAsync();
    }

    private string? GetClientIpAddress() =>
        httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    private static string Snapshot(AdminCatalog catalog, AdminRecord record, Guid? id = null)
    {
        var recordId = id ?? record.Id;
        object value = catalog switch
        {
            AdminCatalog.Users => new
            {
                Id = recordId,
                record.Code,
                record.Name,
                record.Email,
                record.Phone,
                record.AccountRole,
                VillageIds = record.VillageIds,
                AssignedIds = record.AssignedIds,
                record.Active,
                record.ModerationStatus
            },
            AdminCatalog.Parameters => new
            {
                Id = recordId,
                record.Code,
                record.Description,
                record.Active,
                record.ModerationStatus
            },
            AdminCatalog.Menus => new
            {
                Id = recordId,
                record.Name,
                record.Path,
                record.Icon,
                record.Order,
                record.Expanded,
                record.ParentId,
                record.ModuleId,
                record.Active,
                record.ModerationStatus
            },
            AdminCatalog.Roles => new
            {
                Id = recordId,
                record.Code,
                record.Name,
                record.Description,
                AssignedIds = record.AssignedIds,
                record.Active,
                record.ModerationStatus
            },
            AdminCatalog.Groups => new
            {
                Id = recordId,
                record.Code,
                record.Name,
                record.Description,
                record.ParentId,
                record.UnitType,
                record.Active
            },
            _ => new
            {
                Id = recordId,
                record.Code,
                record.Name,
                record.Description,
                record.Active
            }
        };

        return JsonSerializer.Serialize(value);
    }

    private static void ValidateParent(List<AdminRecord> records, Guid id, Guid? parentId)
    {
        var seen = new HashSet<Guid> { id };
        while (parentId.HasValue)
        {
            if (!seen.Add(parentId.Value)) throw new ArgumentException("Không thể chọn chính nó hoặc mục con làm cha.");
            var parent = records.SingleOrDefault(x => x.Id == parentId) ?? throw new ArgumentException("Mục cha không tồn tại.");
            parentId = parent.ParentId;
        }
    }
}
