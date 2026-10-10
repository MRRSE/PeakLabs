using System.ComponentModel.DataAnnotations;

namespace PeakLabs.Areas.Admin.Models;

public sealed class EditDashboardAccountInputModel
{
    [Required]
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "نام را وارد کنید.")]
    [StringLength(60)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "نام خانوادگی را وارد کنید.")]
    [StringLength(80)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "نام کاربری را وارد کنید.")]
    [StringLength(50)]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "ایمیل را وارد کنید.")]
    [EmailAddress(ErrorMessage = "ایمیل واردشده معتبر نیست.")]
    [StringLength(120)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "نقش را انتخاب کنید.")]
    public string RoleName { get; set; } = string.Empty;

    [StringLength(240)]
    public string? AuthorDescription { get; set; }

    public IFormFile? ProfileImage { get; set; }

    [StringLength(100, MinimumLength = 8, ErrorMessage = "رمز عبور جدید باید حداقل ۸ نویسه داشته باشد.")]
    public string? Password { get; set; }

    [Compare(nameof(Password), ErrorMessage = "رمز عبور و تکرار آن یکسان نیستند.")]
    public string? ConfirmPassword { get; set; }
}
