using CleanArchTemplate.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchTemplate.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(512);
        builder.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.LastName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Phone).HasMaxLength(20);
        builder.Property(u => u.ProfileImage).HasMaxLength(1024);
        builder.Property(u => u.ThumbnailImage).HasMaxLength(1024);
        builder.Property(u => u.EmailVerificationToken).HasMaxLength(256);
        builder.Property(u => u.PasswordResetToken).HasMaxLength(256);

        builder.Property(u => u.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(u => u.Gender)
            .HasConversion<string>()
            .HasMaxLength(10);

        // Owned Address entity
        builder.OwnsOne(u => u.Address, addr =>
        {
            addr.Property(a => a.Street).HasMaxLength(256).HasColumnName("AddressStreet");
            addr.Property(a => a.City).HasMaxLength(100).HasColumnName("AddressCity");
            addr.Property(a => a.Country).HasMaxLength(100).HasColumnName("AddressCountry");
            addr.Property(a => a.PostalCode).HasMaxLength(20).HasColumnName("AddressPostalCode");
        });

        // Indexes
        builder.HasIndex(u => u.Email).IsUnique().HasDatabaseName("IX_Users_Email");
        builder.HasIndex(u => u.IsDeleted).HasDatabaseName("IX_Users_IsDeleted");
        builder.HasIndex(u => u.Status).HasDatabaseName("IX_Users_Status");

        // Navigation properties configured on other entities
        builder.Ignore(u => u.FullName);
    }
}
