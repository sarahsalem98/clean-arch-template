using CleanArchTemplate.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchTemplate.Infrastructure.Persistence.Configurations;

public class DeviceTokenConfiguration : IEntityTypeConfiguration<DeviceToken>
{
    public void Configure(EntityTypeBuilder<DeviceToken> builder)
    {
        builder.ToTable("DeviceTokens");
        builder.HasKey(dt => dt.Id);
        builder.Property(dt => dt.Token).IsRequired().HasMaxLength(512);
        builder.Property(dt => dt.DeviceType).IsRequired().HasMaxLength(20);

        builder.HasIndex(dt => new { dt.UserId, dt.Token }).IsUnique().HasDatabaseName("IX_DeviceTokens_UserId_Token");

        builder.HasOne(dt => dt.User)
            .WithMany(u => u.DeviceTokens)
            .HasForeignKey(dt => dt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
