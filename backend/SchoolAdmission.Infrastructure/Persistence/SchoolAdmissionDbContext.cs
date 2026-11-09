using Microsoft.EntityFrameworkCore;
using SchoolAdmission.Domain.Entities;

namespace SchoolAdmission.Infrastructure.Persistence;

public class SchoolAdmissionDbContext : DbContext
{
    public SchoolAdmissionDbContext(DbContextOptions<SchoolAdmissionDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SchoolAdmissionDbContext).Assembly);
    }

    public DbSet<SchoolClass> Classes => Set<SchoolClass>();
    public DbSet<Hold> Holds => Set<Hold>();
}