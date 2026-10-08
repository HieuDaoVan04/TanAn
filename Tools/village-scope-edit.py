from pathlib import Path
root=Path('src')
def write(p,s): Path(p).write_text(s,encoding='utf-8')
def edit(p,a,b):
 p=Path(p);s=p.read_text(encoding='utf-8-sig');assert a in s,p;p.write_text(s.replace(a,b),encoding='utf-8')
domain='src/Service.TanAn/Service.TanAn.Domain/Entities/'
write(domain+'Xa.cs','''namespace Service.TanAn.Domain.Entities;
public class Xa : BaseEntity
{
    public string Ma { get; set; } = "";
    public string Ten { get; set; } = "";
    public ICollection<ApThon> Thons { get; set; } = new List<ApThon>();
}
''')
write(domain+'PhuTrachThon.cs','''namespace Service.TanAn.Domain.Entities;
public class PhuTrachThon
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid ApThonId { get; set; }
    public ApThon Thon { get; set; } = null!;
}
''')
write(domain+'ThongBaoThon.cs','''namespace Service.TanAn.Domain.Entities;
public class ThongBaoThon : BaseEntity
{
    public Guid NguoiNhanId { get; set; }
    public User NguoiNhan { get; set; } = null!;
    public Guid ApThonId { get; set; }
    public ApThon Thon { get; set; } = null!;
    public string TieuDe { get; set; } = "";
    public string NoiDung { get; set; } = "";
    public DateTime? DaDocLuc { get; set; }
}
''')
edit(domain+'ApThon.cs','    public Guid? GroupId','    public Guid? XaId { get; set; }\n    public Xa? Xa { get; set; }\n    public Guid? GroupId')
edit(domain+'YeuCauNguoiDan.cs','        public Guid? NguoiNopId','        public Guid? ApThonId { get; set; }\n        public ApThon? Thon { get; set; }\n        public Guid? NguoiNopId')
for p in ['src/Service.TanAn/Service.TanAn.Application/Interfaces/ITanAnDbContext.cs','src/Service.TanAn/Service.TanAn.Infrastructure/Persistence/TanAnDbContext.cs']:
 interface='Interfaces' in p
 decl='        DbSet' if interface else '        public DbSet'
 end=' { get; set; }' if interface else ' { get; set; } = null!;'
 anchor='        DbSet<ApThon>' if interface else '        public DbSet<ApThon>'
 extra='\n'.join(decl+'<'+t+'> '+n+end for t,n in [('Xa','Xas'),('PhuTrachThon','PhuTrachThons'),('ThongBaoThon','ThongBaoThons')])+'\n'
 edit(p,anchor,extra+anchor)
write('src/Service.TanAn/Service.TanAn.Infrastructure/Persistence/Configurations/VillageScopeConfigurations.cs','''using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Service.TanAn.Domain.Entities;
namespace Service.TanAn.Infrastructure.Persistence.Configurations;
public class XaConfiguration : IEntityTypeConfiguration<Xa>
{
    public void Configure(EntityTypeBuilder<Xa> b) { b.HasKey(x=>x.Id); b.Property(x=>x.Ma).HasMaxLength(30); b.HasIndex(x=>x.Ma).IsUnique(); b.Property(x=>x.Ten).HasMaxLength(150); }
}
public class PhuTrachThonConfiguration : IEntityTypeConfiguration<PhuTrachThon>
{
    public void Configure(EntityTypeBuilder<PhuTrachThon> b) {
        b.HasKey(x=>new {x.UserId,x.ApThonId});
        b.HasOne(x=>x.User).WithMany().HasForeignKey(x=>x.UserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x=>x.Thon).WithMany().HasForeignKey(x=>x.ApThonId).OnDelete(DeleteBehavior.Restrict);
    }
}
public class ThongBaoThonConfiguration : IEntityTypeConfiguration<ThongBaoThon>
{
    public void Configure(EntityTypeBuilder<ThongBaoThon> b) {
        b.HasKey(x=>x.Id); b.Property(x=>x.TieuDe).HasMaxLength(200); b.Property(x=>x.NoiDung).HasMaxLength(2000);
        b.HasOne(x=>x.NguoiNhan).WithMany().HasForeignKey(x=>x.NguoiNhanId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x=>x.Thon).WithMany().HasForeignKey(x=>x.ApThonId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x=>new {x.NguoiNhanId,x.NgayTao});
    }
}
''')
edit('src/Service.TanAn/Service.TanAn.Infrastructure/Persistence/Configurations/DanCuConfigurations.cs','        b.HasIndex(x => x.Ma).IsUnique();','        b.HasIndex(x => x.Ma).IsUnique();\n        b.HasOne(x => x.Xa).WithMany(x => x.Thons).HasForeignKey(x => x.XaId).OnDelete(DeleteBehavior.Restrict);')
edit('src/Service.TanAn/Service.TanAn.Infrastructure/Persistence/Configurations/HoSoConfigurations.cs','        b.HasKey(x => x.Id);','        b.HasKey(x => x.Id);',)
p='src/Service.TanAn/Service.TanAn.Infrastructure/Persistence/Configurations/HoSoConfigurations.cs'
s=Path(p).read_text(encoding='utf-8').replace('        b.HasKey(x => x.Id);','        b.HasKey(x => x.Id);\n        b.HasOne(x => x.Thon).WithMany().HasForeignKey(x => x.ApThonId).OnDelete(DeleteBehavior.Restrict);',1);write(p,s)
