using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PeakLabs.Models.Identity;

namespace PeakLabs.Data;

public class ApplicationDbContext : IdentityDbContext<PeakLabsUser>
{
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
    }
}
