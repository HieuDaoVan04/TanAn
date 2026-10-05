// "Một sản phẩm của HieuDV"

using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Service.TanAn.Domain.Entities;

namespace Service.TanAn.Application.Interfaces
{
    public interface ITanAnDbContext
    {
        DbSet<PhanHe> PhanHes { get; set; }
        DbSet<Module> Modules { get; set; }
        DbSet<Role> Roles { get; set; }
        DbSet<UserRole> UserRoles { get; set; }
        DbSet<RoleModule> RoleModules { get; set; }
        DbSet<Groups> Groups { get; set; }
        DbSet<SystemParameter> SystemParameters { get; set; }
        DbSet<Xa> Xas { get; set; }
        DbSet<PhuTrachThon> PhuTrachThons { get; set; }
        DbSet<ThongBaoThon> ThongBaoThons { get; set; }
        DbSet<ApThon> ApThons { get; set; }
        DbSet<ThanhVienHo> ThanhVienHos { get; set; }
        DbSet<PhanLoaiHo> PhanLoaiHos { get; set; }
        DbSet<LichSuXuLyHoSo> LichSuXuLyHoSos { get; set; }
        DbSet<TepDinhKem> TepDinhKems { get; set; }
        DbSet<HoGiaDinh> HoGiaDinhs { get; set; }
        DbSet<NhanKhau> NhanKhaus { get; set; }
        DbSet<BienDongDanCu> BienDongDanCus { get; set; }
        DbSet<DoiTuongAnSinh> DoiTuongAnSinhs { get; set; }
        DbSet<LichSuTroCap> LichSuTroCaps { get; set; }
        DbSet<YeuCauNguoiDan> YeuCauNguoiDans { get; set; }
        DbSet<AuditLog> AuditLogs { get; set; }
        DbSet<User> Users { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}

