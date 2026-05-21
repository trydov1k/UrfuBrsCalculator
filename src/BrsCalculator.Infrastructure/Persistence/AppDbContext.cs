using BrsCalculator.Domain.Entities;
using BrsCalculator.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BrsCalculator.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Semester> Semesters => Set<Semester>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<SubjectNode> SubjectNodes => Set<SubjectNode>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Semester>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.UserId);
            e.Property(x => x.Name).HasMaxLength(200);
        });

        builder.Entity<Subject>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Semester).WithMany(s => s.Subjects).HasForeignKey(x => x.SemesterId).OnDelete(DeleteBehavior.Cascade);
            e.Property(x => x.Name).HasMaxLength(300);
        });

        builder.Entity<SubjectNode>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.SubjectId);
            e.HasIndex(x => x.Path);
            e.Property(x => x.Path).HasMaxLength(500);
            e.Property(x => x.Name).HasMaxLength(300);
            e.Property(x => x.MaxScore).HasPrecision(10, 4);
            e.Property(x => x.Coefficient).HasPrecision(10, 6);
            e.Property(x => x.ActualScore).HasPrecision(10, 4);
            e.HasOne(x => x.Subject).WithMany(s => s.Nodes).HasForeignKey(x => x.SubjectId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Parent).WithMany(x => x.Children).HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
