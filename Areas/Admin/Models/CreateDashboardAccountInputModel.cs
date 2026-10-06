using System.ComponentModel.DataAnnotations;
using PeakLabs.Models.Identity;

namespace PeakLabs.Areas.Admin.Models;

public sealed class CreateDashboardAccountInputModel
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

    [Required(ErrorMessage = "نقش حساب را انتخاب کنید.")]
    public string RoleName { get; set; } = string.Empty;

    [StringLength(240, ErrorMessage = "توضیحات نقش نمی‌تواند بیشتر از ۲۴۰ نویسه باشد.")]
    public string? AuthorDescription { get; set; }

    public bool IsActive { get; set; } = true;

    [Required(ErrorMessage = "رمز عبور اولیه را وارد کنید.")]
    [MinLength(8, ErrorMessage = "رمز عبور باید حداقل ۸ نویسه باشد.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "تکرار رمز عبور را وارد کنید.")]
    [Compare(nameof(Password), ErrorMessage = "رمز عبور و تکرار آن یکسان نیستند.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public IFormFile? ProfileImage { get; set; }
}
