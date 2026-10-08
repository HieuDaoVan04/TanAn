using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Service.TanAn.Domain.Entities;

namespace Service.TanAn.Infrastructure.Persistence.Configurations;

public class PhanLoaiHoConfiguration : IEntityTypeConfiguration<PhanLoaiHo>
{
    public void Configure(EntityTypeBuilder<PhanLoaiHo> b)
    {
        b.HasKey(x => x.Id);
        b.ToTable("PhanLoaiHos", t => t.HasCheckConstraint("CK_PhanLoaiHo_ThoiGian", "\"DenNgay\" IS NULL OR \"DenNgay\" >= \"TuNgay\""));
        b.Property(x => x.SoQuyetDinh).HasMaxLength(100);
        b.HasOne(x => x.HoGiaDinh).WithMany(x => x.LichSuPhanLoai)
            .HasForeignKey(x => x.HoGiaDinhId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.HoGiaDinhId).IsUnique().HasFilter("\"DenNgay\" IS NULL");
    }
}

public class DoiTuongAnSinhConfiguration : IEntityTypeConfiguration<DoiTuongAnSinh>
{
    public void Configure(EntityTypeBuilder<DoiTuongAnSinh> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.MucTroCapHangThang).HasPrecision(18, 2);
        b.Property(x => x.SoQuyetDinh).HasMaxLength(100);
        b.ToTable("DoiTuongAnSinhs", t =>
        {
            t.HasCheckConstraint("CK_AnSinh_MucTroCap", "\"MucTroCapHangThang\" >= 0");
            t.HasCheckConstraint("CK_AnSinh_ThoiGian", "\"NgayKetThucHuong\" IS NULL OR \"NgayKetThucHuong\" >= \"NgayBatDauHuong\"");
        });
        b.HasOne(x => x.NhanKhau).WithMany(x => x.ChinhSachAnSinh)
            .HasForeignKey(x => x.NhanKhauId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.NhanKhauId, x.LoaiDoiTuong });
    }
}

public class LichSuTroCapConfiguration : IEntityTypeConfiguration<LichSuTroCap>
{
    public void Configure(EntityTypeBuilder<LichSuTroCap> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.SoTien).HasPrecision(18, 2);
        b.Property(x => x.ThangNam).HasMaxLength(7).IsRequired();
        b.ToTable("LichSuTroCaps", t => t.HasCheckConstraint("CK_TroCap_SoTien", "\"SoTien\" > 0"));
        b.HasOne(x => x.DoiTuongAnSinh).WithMany(x => x.LichSuTroCaps)
            .HasForeignKey(x => x.DoiTuongAnSinhId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CanBoChiTra).WithMany().HasForeignKey(x => x.NguoiChiTraId).OnDelete(DeleteBehavior.Restrict);
        // Không unique: có thể có nhiều đợt chi trả trong cùng tháng.
        b.HasIndex(x => new { x.DoiTuongAnSinhId, x.ThangNam });
    }
}
