using Microsoft.EntityFrameworkCore;
using MRT.Domain.Entities;
using MRT.Domain.Enums;
using System.Reflection.Metadata;

namespace MRT.Infrastructure.Data;

public class AppDbContext : DbContext
{

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }


    #region DbSet
    public DbSet<ApplicationUser> User { get; set; }
    public DbSet<Role> Role { get; set; }
    #endregion


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, RoleName = "Admin" },
            new Role { Id = 2, RoleName = "User" }
         );


        //User
        modelBuilder.Entity<ApplicationUser>()
        .Property(u => u.Status)
        .HasConversion(
            v => v.ToString(),
            v => (StatusEnum)Enum.Parse(typeof(StatusEnum), v)
        );

        modelBuilder.Entity<ApplicationUser>()
            .HasData(
                new ApplicationUser { Id = Guid.Parse("8B56687E-8377-4743-AAC9-08DCF5C4B471"), UserName = "Admin", Email = "admin", PasswordHash = "$2y$10$O1smXu1TdT1x.Z35v5jQauKcQIBn85VYRqiLggPD8HMF9rRyGnHXy", Status = StatusEnum.Active, RoleId = 1, IsVerified = true, PhoneNumber = "0123456789", CreationDate = DateTime.Now, IsDeleted = false }
           );

        modelBuilder.Entity<ApplicationUser>()
            .HasOne(r => r.Role)
            .WithMany(u => u.Users);
    }
}
