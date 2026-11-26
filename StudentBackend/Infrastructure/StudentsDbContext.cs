using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StudentBackend.Entities;

namespace StudentBackend.Infrastructure;

public class StudentsDbContext : IdentityDbContext<AppUser, AppRole, Guid>
{
    public StudentsDbContext(DbContextOptions<StudentsDbContext> options)
        : base(options) { }

    public DbSet<Student> Students => Set<Student>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Student>(student =>
        {
            student.HasKey(s => s.StudentId);
            student.Property(s => s.StudentId).ValueGeneratedNever();

            student.HasOne<AppUser>()
                   .WithOne(u => u.Student)
                   .HasForeignKey<Student>(s => s.StudentId); 
        });
    }
}
