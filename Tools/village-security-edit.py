from pathlib import Path
def w(p,s): Path(p).write_text(s,encoding='utf-8')
p='src/Service.TanAn/Service.TanAn.Infrastructure/Services/DataActor.cs'
w(p,'''using System.Security.Claims;
using Microsoft.AspNetCore.Http;
namespace Service.TanAn.Infrastructure.Services;
/// <summary>Execution-local identity shared by nested Blazor scopes; HTTP fallback for API/SSR.</summary>
public sealed class DataActor(IHttpContextAccessor http)
{
    private readonly AsyncLocal<ClaimsPrincipal?> current = new();
    public ClaimsPrincipal? Principal { get => current.Value ?? http.HttpContext?.User; set => current.Value = value; }
    public Guid UserId => Principal?.Identity?.IsAuthenticated == true && Guid.TryParse(Principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;
}
''')
p=Path('src/Service.TanAn/Service.TanAn.Infrastructure/DependencyInjection.cs');s=p.read_text(encoding='utf-8-sig').replace('            services.AddHttpContextAccessor();','            services.AddHttpContextAccessor();\n            services.AddSingleton<DataActor>();');w(p,s)
p=Path('src/Service.TanAn/Service.TanAn.Infrastructure/Persistence/TanAnDbContext.cs');s=p.read_text(encoding='utf-8-sig').replace('public class TanAnDbContext','public partial class TanAnDbContext').replace('public TanAnDbContext(DbContextOptions<TanAnDbContext> options) : base(options) { }','private readonly Service.TanAn.Infrastructure.Services.DataActor? actor;\n        public TanAnDbContext(DbContextOptions<TanAnDbContext> options, Service.TanAn.Infrastructure.Services.DataActor? actor = null) : base(options) { this.actor = actor; }').replace('            return base.SaveChanges();','            return SaveChanges();').replace('return await base.SaveChangesAsync(cancellationToken);','return await SaveChangesAsync(cancellationToken);').replace('            modelBuilder.ApplyConfigurationsFromAssembly(typeof(TanAnDbContext).Assembly);','            modelBuilder.ApplyConfigurationsFromAssembly(typeof(TanAnDbContext).Assembly);\n            ConfigureVillageScope(modelBuilder);');w(p,s)
w('src/Service.TanAn/Service.TanAn.Infrastructure/Persistence/TanAnDbContext.Scope.cs','''using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Service.TanAn.Domain.Entities;
using Service.TanAn.Domain.Enums;
using Service.Shared.Commons.Model.SQL;
namespace Service.TanAn.Infrastructure.Persistence;
public partial class TanAnDbContext
{
    private Guid ActorId => actor?.UserId ?? Guid.Empty;
    // Standalone migration/seed tools explicitly construct a context without an HTTP actor.
    private bool FullVillageAccess => actor == null || Users.AsNoTracking().Any(u => u.Id == ActorId && u.ModerationStatus == ModerationStatus.Approved && (u.LockoutEnd == null || u.LockoutEnd <= DateTime.UtcNow) && (u.Role == RoleEnum.Admin || u.Role == RoleEnum.CanBoXa));
    private Guid[] AllowedVillageIds => actor == null ? [] : PhuTrachThons.AsNoTracking()
        .Where(p => p.UserId == ActorId && p.User.Role == RoleEnum.CanBoThon && p.User.ModerationStatus == ModerationStatus.Approved && (p.User.LockoutEnd == null || p.User.LockoutEnd <= DateTime.UtcNow) && p.Thon.DangHoatDong)
        .Select(p => p.ApThonId).ToArray();
    private void ConfigureVillageScope(ModelBuilder b)
    {
        b.Entity<HoGiaDinh>().HasQueryFilter(x => FullVillageAccess || (x.ApThonId.HasValue && AllowedVillageIds.Contains(x.ApThonId.Value)));
        b.Entity<NhanKhau>().HasQueryFilter(x => FullVillageAccess || HoGiaDinhs.Any(h => h.Id == x.MaHoGiaDinh));
        b.Entity<ThanhVienHo>().HasQueryFilter(x => FullVillageAccess || HoGiaDinhs.Any(h => h.Id == x.HoGiaDinhId));
        b.Entity<PhanLoaiHo>().HasQueryFilter(x => FullVillageAccess || HoGiaDinhs.Any(h => h.Id == x.HoGiaDinhId));
        b.Entity<BienDongDanCu>().HasQueryFilter(x => FullVillageAccess || NhanKhaus.Any(n => n.Id == x.NhanKhauId));
        b.Entity<DoiTuongAnSinh>().HasQueryFilter(x => FullVillageAccess || NhanKhaus.Any(n => n.Id == x.NhanKhauId));
        b.Entity<LichSuTroCap>().HasQueryFilter(x => FullVillageAccess || DoiTuongAnSinhs.Any(n => n.Id == x.DoiTuongAnSinhId));
        b.Entity<YeuCauNguoiDan>().HasQueryFilter(x => FullVillageAccess || (x.ApThonId.HasValue && AllowedVillageIds.Contains(x.ApThonId.Value)));
        b.Entity<LichSuXuLyHoSo>().HasQueryFilter(x => FullVillageAccess || YeuCauNguoiDans.Any(n => n.Id == x.YeuCauId));
        b.Entity<TepDinhKem>().HasQueryFilter(x => FullVillageAccess || YeuCauNguoiDans.Any(n => n.Id == x.YeuCauId));
        b.Entity<ThongBaoThon>().HasQueryFilter(x => actor == null || (x.NguoiNhanId == ActorId && AllowedVillageIds.Contains(x.ApThonId)));
    }
    private static bool Business(object x) => x is HoGiaDinh or NhanKhau or ThanhVienHo or PhanLoaiHo or BienDongDanCu or DoiTuongAnSinh or LichSuTroCap or YeuCauNguoiDan or LichSuXuLyHoSo or TepDinhKem;
    private async Task<bool> OwnsValues(object entity, PropertyValues values, Guid[] allowed, CancellationToken ct)
    {
        Guid Id(string key) => values.GetValue<Guid>(key);
        Guid? NullableId(string key) => values.GetValue<Guid?>(key);
        async Task<bool> OwnsHouse(Guid id) => await HoGiaDinhs.AsNoTracking().AnyAsync(x=>x.Id==id && x.ApThonId.HasValue && allowed.Contains(x.ApThonId.Value),ct)
            || ChangeTracker.Entries<HoGiaDinh>().Any(e=>e.State==EntityState.Added && e.Entity.Id==id && e.Entity.ApThonId.HasValue && allowed.Contains(e.Entity.ApThonId.Value));
        return entity switch {
            HoGiaDinh or YeuCauNguoiDan => NullableId("ApThonId") is Guid t && allowed.Contains(t),
            NhanKhau => await OwnsHouse(Id("MaHoGiaDinh")),
            ThanhVienHo => await OwnsHouse(Id("HoGiaDinhId")) && await NhanKhaus.AsNoTracking().AnyAsync(n=>n.Id==Id("NhanKhauId") && n.MaHoGiaDinh==Id("HoGiaDinhId"),ct),
            PhanLoaiHo => await OwnsHouse(Id("HoGiaDinhId")),
            BienDongDanCu or DoiTuongAnSinh => await NhanKhaus.AsNoTracking().AnyAsync(x=>x.Id==Id("NhanKhauId"),ct),
            LichSuTroCap => await DoiTuongAnSinhs.AsNoTracking().AnyAsync(x=>x.Id==Id("DoiTuongAnSinhId"),ct),
            LichSuXuLyHoSo or TepDinhKem => await YeuCauNguoiDans.AsNoTracking().AnyAsync(x=>x.Id==Id("YeuCauId"),ct),
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
''')
w('src/Service.UI/Service.UI.CMS.Blazor/Applications/VillageCircuitHandler.cs','''using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Service.TanAn.Infrastructure.Services;
namespace Service.UI.CMS.Blazor.Applications;
public sealed class VillageCircuitHandler(DataActor actor, AuthenticationStateProvider authentication) : CircuitHandler
{
    public override Func<CircuitInboundActivityContext, Task> CreateInboundActivityHandler(Func<CircuitInboundActivityContext, Task> next)
        => async context => {
            var previous = actor.Principal;
            try { actor.Principal = (await authentication.GetAuthenticationStateAsync()).User; await next(context); }
            finally { actor.Principal = previous; }
        };
}
''')
p=Path('src/Service.UI/Service.UI.CMS.Blazor/Program.cs');s=p.read_text(encoding='utf-8-sig').replace('builder.Services.AddScoped<AccountService>();','builder.Services.AddScoped<AccountService>();\nbuilder.Services.AddScoped<Microsoft.AspNetCore.Components.Server.Circuits.CircuitHandler, VillageCircuitHandler>();');w(p,s)
