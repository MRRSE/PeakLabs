using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PeakLabs.Areas.Admin.Models;
using PeakLabs.Models.Identity;

namespace PeakLabs.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public sealed class AccountController : Controller
{
    private const long MaxProfileImageSize = 2 * 1024 * 1024;

    private readonly UserManager<PeakLabsUser> _userManager;
    private readonly SignInManager<PeakLabsUser> _signInManager;
    private readonly IWebHostEnvironment _environment;

    public AccountController(
        UserManager<PeakLabsUser> userManager,
        SignInManager<PeakLabsUser> signInManager,
        IWebHostEnvironment environment)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _environment = environment;
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        return View(await CreateInputModelAsync(user));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(MaxProfileImageSize + 64 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxProfileImageSize + 64 * 1024)]
    public async Task<IActionResult> Profile(ProfileInputModel input)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        var changesPassword = !string.IsNullOrWhiteSpace(input.NewPassword)
            || !string.IsNullOrWhiteSpace(input.ConfirmNewPassword)
            || !string.IsNullOrWhiteSpace(input.CurrentPassword);

        if (changesPassword && string.IsNullOrWhiteSpace(input.CurrentPassword))
        {
            ModelState.AddModelError(nameof(input.CurrentPassword), "برای تغییر رمز، رمز عبور فعلی را وارد کنید.");
        }

        if (changesPassword && string.IsNullOrWhiteSpace(input.NewPassword))
        {
            ModelState.AddModelError(nameof(input.NewPassword), "رمز عبور جدید را وارد کنید.");
        }

        if (input.ProfileImage is { Length: > MaxProfileImageSize })
        {
            ModelState.AddModelError(nameof(input.ProfileImage), "حجم تصویر باید کمتر از ۲ مگابایت باشد.");
        }

        if (changesPassword && !string.IsNullOrWhiteSpace(input.CurrentPassword)
            && !await _userManager.CheckPasswordAsync(user, input.CurrentPassword))
        {
            ModelState.AddModelError(nameof(input.CurrentPassword), "رمز عبور فعلی درست نیست.");
        }

        if (changesPassword && !string.IsNullOrWhiteSpace(input.NewPassword))
        {
            foreach (var validator in _userManager.PasswordValidators)
            {
                var result = await validator.ValidateAsync(_userManager, user, input.NewPassword);
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(nameof(input.NewPassword), TranslateIdentityError(error.Code));
                }
            }
        }

        if (!ModelState.IsValid)
        {
            return View(await CreateInputModelAsync(user, input));
        }

        string? newImagePath = null;
        if (input.ProfileImage is not null)
        {
            var imageResult = await SaveProfileImageAsync(input.ProfileImage);
            if (!imageResult.Succeeded)
            {
                ModelState.AddModelError(nameof(input.ProfileImage), imageResult.Error!);
                return View(await CreateInputModelAsync(user, input));
            }

            newImagePath = imageResult.Path;
        }

        var oldImagePath = user.ProfileImagePath;
        user.FirstName = input.FirstName.Trim();
        user.LastName = input.LastName.Trim();
        user.UserName = input.UserName.Trim();
        user.Email = input.Email.Trim();
        user.AuthorDescription = NormalizeOptionalText(input.AuthorDescription);
        user.GitHubUrl = NormalizeSocialUrl(input.GitHubUrl);
        user.LinkedInUrl = NormalizeSocialUrl(input.LinkedInUrl);
        if (newImagePath is not null)
        {
            user.ProfileImagePath = newImagePath;
        }

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            DeleteProfileImage(newImagePath);
            AddIdentityErrors(updateResult, nameof(input.UserName));
            return View(await CreateInputModelAsync(user, input));
        }

        if (changesPassword)
        {
            var passwordResult = await _userManager.ChangePasswordAsync(user, input.CurrentPassword!, input.NewPassword!);
            if (!passwordResult.Succeeded)
            {
                AddIdentityErrors(passwordResult, nameof(input.NewPassword));
                return View(await CreateInputModelAsync(user, input));
            }
        }

        if (newImagePath is not null)
        {
            DeleteProfileImage(oldImagePath);
        }

        await _userManager.UpdateSecurityStampAsync(user);
        await _signInManager.RefreshSignInAsync(user);

        TempData["ProfileSuccess"] = "اطلاعات پروفایل با موفقیت ذخیره شد.";
        return RedirectToAction(nameof(Profile));
    }

    private async Task<ProfileInputModel> CreateInputModelAsync(PeakLabsUser user, ProfileInputModel? input = null)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var roleName = roles.FirstOrDefault();

        var model = input ?? new ProfileInputModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            AuthorDescription = user.AuthorDescription,
            GitHubUrl = user.GitHubUrl,
            LinkedInUrl = user.LinkedInUrl,
        };

        return new ProfileInputModel
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            UserName = model.UserName,
            Email = model.Email,
            AuthorDescription = model.AuthorDescription,
            GitHubUrl = model.GitHubUrl,
            LinkedInUrl = model.LinkedInUrl,
            ProfileImage = model.ProfileImage,
            CurrentPassword = model.CurrentPassword,
            NewPassword = model.NewPassword,
            ConfirmNewPassword = model.ConfirmNewPassword,
            ProfileImagePath = user.ProfileImagePath,
            FullName = $"{user.FirstName} {user.LastName}".Trim(),
            RoleDisplayName = DashboardRoles.GetDisplayName(roleName),
            AccessDescription = DashboardRoles.GetAccessDescription(roleName),
        };
    }

    private async Task<(bool Succeeded, string? Path, string? Error)> SaveProfileImageAsync(IFormFile image)
    {
        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
        if (image.Length == 0 || !IsSupportedImage(image, extension))
        {
            return (false, null, "فایل انتخاب‌شده تصویر معتبر PNG، JPG یا WebP نیست.");
        }

        var directory = Path.Combine(_environment.WebRootPath, "uploads", "admin-profiles");
        Directory.CreateDirectory(directory);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        await using var stream = System.IO.File.Create(Path.Combine(directory, fileName));
        await image.CopyToAsync(stream);

        return (true, $"/uploads/admin-profiles/{fileName}", null);
    }

    private void DeleteProfileImage(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return;

        var fileName = Path.GetFileName(relativePath);
        var physicalPath = Path.Combine(_environment.WebRootPath, "uploads", "admin-profiles", fileName);
        if (System.IO.File.Exists(physicalPath))
        {
            System.IO.File.Delete(physicalPath);
        }
    }

    private static bool IsSupportedImage(IFormFile image, string extension)
    {
        var allowedTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".webp"] = "image/webp",
        };

        if (!allowedTypes.TryGetValue(extension, out var contentType)
            || !string.Equals(image.ContentType, contentType, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        Span<byte> header = stackalloc byte[12];
        using var stream = image.OpenReadStream();
        var bytesRead = stream.Read(header);

        return extension switch
        {
            ".jpg" or ".jpeg" => bytesRead >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            ".png" => bytesRead >= 8 && header[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
            ".webp" => bytesRead >= 12 && header[..4].SequenceEqual("RIFF"u8) && header[8..12].SequenceEqual("WEBP"u8),
            _ => false,
        };
    }

    private void AddIdentityErrors(IdentityResult result, string key)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(key, TranslateIdentityError(error.Code));
        }
    }

    private static string TranslateIdentityError(string code) => code switch
    {
        "DuplicateUserName" => "این نام کاربری قبلاً استفاده شده است.",
        "DuplicateEmail" => "این ایمیل قبلاً برای حساب دیگری ثبت شده است.",
        "PasswordTooShort" => "رمز عبور باید حداقل ۸ نویسه باشد.",
        "PasswordRequiresDigit" => "رمز عبور باید حداقل یک عدد داشته باشد.",
        "PasswordRequiresLower" => "رمز عبور باید حداقل یک حرف کوچک انگلیسی داشته باشد.",
        "PasswordRequiresUpper" => "رمز عبور باید حداقل یک حرف بزرگ انگلیسی داشته باشد.",
        "PasswordRequiresNonAlphanumeric" => "رمز عبور باید حداقل یک نماد داشته باشد.",
        _ => "اطلاعات واردشده معتبر نیست.",
    };

    private static string? NormalizeOptionalText(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizeSocialUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        var trimmed = value.Trim();
        return Uri.TryCreate(trimmed, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
                ? uri.ToString()
                : null;
    }
}
