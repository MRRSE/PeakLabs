using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PeakLabs.Models.Identity;
using PeakLabs.Models;

namespace PeakLabs.Data;

public class ApplicationDbContext : IdentityDbContext<PeakLabsUser>
{
    public DbSet<PortfolioItem> Portfolio { get; set; } = null!;
    public DbSet<PortfolioView> PortfolioViews { get; set; } = null!;
    public DbSet<Article> Articles { get; set; } = null!;
    public DbSet<ArticleView> ArticleViews { get; set; } = null!;
    public DbSet<ArticleHashtag> ArticleHashtags { get; set; } = null!;
    public DbSet<ArticleCategory> ArticleCategories { get; set; } = null!;
    public DbSet<ArticleSection> ArticleSections { get; set; } = null!;
    public DbSet<Developer> Developers { get; set; } = null!;
    public DbSet<ContactMessage> ContactMessages { get; set; } = null!;
    public DbSet<SiteVisit> SiteVisits { get; set; } = null!;
    public DbSet<DashboardActivity> DashboardActivities { get; set; } = null!;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<PeakLabsUser>(entity =>
        {
            entity.Property(user => user.ArticlesLastViewedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.Property(user => user.PortfolioLastViewedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");

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

            entity.Property(user => user.CreatedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");
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
            entity.Property(item => item.ViewCount).HasDefaultValue(0);
        });

        builder.Entity<PortfolioView>(entity =>
        {
            entity.ToTable("PortfolioView");
            entity.HasKey(view => view.Id);
            entity.Property(view => view.ViewedAt).IsRequired();
            entity.HasIndex(view => view.ViewedAt);
            entity.HasOne(view => view.PortfolioItem).WithMany()
                .HasForeignKey(view => view.PortfolioItemId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Developer>(entity =>
        {
            entity.ToTable("Developers");
            entity.HasKey(developer => developer.Id);
            entity.Property(developer => developer.Name).HasMaxLength(120).IsRequired();
            entity.Property(developer => developer.JobTitle).HasMaxLength(120).IsRequired();
            entity.Property(developer => developer.Email).HasMaxLength(254).IsRequired();
            entity.Property(developer => developer.Bio).HasMaxLength(1000).IsRequired();
            entity.Property(developer => developer.LinkedInUrl).HasMaxLength(500);
            entity.Property(developer => developer.GitHubUrl).HasMaxLength(500);
            entity.Property(developer => developer.ProfileImagePath).HasMaxLength(500);
            entity.Property(developer => developer.CreatedAt).IsRequired();
            entity.HasIndex(developer => developer.IsActive);
        });

        builder.Entity<ArticleCategory>(entity =>
        {
            entity.ToTable("ArticleCategory");
            entity.HasKey(category => category.Id);
            entity.Property(category => category.Name).HasMaxLength(80).IsRequired();
            entity.HasIndex(category => category.Name).IsUnique();
            entity.HasData(
                new ArticleCategory { Id = 1, Name = "فناوری" },
                new ArticleCategory { Id = 2, Name = "ASP.NET Core" },
                new ArticleCategory { Id = 3, Name = "هوش مصنوعی" },
                new ArticleCategory { Id = 4, Name = "ابر" },
                new ArticleCategory { Id = 5, Name = "DevOps" },
                new ArticleCategory { Id = 6, Name = "برنامه‌نویسی" },
                new ArticleCategory { Id = 7, Name = "مسیر شغلی" },
                new ArticleCategory { Id = 8, Name = "معماری نرم‌افزار" });
        });

        builder.Entity<Article>(entity =>
        {
            entity.ToTable("Article");
            entity.HasKey(article => article.Id);
            entity.Property(article => article.Title).HasMaxLength(60).IsRequired();
            entity.Property(article => article.ShortDescription).HasMaxLength(160).IsRequired();
            entity.Property(article => article.CoverImage).HasMaxLength(500).IsRequired();
            entity.Property(article => article.Status).HasMaxLength(20).HasDefaultValue("published").IsRequired();
            entity.Property(article => article.PublishedAt).IsRequired();
            entity.HasOne(article => article.Category).WithMany(category => category.Articles)
                .HasForeignKey(article => article.CategoryId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(article => article.Author).WithMany()
                .HasForeignKey(article => article.AuthorId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<ArticleSection>(entity =>
        {
            entity.ToTable("ArticleSection");
            entity.HasKey(section => section.Id);
            entity.Property(section => section.HeadingType).HasMaxLength(2);
            entity.Property(section => section.Title).HasMaxLength(100);
            entity.Property(section => section.Content).IsRequired();
            entity.Property(section => section.Order).HasColumnName("Order");
            entity.HasOne(section => section.Article).WithMany(article => article.Sections)
                .HasForeignKey(section => section.ArticleId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ArticleHashtag>(entity =>
        {
            entity.ToTable("ArticleHashtag");
            entity.HasKey(hashtag => new { hashtag.ArticleId, hashtag.Tag });
            entity.Property(hashtag => hashtag.Tag).HasMaxLength(40).IsRequired();
            entity.HasOne(hashtag => hashtag.Article).WithMany(article => article.Hashtags)
                .HasForeignKey(hashtag => hashtag.ArticleId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ArticleView>(entity =>
        {
            entity.ToTable("ArticleView");
            entity.HasKey(view => view.Id);
            entity.Property(view => view.ViewedAt).IsRequired();
            entity.HasIndex(view => view.ViewedAt);
            entity.HasOne(view => view.Article).WithMany()
                .HasForeignKey(view => view.ArticleId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ContactMessage>(entity =>
        {
            entity.ToTable("ContactMessages");
            entity.HasKey(message => message.Id);
            entity.Property(message => message.Name).HasMaxLength(120).IsRequired();
            entity.Property(message => message.Email).HasMaxLength(254).IsRequired();
            entity.Property(message => message.Subject).HasMaxLength(160).IsRequired();
            entity.Property(message => message.Message).HasMaxLength(5000).IsRequired();
            entity.Property(message => message.SentAt).IsRequired();
            entity.HasIndex(message => message.SentAt);
        });
        builder.Entity<SiteVisit>(entity =>
        {
            entity.ToTable("SiteVisit");
            entity.HasKey(visit => visit.Id);
            entity.Property(visit => visit.Path).HasMaxLength(500).IsRequired();
            entity.Property(visit => visit.VisitedAt).IsRequired();
            entity.HasIndex(visit => visit.VisitedAt);
        });
        builder.Entity<DashboardActivity>(entity =>
        {
            entity.ToTable("DashboardActivities");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Message).HasMaxLength(1000).IsRequired();
            entity.HasIndex(x => x.OccurredAt);
            entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
        });
    }
}
