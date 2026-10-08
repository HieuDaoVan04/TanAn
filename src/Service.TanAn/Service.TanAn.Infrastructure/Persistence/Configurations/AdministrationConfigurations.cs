using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Service.TanAn.Domain.Entities;

namespace Service.TanAn.Infrastructure.Persistence.Configurations;

public class PhanHeConfiguration : IEntityTypeConfiguration<PhanHe>
{
    public void Configure(EntityTypeBuilder<PhanHe> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Ma).HasMaxLength(50).IsRequired();
        b.Property(x => x.Ten).HasMaxLength(200).IsRequired();
        b.HasIndex(x => x.Ma).IsUnique();
    }
}
public class ModuleConfiguration : IEntityTypeConfiguration<Module>
{
    public void Configure(EntityTypeBuilder<Module> b)
    {
        b.HasOne(x => x.PhanHe).WithMany().HasForeignKey(x => x.PhanHeId).OnDelete(DeleteBehavior.Restrict);
    }
}
public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> b)
    {
        b.HasIndex(x => new { x.UserId, x.RoleId }).IsUnique();
        b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Role).WithMany().HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Restrict);
    }
}
