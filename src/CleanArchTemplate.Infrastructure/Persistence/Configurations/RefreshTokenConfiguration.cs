using CleanArchTemplate.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchTemplate.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(rt => rt.Id);
        builder.Property(rt => rt.Token).IsRequired().HasMaxLength(512);
        builder.Property(rt => rt.ReplacedByToken).HasMaxLength(512);
        builder.Property(rt => rt.RevokedByIp).HasMaxLength(64);
        builder.Property(rt => rt.CreatedByIp).HasMaxLength(64);

        builder.HasIndex(rt => rt.Token).IsUnique().HasDatabaseName("IX_RefreshTokens_Token");
        builder.HasIndex(rt => rt.UserId).HasDatabaseName("IX_RefreshTokens_UserId");

        builder.HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(rt => rt.IsExpired);
        builder.Ignore(rt => rt.IsActive);
    }
}
