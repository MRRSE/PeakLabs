using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PeakLabs.Models.Identity;
using PeakLabs.Models;

namespace PeakLabs.Data;

public class ApplicationDbContext : IdentityDbContext<PeakLabsUser>
{
    public DbSet<PortfolioItem> Portfolio { get; set; } = null!;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<PeakLabsUser>(entity =>
        {
            entity.Property(user => user.FirstName)
                .HasMaxLength(60)
                .HasDefaultValue(string.Empty);

            entity.Property(user => user.LastName)
                .HasMaxLength(80)
                .HasDefaultValue(string.Empty);

            entity.Property(user => user.ProfileImagePath)
                .HasMaxLength(500);

            entity.Property(user => user.AuthorDescription)
                .HasMaxLength(240);

            entity.Property(user => user.IsActive)
                .HasDefaultValue(true);
        });

        builder.Entity<PortfolioItem>(entity =>
        {
            entity.ToTable("Portfolio");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.ProjectImagePath).HasMaxLength(500).IsRequired();
            entity.Property(item => item.ProjectName).HasMaxLength(120).IsRequired();
            entity.Property(item => item.ProjectDescription).HasMaxLength(1000).IsRequired();
            entity.Property(item => item.ProjectLink).HasMaxLength(500).IsRequired();
            entity.Property(item => item.AddedAt).IsRequired();
        });
    }
}
