using LMS.Domain.Entities;
using LMS.Infrastructure.Data.Configurations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Data;
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
            public DbSet<Course> Courses { get; set; } = null!;
            public DbSet<Module> Modules { get; set; } = null!;
            public DbSet<Activity> Activities { get; set; } = null!;
            public DbSet<Document> Documents { get; set; } = null!;
            public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
            public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
                : base(options)
            {
            }
            protected override void OnModelCreating(ModelBuilder builder)
            {
                base.OnModelCreating(builder);
                builder.ApplyConfiguration(new ApplicationUserConfigurations());
                builder.Entity<RefreshToken>(b =>
                {
                    b.HasIndex(r => r.TokenHash).IsUnique();
                    b.HasOne(r => r.User)
                        .WithMany(u => u.RefreshTokens)
                        .HasForeignKey(r => r.UserId)
                        .OnDelete(DeleteBehavior.Cascade);
                    b.HasOne(r => r.ReplacedByToken)
                        .WithMany()
                        .HasForeignKey(r => r.ReplacedByTokenId)
                        .OnDelete(DeleteBehavior.NoAction);
                });
            }
}
