using PeakLabs.Data;
using PeakLabs.Models;

namespace PeakLabs.Infrastructure;

public sealed class SiteTrafficMiddleware
{
    private static readonly string[] StaticExtensions = [
        ".css", ".js", ".map", ".png", ".jpg", ".jpeg", ".webp", ".gif", ".svg", ".ico", ".woff", ".woff2"
    ];

    private readonly RequestDelegate _next;

    public SiteTrafficMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
    {
        await _next(context);

        if (context.Request.Method == HttpMethods.Get
            && context.Response.StatusCode is >= 200 and < 400
            && IsPublicPage(context.Request.Path))
        {
            dbContext.SiteVisits.Add(new SiteVisit
            {
                Path = context.Request.Path.Value ?? "/",
                VisitedAt = DateTimeOffset.UtcNow,
            });

            await dbContext.SaveChangesAsync();
        }
    }

    private static bool IsPublicPage(PathString path)
    {
        var value = path.Value ?? string.Empty;
        if (value.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/api", StringComparison.OrdinalIgnoreCase)) return false;

        return !StaticExtensions.Any(extension => value.EndsWith(extension, StringComparison.OrdinalIgnoreCase));
    }
}
