using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Service.TanAn.Domain.Entities;

namespace Service.TanAn.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.HasKey(x => x.Id);
        // Các alias CLR không phải cột độc lập. SQLite coi UserName và Username là cùng tên.
        b.Ignore(x => x.Username);
        b.Ignore(x => x.Phone);
        b.Ignore(x => x.CreatedAt);
        b.Property(x => x.UserName).HasMaxLength(100).IsRequired();
        b.HasIndex(x => x.UserName).IsUnique();
    }
}
