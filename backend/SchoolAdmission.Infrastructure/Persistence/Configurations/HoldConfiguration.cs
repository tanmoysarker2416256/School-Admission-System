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

        // one active hold or confirmed seat per student across ALL classes, enforced by PostgreSQL
        builder.HasIndex(h => h.StudentId)
            .IsUnique()
            .HasFilter("\"Status\" IN ('Held','Confirmed')")
            .HasDatabaseName("ux_holds_one_active_per_student");
    }
}