using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Service.TanAn.Domain.Entities;
using Service.TanAn.Domain.Enums;
using Service.Shared.Commons.Model.SQL;
namespace Service.TanAn.Infrastructure.Persistence;
public partial class TanAnDbContext
{
    private Guid ActorId => actor?.UserId ?? Guid.Empty;
    // Standalone migration/seed tools explicitly construct a context without an HTTP actor.
    private bool FullVillageAccess => actor == null || Users.AsNoTracking().Any(u => u.Id == ActorId && u.ModerationStatus == ModerationStatus.Approved && (u.LockoutEnd == null || u.LockoutEnd <= DateTime.UtcNow) && (u.Role == RoleEnum.Admin || u.Role == RoleEnum.CanBoXa || u.Role == RoleEnum.ChuTichXa));
    private Guid[] AllowedVillageIds => actor == null ? [] : PhuTrachThons.AsNoTracking()
        .Where(p => p.UserId == ActorId && p.User.Role == RoleEnum.CanBoThon && p.User.ModerationStatus == ModerationStatus.Approved && (p.User.LockoutEnd == null || p.User.LockoutEnd <= DateTime.UtcNow) && p.Thon.DangHoatDong)
        .Select(p => p.ApThonId).ToArray();
    private void ConfigureVillageScope(ModelBuilder b)
    {
        b.Entity<HoGiaDinh>().HasQueryFilter(x => FullVillageAccess || (x.ApThonId.HasValue && Enumerable.Contains(AllowedVillageIds, x.ApThonId.Value)));
        b.Entity<NhanKhau>().HasQueryFilter(x => FullVillageAccess || HoGiaDinhs.Any(h => h.Id == x.MaHoGiaDinh));
        b.Entity<ThanhVienHo>().HasQueryFilter(x => FullVillageAccess || HoGiaDinhs.Any(h => h.Id == x.HoGiaDinhId));
        b.Entity<PhanLoaiHo>().HasQueryFilter(x => FullVillageAccess || HoGiaDinhs.Any(h => h.Id == x.HoGiaDinhId));
        b.Entity<BienDongDanCu>().HasQueryFilter(x => FullVillageAccess || NhanKhaus.Any(n => n.Id == x.NhanKhauId));
        b.Entity<HoSoKhaiSinh>().HasQueryFilter(x => FullVillageAccess || (x.ApThonId.HasValue && Enumerable.Contains(AllowedVillageIds, x.ApThonId.Value)) || (x.ApThonId == null && x.NguoiTaoId == ActorId));
        b.Entity<DoiTuongAnSinh>().HasQueryFilter(x => FullVillageAccess || NhanKhaus.Any(n => n.Id == x.NhanKhauId));
        b.Entity<LichSuTroCap>().HasQueryFilter(x => FullVillageAccess || DoiTuongAnSinhs.Any(n => n.Id == x.DoiTuongAnSinhId));
        b.Entity<YeuCauNguoiDan>().HasQueryFilter(x => FullVillageAccess || (x.ApThonId.HasValue && Enumerable.Contains(AllowedVillageIds, x.ApThonId.Value)));
        b.Entity<LichSuXuLyHoSo>().HasQueryFilter(x => FullVillageAccess || YeuCauNguoiDans.Any(n => n.Id == x.YeuCauId));
        b.Entity<TepDinhKem>().HasQueryFilter(x => FullVillageAccess || YeuCauNguoiDans.Any(n => n.Id == x.YeuCauId));
        b.Entity<ThongBaoThon>().HasQueryFilter(x => actor == null || (x.NguoiNhanId == ActorId && Enumerable.Contains(AllowedVillageIds, x.ApThonId)));
    }
    private static bool Business(object x) => x is HoGiaDinh or NhanKhau or ThanhVienHo or PhanLoaiHo or BienDongDanCu or HoSoKhaiSinh or DoiTuongAnSinh or LichSuTroCap or YeuCauNguoiDan or LichSuXuLyHoSo or TepDinhKem;
    private async Task<bool> OwnsValues(object entity, PropertyValues values, Guid[] allowed, CancellationToken ct)
    {
        Guid Id(string key) => values.GetValue<Guid>(key);
        Guid? NullableId(string key) => values.GetValue<Guid?>(key);
        async Task<bool> OwnsHouse(Guid id) => await HoGiaDinhs.AsNoTracking().AnyAsync(x=>x.Id==id && x.ApThonId.HasValue && Enumerable.Contains(allowed, x.ApThonId.Value),ct)
            || ChangeTracker.Entries<HoGiaDinh>().Any(e=>e.State==EntityState.Added && e.Entity.Id==id && e.Entity.ApThonId.HasValue && allowed.Contains(e.Entity.ApThonId.Value));
        var houseId = entity is ThanhVienHo or PhanLoaiHo ? Id("HoGiaDinhId") : Guid.Empty;
        var personId = entity is ThanhVienHo or BienDongDanCu or DoiTuongAnSinh ? Id("NhanKhauId") : Guid.Empty;
        var benefitId = entity is LichSuTroCap ? Id("DoiTuongAnSinhId") : Guid.Empty;
        var requestId = entity is LichSuXuLyHoSo or TepDinhKem ? Id("YeuCauId") : Guid.Empty;
        return entity switch {
            HoSoKhaiSinh => (NullableId("ApThonId") is Guid v && allowed.Contains(v) || NullableId("ApThonId") == null && NullableId("NguoiTaoId") == ActorId)
                && (NullableId("HoGiaDinhId") is not Guid h || await OwnsHouse(h)),
            HoGiaDinh or YeuCauNguoiDan => NullableId("ApThonId") is Guid t && allowed.Contains(t),
            NhanKhau => await OwnsHouse(Id("MaHoGiaDinh")),
            ThanhVienHo => await OwnsHouse(Id("HoGiaDinhId")) && (await NhanKhaus.AsNoTracking().AnyAsync(n=>n.Id==personId && n.MaHoGiaDinh==houseId,ct) || ChangeTracker.Entries<NhanKhau>().Any(e=>e.State==EntityState.Added && e.Entity.Id==personId && e.Entity.MaHoGiaDinh==houseId)),
            PhanLoaiHo => await OwnsHouse(Id("HoGiaDinhId")),
            BienDongDanCu or DoiTuongAnSinh => await NhanKhaus.AsNoTracking().AnyAsync(x=>x.Id==personId,ct),
            LichSuTroCap => await DoiTuongAnSinhs.AsNoTracking().AnyAsync(x=>x.Id==benefitId,ct),
            LichSuXuLyHoSo or TepDinhKem => await YeuCauNguoiDans.AsNoTracking().AnyAsync(x=>x.Id==requestId,ct),
            _ => false
        };
    }
    private async Task ValidateVillageWrites(CancellationToken ct)
    {
        if (actor == null) return;
        var entries = ChangeTracker.Entries().Where(e=>e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted).ToList();
        bool full = FullVillageAccess;
        var allowed = full ? Array.Empty<Guid>() : AllowedVillageIds;
        foreach(var e in entries)
        {
            if (e.Entity is PhuTrachThon or Xa or ApThon)
            {
                if(!await Users.AsNoTracking().AnyAsync(u=>u.Id==ActorId && u.Role==RoleEnum.Admin && u.ModerationStatus==ModerationStatus.Approved,ct)) throw new UnauthorizedAccessException("Chỉ quản trị xã được gán địa bàn.");
            }
            if (e.Entity is ThongBaoThon notice && !full)
            {
                if(e.State == EntityState.Added && allowed.Contains(notice.ApThonId) && await PhuTrachThons.AnyAsync(p=>p.ApThonId==notice.ApThonId && p.UserId==notice.NguoiNhanId,ct)) continue;
                var prior = await e.GetDatabaseValuesAsync(ct);
                if(e.State != EntityState.Modified || prior == null || prior.GetValue<Guid>("NguoiNhanId") != ActorId || !allowed.Contains(prior.GetValue<Guid>("ApThonId")) || e.Properties.Any(p=>p.IsModified && p.Metadata.Name != "DaDocLuc")) throw new UnauthorizedAccessException("Không có quyền sửa thông báo.");
            }
            if(full || !Business(e.Entity)) continue;
            if(e.State != EntityState.Added)
            {
                var original = await e.GetDatabaseValuesAsync(ct);
                if(original == null || !await OwnsValues(e.Entity,original,allowed,ct)) throw new UnauthorizedAccessException("Dữ liệu không thuộc thôn được giao.");
            }
            if(e.State != EntityState.Deleted && !await OwnsValues(e.Entity,e.CurrentValues,allowed,ct)) throw new UnauthorizedAccessException("Không được ghi dữ liệu ngoài thôn được giao.");
        }
    }
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ValidateVillageWrites(CancellationToken.None).GetAwaiter().GetResult();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }
    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        await ValidateVillageWrites(cancellationToken);
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess,cancellationToken);
    }
}
