// "Một sản phẩm của HieuDV"

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Service.TanAn.Application.Interfaces;
using Service.TanAn.Domain.Entities;

namespace Service.TanAn.Infrastructure.Persistence
{
    public partial class TanAnDbContext : DbContext, ITanAnDbContext
    {
        private readonly Service.TanAn.Infrastructure.Services.DataActor? actor;
        public TanAnDbContext(DbContextOptions<TanAnDbContext> options, Service.TanAn.Infrastructure.Services.DataActor? actor = null) : base(options) { this.actor = actor; }
        public DbSet<PhanHe> PhanHes { get; set; } = null!;
        public DbSet<UserRole> UserRoles { get; set; } = null!;

        public DbSet<Xa> Xas { get; set; } = null!;
        public DbSet<PhuTrachThon> PhuTrachThons { get; set; } = null!;
        public DbSet<ThongBaoThon> ThongBaoThons { get; set; } = null!;
        public DbSet<ApThon> ApThons { get; set; } = null!;
        public DbSet<ThanhVienHo> ThanhVienHos { get; set; } = null!;
        public DbSet<PhanLoaiHo> PhanLoaiHos { get; set; } = null!;
        public DbSet<LichSuXuLyHoSo> LichSuXuLyHoSos { get; set; } = null!;
        public DbSet<TepDinhKem> TepDinhKems { get; set; } = null!;

        public DbSet<HoGiaDinh> HoGiaDinhs { get; set; } = null!;
        public DbSet<NhanKhau> NhanKhaus { get; set; } = null!;
        public DbSet<BienDongDanCu> BienDongDanCus { get; set; } = null!;
        public DbSet<DoiTuongAnSinh> DoiTuongAnSinhs { get; set; } = null!;
        public DbSet<LichSuTroCap> LichSuTroCaps { get; set; } = null!;
        public DbSet<YeuCauNguoiDan> YeuCauNguoiDans { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<RolePermission> RolePermissions { get; set; } = null!;
        public DbSet<RoleModule> RoleModules { get; set; } = null!;
        public DbSet<UserRoleHistory> UserRoleHistories { get; set; } = null!;
        public DbSet<Permission> Permissions { get; set; } = null!;
        public DbSet<Module> Modules { get; set; } = null!;
        public DbSet<LogThaoTacNguoiDung> LogThaoTacNguoiDungs { get; set; } = null!;
        public DbSet<SystemParameter> SystemParameters { get; set; } = null!;
        public DbSet<Groups> Groups { get; set; } = null!;
        public DbSet<UserGroups> UserGroups { get; set; } = null!;
        public DbSet<UserPermission> UserPermissions { get; set; } = null!;

        public int SaveChanges(Guid userId, Guid departmentId)
        {
            return SaveChanges();
        }

        public async Task<int> SaveChangesAsync(Guid userId, Guid departmentId, CancellationToken cancellationToken = default)
        {
            return await SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(TanAnDbContext).Assembly);
            ConfigureVillageScope(modelBuilder);
        }
    }
}
