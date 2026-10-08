using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Service.TanAn.Domain.Entities;

namespace Service.TanAn.Infrastructure.Persistence.Configurations;

public class YeuCauNguoiDanConfiguration : IEntityTypeConfiguration<YeuCauNguoiDan>
{
    public void Configure(EntityTypeBuilder<YeuCauNguoiDan> b)
    {
        b.HasKey(x => x.Id);
        b.HasOne(x => x.Thon).WithMany().HasForeignKey(x => x.ApThonId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.MaYeuCau).HasMaxLength(50).IsRequired();
        b.HasIndex(x => x.MaYeuCau).IsUnique();
        b.Property(x => x.HoTenNguoiYeuCau).HasMaxLength(200).IsRequired();
        b.Property(x => x.CCCDNguoiYeuCau).HasMaxLength(12);
        b.Property(x => x.SoDienThoai).HasMaxLength(20);
        b.Property(x => x.LoaiYeuCau).HasMaxLength(150).IsRequired();
        b.HasOne(x => x.NguoiNop).WithMany().HasForeignKey(x => x.NguoiNopId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.NguoiXuLy).WithMany().HasForeignKey(x => x.CanBoXuLyId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.TrangThai, x.NgayGui });
    }
}

public class LichSuXuLyHoSoConfiguration : IEntityTypeConfiguration<LichSuXuLyHoSo>
{
    public void Configure(EntityTypeBuilder<LichSuXuLyHoSo> b)
    {
        b.HasKey(x => x.Id);
        b.HasOne(x => x.YeuCau).WithMany(x => x.LichSuXuLy).HasForeignKey(x => x.YeuCauId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.NguoiXuLy).WithMany().HasForeignKey(x => x.NguoiXuLyId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.YeuCauId, x.NgayTao });
    }
}

public class TepDinhKemConfiguration : IEntityTypeConfiguration<TepDinhKem>
{
    public void Configure(EntityTypeBuilder<TepDinhKem> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.TenTep).HasMaxLength(255).IsRequired();
        b.Property(x => x.DuongDanLuu).HasMaxLength(1000).IsRequired();
        b.Property(x => x.LoaiTep).HasMaxLength(150).IsRequired();
        b.ToTable("TepDinhKems", t => t.HasCheckConstraint("CK_TepDinhKem_DungLuong", "\"DungLuong\" >= 0"));
        b.HasOne(x => x.YeuCau).WithMany(x => x.TepDinhKems).HasForeignKey(x => x.YeuCauId).OnDelete(DeleteBehavior.Restrict);
    }
}
