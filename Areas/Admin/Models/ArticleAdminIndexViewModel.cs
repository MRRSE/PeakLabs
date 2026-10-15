using PeakLabs.Models;

namespace PeakLabs.Areas.Admin.Models;

public class ArticleAdminIndexViewModel
{
    public IReadOnlyList<Article> Articles { get; init; } = [];
    public IReadOnlyList<ArticleCategory> Categories { get; init; } = [];
}
