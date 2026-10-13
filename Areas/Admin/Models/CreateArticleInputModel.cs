using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace PeakLabs.Areas.Admin.Models;

public class CreateArticleInputModel
{
    [Required, StringLength(60, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(160, MinimumLength = 1)]
    public string ShortDescription { get; set; } = string.Empty;

    public IFormFile? CoverImage { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    [Required, RegularExpression("^(draft|published)$")]
    public string Status { get; set; } = "published";

    [MinLength(1)]
    public List<CreateArticleSectionInputModel> Sections { get; set; } = [];
}

public class CreateArticleSectionInputModel
{
    [StringLength(2)]
    public string? HeadingType { get; set; }

    [StringLength(100)]
    public string? Title { get; set; }

    [Required, StringLength(50000, MinimumLength = 1)]
    public string Content { get; set; } = string.Empty;
}
