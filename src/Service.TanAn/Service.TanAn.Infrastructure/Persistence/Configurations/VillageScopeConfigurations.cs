using Microsoft.EntityFrameworkCore;
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
