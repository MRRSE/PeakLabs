using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using PeakLabs.Data;
using PeakLabs.Models.Identity;

namespace PeakLabs.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class DashboardController : Controller
{
    private readonly UserManager<PeakLabsUser> _userManager;
    private readonly ApplicationDbContext _dbContext;

    public DashboardController(UserManager<PeakLabsUser> userManager, ApplicationDbContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        ViewData["WelcomeName"] = string.IsNullOrWhiteSpace(user?.FirstName)
            ? user?.UserName ?? User.Identity?.Name ?? "کاربر"
            : user.FirstName;
        ViewData["ArticleCategories"] = await _dbContext.ArticleCategories.AsNoTracking()
            .OrderBy(category => category.Name).ToListAsync();
        var publishedArticles = _dbContext.Articles.AsNoTracking()
            .Where(article => article.Status == "published");
        var now = DateTimeOffset.UtcNow;
        var currentMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var previousMonth = currentMonth.AddMonths(-1);

        var publishedCount = await publishedArticles.CountAsync();
        var articleViewTotal = await publishedArticles.SumAsync(article => (long?)article.ViewCount) ?? 0;
        var portfolioCount = await _dbContext.Portfolio.CountAsync();
        var activeAccountCount = await _userManager.Users.CountAsync(user => user.IsActive);
        ViewData["PublishedArticleCount"] = publishedCount;
        ViewData["ArticleViewTotal"] = articleViewTotal;
        ViewData["PortfolioCount"] = portfolioCount;
        ViewData["ActiveAccountCount"] = activeAccountCount;
        ViewData["ArticleViewsTrend"] = await CalculateTrendAsync(
            _dbContext.ArticleViews.Where(view => view.Article.Status == "published").Select(view => view.ViewedAt), currentMonth, previousMonth);
        ViewData["PublishedArticlesTrend"] = await CalculateTrendAsync(
            publishedArticles.Select(article => article.PublishedAt), currentMonth, previousMonth);
        ViewData["PortfolioTrend"] = await CalculateTrendAsync(
            _dbContext.Portfolio.Select(item => item.AddedAt), currentMonth, previousMonth);
        ViewData["AccountsTrend"] = await CalculateTrendAsync(
            _userManager.Users.Where(user => user.IsActive).Select(user => user.CreatedAt), currentMonth, previousMonth);
        var topArticlesQuery = publishedArticles;
        if (User.IsInRole(DashboardRoles.Editor))
        {
            var currentUserId = _userManager.GetUserId(User);
            topArticlesQuery = topArticlesQuery.Where(article => article.AuthorId == currentUserId);
        }

        ViewData["TopArticles"] = await topArticlesQuery
            .Include(article => article.Author)
            .OrderByDescending(article => article.ViewCount)
            .ThenByDescending(article => article.PublishedAt)
            .Take(6)
            .ToListAsync();
        var trendEvents = await _dbContext.ArticleViews.AsNoTracking()
            .Where(view => view.ViewedAt >= DateTimeOffset.UtcNow.AddYears(-1).AddDays(-1))
            .Select(view => new ArticleViewPoint(view.ViewedAt, view.Article.Category.Name))
            .ToListAsync();
        var categories = await _dbContext.ArticleCategories.AsNoTracking()
            .Where(category => category.Articles.Any(article => article.Status == "published"))
            .OrderBy(category => category.Name)
            .Select(category => category.Name)
            .ToListAsync();
        ViewData["ArticleViewCategories"] = categories;
        ViewData["ArticleViewTrends"] = new Dictionary<string, object>
        {
            ["daily"] = BuildViewTrend(trendEvents, categories, "daily"),
            ["weekly"] = BuildViewTrend(trendEvents, categories, "weekly"),
            ["monthly"] = BuildViewTrend(trendEvents, categories, "monthly"),
            ["yearly"] = BuildViewTrend(trendEvents, categories, "yearly")
        };
        var trafficEvents = await _dbContext.SiteVisits.AsNoTracking()
            .Where(visit => visit.VisitedAt >= now.AddYears(-1).AddDays(-1))
            .Select(visit => visit.VisitedAt)
            .ToListAsync();
        ViewData["SiteTrafficTrends"] = new Dictionary<string, object>
        {
            ["daily"] = BuildSimpleTrend(trafficEvents, "daily"),
            ["weekly"] = BuildSimpleTrend(trafficEvents, "weekly"),
            ["monthly"] = BuildSimpleTrend(trafficEvents, "monthly"),
            ["yearly"] = BuildSimpleTrend(trafficEvents, "yearly")
        };
        return View();
    }

