using CleanArchTemplate.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchTemplate.Infrastructure.Persistence.Configurations;

public class SocialLoginConfiguration : IEntityTypeConfiguration<SocialLogin>
{
    public void Configure(EntityTypeBuilder<SocialLogin> builder)
    {
        builder.ToTable("SocialLogins");
        builder.HasKey(sl => sl.Id);
        builder.Property(sl => sl.ProviderUserId).IsRequired().HasMaxLength(256);
        builder.Property(sl => sl.ProviderEmail).HasMaxLength(256);
        builder.Property(sl => sl.AccessToken).HasMaxLength(2048);

        builder.Property(sl => sl.Provider)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(sl => new { sl.Provider, sl.ProviderUserId })
            .IsUnique()
            .HasDatabaseName("IX_SocialLogins_Provider_ProviderUserId");

        builder.HasOne(sl => sl.User)
            .WithMany(u => u.SocialLogins)
            .HasForeignKey(sl => sl.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
