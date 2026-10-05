using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Service.TanAn.Domain.Entities;

namespace Service.TanAn.Infrastructure.Persistence.Configurations;

public class ApThonConfiguration : IEntityTypeConfiguration<ApThon>
{
    public void Configure(EntityTypeBuilder<ApThon> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Ma).HasMaxLength(30).IsRequired();
        b.Property(x => x.Ten).HasMaxLength(150).IsRequired();
        b.HasIndex(x => x.Ma).IsUnique();
        b.HasOne(x => x.Xa).WithMany(x => x.Thons).HasForeignKey(x => x.XaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.DonVi).WithOne().HasForeignKey<ApThon>(x => x.GroupId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class HoGiaDinhConfiguration : IEntityTypeConfiguration<HoGiaDinh>
{
    public void Configure(EntityTypeBuilder<HoGiaDinh> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.MaSoHo).HasMaxLength(50).IsRequired();
        b.Property(x => x.TenChuHo).HasMaxLength(200).IsRequired();
        b.Property(x => x.CCCDChuHo).HasMaxLength(12);
        b.Property(x => x.DiaChi).HasMaxLength(500).IsRequired();
        b.Property(x => x.ApThon).HasMaxLength(150);
        b.HasIndex(x => x.MaSoHo).IsUnique();
        b.HasOne(x => x.DiaBan).WithMany(x => x.HoGiaDinhs)
            .HasForeignKey(x => x.ApThonId).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(x => x.ThanhVien).WithOne(x => x.HoGiaDinh)
            .HasForeignKey(x => x.MaHoGiaDinh).OnDelete(DeleteBehavior.Restrict);
    }
}

public class NhanKhauConfiguration : IEntityTypeConfiguration<NhanKhau>
{
    public void Configure(EntityTypeBuilder<NhanKhau> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.HoTen).HasMaxLength(200).IsRequired();
        // Giữ chuỗi rỗng cho người chưa có CCCD để tương thích DTO/service hiện tại.
        b.Property(x => x.CCCD).HasMaxLength(12);
        b.HasIndex(x => x.CCCD).IsUnique().HasFilter("\"CCCD\" IS NOT NULL AND \"CCCD\" <> ''");
        b.Property(x => x.QuanHeVoiChuHo).HasMaxLength(100);
        b.Property(x => x.ThuongTru).HasMaxLength(500);
        b.Property(x => x.TamTru).HasMaxLength(500);
        b.HasIndex(x => new { x.MaHoGiaDinh, x.TrangThai });
    }
}

public class ThanhVienHoConfiguration : IEntityTypeConfiguration<ThanhVienHo>
{
    public void Configure(EntityTypeBuilder<ThanhVienHo> b)
    {
        b.HasKey(x => x.Id);
        b.ToTable("ThanhVienHos", t => t.HasCheckConstraint("CK_ThanhVienHo_ThoiGian", "\"DenNgay\" IS NULL OR \"DenNgay\" >= \"TuNgay\""));
        b.Property(x => x.QuanHeVoiChuHo).HasMaxLength(100).IsRequired();
        b.HasOne(x => x.HoGiaDinh).WithMany(x => x.LichSuThanhVien)
            .HasForeignKey(x => x.HoGiaDinhId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.NhanKhau).WithMany(x => x.LichSuHoGiaDinh)
            .HasForeignKey(x => x.NhanKhauId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.NhanKhauId).IsUnique().HasFilter("\"DenNgay\" IS NULL");
        b.HasIndex(x => x.HoGiaDinhId).IsUnique().HasFilter("\"DenNgay\" IS NULL AND \"LaChuHo\" = TRUE");
        b.HasIndex(x => new { x.HoGiaDinhId, x.TuNgay });
    }
}

public class BienDongDanCuConfiguration : IEntityTypeConfiguration<BienDongDanCu>
{
    public void Configure(EntityTypeBuilder<BienDongDanCu> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.NoiDi).HasMaxLength(500);
        b.Property(x => x.NoiDen).HasMaxLength(500);
        b.HasOne(x => x.NhanKhau).WithMany(x => x.BienDongs)
            .HasForeignKey(x => x.NhanKhauId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CanBo).WithMany().HasForeignKey(x => x.CanBoId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.NhanKhauId, x.NgayPhatSinh });
    }
}
