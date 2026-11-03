using System.ComponentModel.DataAnnotations;

namespace PeakLabs.Areas.Admin.Models;

public sealed class ProfileInputModel
{
    [Required(ErrorMessage = "نام را وارد کنید.")]
    [StringLength(60, ErrorMessage = "نام نمی‌تواند بیشتر از ۶۰ نویسه باشد.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "نام خانوادگی را وارد کنید.")]
    [StringLength(80, ErrorMessage = "نام خانوادگی نمی‌تواند بیشتر از ۸۰ نویسه باشد.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "نام کاربری را وارد کنید.")]
    [StringLength(50, ErrorMessage = "نام کاربری نمی‌تواند بیشتر از ۵۰ نویسه باشد.")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "ایمیل را وارد کنید.")]
    [EmailAddress(ErrorMessage = "ایمیل واردشده معتبر نیست.")]
    [StringLength(120, ErrorMessage = "ایمیل نمی‌تواند بیشتر از ۱۲۰ نویسه باشد.")]
    public string Email { get; set; } = string.Empty;

    [StringLength(240, ErrorMessage = "توضیحات نمی‌تواند بیشتر از ۲۴۰ نویسه باشد.")]
    public string? AuthorDescription { get; set; }

    [Url(ErrorMessage = "لینک GitHub معتبر نیست.")]
    [StringLength(200)]
    public string? GitHubUrl { get; set; }

    [Url(ErrorMessage = "لینک LinkedIn معتبر نیست.")]
    [StringLength(200)]
    public string? LinkedInUrl { get; set; }

    public IFormFile? ProfileImage { get; set; }

    [DataType(DataType.Password)]
    public string? CurrentPassword { get; set; }

    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "رمز عبور جدید باید حداقل ۸ نویسه داشته باشد.")]
    public string? NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "رمز عبور جدید و تکرار آن یکسان نیستند.")]
    public string? ConfirmNewPassword { get; set; }

    public string? ProfileImagePath { get; init; }

    public string FullName { get; init; } = string.Empty;

    public string RoleDisplayName { get; init; } = string.Empty;

    public string AccessDescription { get; init; } = string.Empty;
}
