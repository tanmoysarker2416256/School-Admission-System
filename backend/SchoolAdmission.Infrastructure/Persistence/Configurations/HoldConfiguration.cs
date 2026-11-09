using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolAdmission.Domain.Entities;

namespace SchoolAdmission.Infrastructure.Persistence.Configurations;

public class HoldConfiguration : IEntityTypeConfiguration<Hold>
{
    public void Configure(EntityTypeBuilder<Hold> builder)
    {
        builder.ToTable("holds");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.StudentId)
            .IsRequired()
            .HasMaxLength(17);

        builder.Property(h => h.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasOne(h => h.SchoolClass)
            .WithMany()
            .HasForeignKey(h => h.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        // seat counting per class
        builder.HasIndex(h => new { h.ClassId, h.Status, h.ExpiresAt });

        // "my holds" lookup
        builder.HasIndex(h => h.StudentId);

        // one active hold per student per class, enforced by PostgreSQL
        builder.HasIndex(h => new { h.StudentId, h.ClassId })
            .IsUnique()
            .HasFilter("\"Status\" IN ('Held','Confirmed')")
            .HasDatabaseName("ux_holds_one_active_per_student_per_class");
    }
}