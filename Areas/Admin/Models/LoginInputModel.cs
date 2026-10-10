using System.ComponentModel.DataAnnotations;

namespace PeakLabs.Areas.Admin.Models;

public sealed class LoginInputModel
{
    [Required(ErrorMessage = "نام کاربری را وارد کنید.")]
    [StringLength(50, ErrorMessage = "نام کاربری نمی‌تواند بیشتر از ۵۰ نویسه باشد.")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "رمز عبور را وارد کنید.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "رمز عبور باید حداقل ۸ نویسه باشد.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}
