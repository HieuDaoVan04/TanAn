using Microsoft.EntityFrameworkCore;
using Service.Shared.Commons.Enums;
using Service.Shared.Commons.Model.SQL;
using Service.TanAn.Application.Interfaces;

namespace Service.TanAn.Application.Services;

public static class BirthApprovalAuthorization
{
    public const string DeniedMessage = "Chỉ tài khoản được gán vai trò mã ctx đã duyệt và có quyền truy cập khai sinh được duyệt hồ sơ.";

    public static async Task<bool> CanApproveAsync(ITanAnDbContext db, Guid userId)
    {
        // Đọc lại các quyền đã lưu mỗi lần duyệt để việc thu hồi có hiệu lực ngay.
        var hasRole = await (from ur in db.UserRoles
                             join role in db.Roles on ur.RoleId equals role.Id
                             join user in db.Users on ur.UserId equals user.Id
                             where ur.UserId == userId && role.ModerationStatus == ModerationStatus.Approved
                               && role.RoleCode.Trim().ToLower() == RoleCodes.ChuTichXa
                               && user.ModerationStatus == ModerationStatus.Approved
                               && (user.LockoutEnd == null || user.LockoutEnd <= DateTime.UtcNow)
                             select ur.Id).AnyAsync();
        if (!hasRole) return false;
        return await (from ur in db.UserRoles
                      join role in db.Roles on ur.RoleId equals role.Id
                      join grant in db.RoleModules on role.Id equals grant.RoleId
                      join module in db.Modules on grant.ModuleId equals module.Id
                      where ur.UserId == userId && role.ModerationStatus == ModerationStatus.Approved
                        && module.ModerationStatus == ModerationStatus.Approved && module.LienKet == "/bien-dong/khai-sinh"
                        && (module.PhanHeId == null || module.PhanHe!.HoatDong)
                      select grant.Id).AnyAsync();
    }
}
