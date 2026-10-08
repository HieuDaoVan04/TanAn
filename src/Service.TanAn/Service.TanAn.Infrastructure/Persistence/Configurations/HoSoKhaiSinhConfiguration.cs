using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Service.TanAn.Domain.Entities;
using Service.Shared.Commons.Model.SQL;

namespace Service.TanAn.Infrastructure.Persistence.Configurations;

public class HoSoKhaiSinhConfiguration : IEntityTypeConfiguration<HoSoKhaiSinh>
{
    public void Configure(EntityTypeBuilder<HoSoKhaiSinh> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.MaHoSo).HasMaxLength(50).IsRequired();
        b.HasIndex(x => x.MaHoSo).IsUnique();
        b.Property(x => x.HoTenTre).HasMaxLength(200).IsRequired();
        b.Property(x => x.HoTenNguoiYeuCau).HasMaxLength(200).IsRequired();
        b.Property(x => x.MaSoHo).HasMaxLength(50).IsRequired();
        b.Property(x => x.NoiDungJson).HasColumnType("text").IsRequired();
        b.Property(x => x.PhienBan).IsConcurrencyToken();
        b.Property(x => x.ModerationStatus).HasDefaultValue(ModerationStatus.Pending).HasSentinel(ModerationStatus.Pending);
        b.ToTable(t => t.HasCheckConstraint("CK_HoSoKhaiSinhs_ModerationStatus", "\"ModerationStatus\" IN (0, 1)"));
        b.Property(x => x.NguoiDuyet).HasMaxLength(100).IsRequired();
        b.HasOne<User>().WithMany().HasForeignKey(x => x.NguoiDuyetId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<HoGiaDinh>().WithMany().HasForeignKey(x => x.HoGiaDinhId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<ApThon>().WithMany().HasForeignKey(x => x.ApThonId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<User>().WithMany().HasForeignKey(x => x.NguoiTaoId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.ApThonId, x.NgayTao });
        b.HasIndex(x => new { x.NguoiTaoId, x.NgayTao });
    }
}
