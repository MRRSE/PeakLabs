using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace PeakLabs.Areas.Admin.Models;

public class UpdatePortfolioItemInputModel
{
    [Range(1, int.MaxValue)]
    public int Id { get; set; }

    public IFormFile? ProjectImage { get; set; }

    [Required, StringLength(120)]
    public string ProjectName { get; set; } = string.Empty;

    [Required, StringLength(1000)]
    public string ProjectDescription { get; set; } = string.Empty;

    [Required, Url, StringLength(500)]
    public string ProjectLink { get; set; } = string.Empty;
}