    private async Task<TrendSummary> CalculateTrendAsync(IQueryable<DateTimeOffset> dates, DateTimeOffset currentMonth, DateTimeOffset previousMonth)
    {
        var current = await dates.CountAsync(date => date >= currentMonth && date < currentMonth.AddMonths(1));
        var previous = await dates.CountAsync(date => date >= previousMonth && date < currentMonth);
        var percentage = previous == 0 ? (current == 0 ? 0 : 100) : Math.Round((current - previous) * 100m / previous, 1);
        return new TrendSummary(current, previous, percentage);
    }

    private static object BuildSimpleTrend(IReadOnlyList<DateTimeOffset> events, string period)
    {
        var now = DateTime.Now;
        var bucketCount = period switch { "daily" => 24, "weekly" => 7, "monthly" => 30, _ => 12 };
        var values = new int[bucketCount];
        var labels = new string[bucketCount];
        foreach (var item in events)
        {
            var local = item.ToLocalTime();
            var bucket = period switch
            {
                "daily" => local.Date == now.Date ? local.Hour : -1,
                "yearly" => (local.Year - new DateTime(now.Year, now.Month, 1).AddMonths(-11).Year) * 12 + local.Month - new DateTime(now.Year, now.Month, 1).AddMonths(-11).Month,
                _ => (local.Date - now.Date.AddDays(-(period == "weekly" ? 6 : 29))).Days
            };
            if (bucket >= 0 && bucket < bucketCount) values[bucket]++;
        }
        var culture = CultureInfo.GetCultureInfo("fa-IR");
        for (var i = 0; i < bucketCount; i++)
        {
            labels[i] = period == "daily" ? $"{i:00}:00" : period == "yearly"
                ? new DateTime(now.Year, now.Month, 1).AddMonths(i - 11).ToString("MMM", culture)
                : now.Date.AddDays(i - (period == "weekly" ? 6 : 29)).ToString(period == "weekly" ? "ddd" : "d", culture);
        }
        return new { labels, datasets = new[] { new { label = "ترافیک سایت", data = values } } };
    }

    public sealed record TrendSummary(long Current, long Previous, decimal Percentage);

    private static object BuildViewTrend(IReadOnlyList<ArticleViewPoint> events, IReadOnlyList<string> categories, string period)
    {
        var now = DateTime.Now;
        var bucketCount = period switch
        {
            "daily" => 24,
            "weekly" => 7,
            "monthly" => 30,
            _ => 12
        };
        var values = categories.Select(_ => new int[bucketCount]).ToArray();
        var categoryIndexes = categories.Select((category, index) => (category, index))
            .ToDictionary(item => item.category, item => item.index, StringComparer.Ordinal);
        var labels = new string[bucketCount];
        var culture = CultureInfo.GetCultureInfo("fa-IR");

        foreach (var view in events)
        {
            if (!categoryIndexes.TryGetValue(view.CategoryName, out var categoryIndex)) continue;
            var local = view.ViewedAt.ToLocalTime();
            var bucket = -1;
            if (period == "daily")
            {
                if (local.Date == now.Date) bucket = local.Hour;
            }
            else if (period == "yearly")
            {
                var startMonth = new DateTime(now.Year, now.Month, 1).AddMonths(-11);
                bucket = (local.Year - startMonth.Year) * 12 + local.Month - startMonth.Month;
            }
            else
            {
                var days = period == "weekly" ? 7 : 30;
                var startDate = now.Date.AddDays(-days + 1);
                bucket = (local.Date - startDate).Days;
            }

            if (bucket >= 0 && bucket < bucketCount) values[categoryIndex][bucket]++;
        }

        if (period == "daily")
        {
            for (var hour = 0; hour < bucketCount; hour++)
                labels[hour] = new DateTime(2000, 1, 1, hour, 0, 0).ToString("HH:mm", culture);
        }
        else if (period == "yearly")
        {
            var startMonth = new DateTime(now.Year, now.Month, 1).AddMonths(-11);
            for (var month = 0; month < bucketCount; month++)
                labels[month] = startMonth.AddMonths(month).ToString("MMM", culture);
        }
        else
        {
            var days = period == "weekly" ? 7 : 30;
            var startDate = now.Date.AddDays(-days + 1);
            for (var day = 0; day < bucketCount; day++)
            {
                var date = startDate.AddDays(day);
                labels[day] = period == "weekly" ? date.ToString("ddd", culture) : date.ToString("d", culture);
            }
        }

        var datasets = categories.Select((category, index) => new { label = category, data = values[index] }).ToArray();
        return new { labels, datasets };
    }

    private sealed record ArticleViewPoint(DateTimeOffset ViewedAt, string CategoryName);
}



