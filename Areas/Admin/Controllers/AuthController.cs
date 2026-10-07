using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeakLabs.Areas.Admin.Models;
using PeakLabs.Data;
using PeakLabs.Models.Identity;

namespace PeakLabs.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class AuthController : Controller
{
    private const long MaxProfileImageSize = 2 * 1024 * 1024;
    private const int MaxDashboardAccounts = 10;

    private readonly UserManager<PeakLabsUser> _userManager;
    private readonly SignInManager<PeakLabsUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IWebHostEnvironment _environment;
    private readonly ApplicationDbContext _dbContext;

    public AuthController(
        UserManager<PeakLabsUser> userManager,
        SignInManager<PeakLabsUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        IWebHostEnvironment environment,
        ApplicationDbContext dbContext)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _environment = environment;
        _dbContext = dbContext;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
        }

        return View(new LoginInputModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginInputModel input)
    {
        if (!ModelState.IsValid)
        {
            return View(input);
        }

        var user = await _userManager.FindByNameAsync(input.UserName.Trim());
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "نام کاربری یا رمز عبور اشتباه است.");
            return View(input);
        }

        if (!user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "این حساب غیرفعال است. با مدیر سیستم تماس بگیرید.");
            return View(input);
        }

        var signInResult = await _signInManager.PasswordSignInAsync(
            user,
            input.Password,
            input.RememberMe,
            lockoutOnFailure: true);

        if (signInResult.Succeeded)
        {
            if (!string.IsNullOrWhiteSpace(input.ReturnUrl) && Url.IsLocalUrl(input.ReturnUrl))
            {
                return LocalRedirect(input.ReturnUrl);
            }

            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
        }

        var errorMessage = signInResult.IsLockedOut
            ? "به‌دلیل چند تلاش ناموفق، حساب موقتاً قفل شده است. کمی بعد دوباره تلاش کنید."
            : signInResult.IsNotAllowed
                ? "ورود برای این حساب در حال حاضر مجاز نیست."
                : "نام کاربری یا رمز عبور اشتباه است.";
        ModelState.AddModelError(string.Empty, errorMessage);
        return View(input);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    public IActionResult Profile()
    {
        return View();
    }
    [Authorize(Roles = DashboardRoles.SuperAdmin)]
    public async Task<IActionResult> AdminUsers()
    {
        var users = await _userManager.Users
            .OrderBy(user => user.LastName)
            .ThenBy(user => user.FirstName)
            .ToListAsync();

        var accounts = new List<DashboardAccountListItemViewModel>(users.Count);
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var roleName = roles.FirstOrDefault();

            accounts.Add(new DashboardAccountListItemViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName ?? string.Empty,
                RoleName = roleName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                ProfileImagePath = user.ProfileImagePath,
                AuthorDescription = user.AuthorDescription,
                IsActive = user.IsActive,
                RoleDisplayName = DashboardRoles.GetDisplayName(roleName),
                AccessDescription = DashboardRoles.GetAccessDescription(roleName),
            });
        }

        return View(new AdminUsersViewModel
        {
            CurrentUserId = _userManager.GetUserId(User),
            Accounts = accounts,
        });
    }

    [HttpPost]
    [Authorize(Roles = DashboardRoles.SuperAdmin)]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(MaxProfileImageSize + 64 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxProfileImageSize + 64 * 1024)]
    public async Task<IActionResult> CreateAccount(CreateDashboardAccountInputModel input)
    {
        if (await _userManager.Users.CountAsync() >= MaxDashboardAccounts)
        {
            return Conflict(new { errors = new[] { "حداکثر ۱۰ حساب داشبورد می‌توان ساخت." } });
        }

        if (!string.IsNullOrWhiteSpace(input.RoleName) && !DashboardRoles.IsKnown(input.RoleName))
        {
            ModelState.AddModelError(nameof(input.RoleName), "نقش انتخاب‌شده معتبر نیست.");
        }

        if (input.ProfileImage is { Length: > MaxProfileImageSize })
        {
            ModelState.AddModelError(nameof(input.ProfileImage), "حجم تصویر باید کمتر از ۲ مگابایت باشد.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                errors = ModelState.Values
                    .SelectMany(value => value.Errors)
                    .Select(error => error.ErrorMessage)
                    .Where(message => !string.IsNullOrWhiteSpace(message))
                    .Distinct()
                    .ToArray(),
            });
        }

        string? profileImagePath = null;
        if (input.ProfileImage is not null)
        {
            var imageSaveResult = await SaveProfileImageAsync(input.ProfileImage);
            if (!imageSaveResult.Succeeded)
            {
                return BadRequest(new { errors = new[] { imageSaveResult.Error } });
            }

            profileImagePath = imageSaveResult.Path;
        }

        var user = new PeakLabsUser
        {
            FirstName = input.FirstName.Trim(),
            LastName = input.LastName.Trim(),
            UserName = input.UserName.Trim(),
            Email = input.Email.Trim(),
            AuthorDescription = string.IsNullOrWhiteSpace(input.AuthorDescription)
                ? null
                : input.AuthorDescription.Trim(),
            ProfileImagePath = profileImagePath,
            IsActive = input.IsActive,
        };

        var createResult = await _userManager.CreateAsync(user, input.Password);
        if (!createResult.Succeeded)
        {
            DeleteProfileImage(profileImagePath);
            return BadRequest(new { errors = TranslateIdentityErrors(createResult) });
        }

        if (!await _roleManager.RoleExistsAsync(input.RoleName))
        {
            var roleResult = await _roleManager.CreateAsync(new IdentityRole(input.RoleName));
            if (!roleResult.Succeeded && !await _roleManager.RoleExistsAsync(input.RoleName))
            {
                await _userManager.DeleteAsync(user);
                DeleteProfileImage(profileImagePath);
                return BadRequest(new { errors = TranslateIdentityErrors(roleResult) });
            }
        }

        var addToRoleResult = await _userManager.AddToRoleAsync(user, input.RoleName);
        if (!addToRoleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            DeleteProfileImage(profileImagePath);
            return BadRequest(new { errors = TranslateIdentityErrors(addToRoleResult) });
        }

        return Ok(new { message = "حساب کاربری ساخته شد." });
    }

    [HttpPost]
    [Authorize(Roles = DashboardRoles.SuperAdmin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAccount([FromForm] string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new { message = "حساب انتخاب‌شده معتبر نیست." });
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound(new { message = "حساب موردنظر پیدا نشد." });
        }

        if (string.Equals(user.Id, _userManager.GetUserId(User), StringComparison.Ordinal))
        {
            return BadRequest(new { message = "نمی‌توانید حسابی را که با آن وارد شده‌اید حذف کنید." });
        }

        var deleteResult = await _userManager.DeleteAsync(user);
        if (!deleteResult.Succeeded)
        {
            return BadRequest(new { message = "حذف حساب انجام نشد. دوباره تلاش کنید." });
        }

        DeleteProfileImage(user.ProfileImagePath);
        return Ok(new { message = "حساب کاربری حذف شد." });
    }

    [HttpPost]
    [Authorize(Roles = DashboardRoles.SuperAdmin)]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(MaxProfileImageSize + 64 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxProfileImageSize + 64 * 1024)]
    public async Task<IActionResult> UpdateAccount(EditDashboardAccountInputModel input)
    {
        if (!DashboardRoles.IsKnown(input.RoleName))
        {
            ModelState.AddModelError(nameof(input.RoleName), "نقش انتخاب‌شده معتبر نیست.");
        }

        if (!string.IsNullOrEmpty(input.Password) && string.IsNullOrEmpty(input.ConfirmPassword))
        {
            ModelState.AddModelError(nameof(input.ConfirmPassword), "تکرار رمز عبور جدید را وارد کنید.");
        }

        if (input.ProfileImage is { Length: > MaxProfileImageSize })
        {
            ModelState.AddModelError(nameof(input.ProfileImage), "حجم تصویر باید کمتر از ۲ مگابایت باشد.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                errors = ModelState.Values
                    .SelectMany(value => value.Errors)
                    .Select(error => error.ErrorMessage)
                    .Where(message => !string.IsNullOrWhiteSpace(message))
                    .Distinct()
                    .ToArray(),
            });
        }

        var user = await _userManager.FindByIdAsync(input.Id);
        if (user is null)
        {
            return NotFound(new { errors = new[] { "حساب موردنظر پیدا نشد." } });
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        var oldImagePath = user.ProfileImagePath;
        string? newImagePath = null;
        if (input.ProfileImage is not null)
        {
            var imageSaveResult = await SaveProfileImageAsync(input.ProfileImage);
            if (!imageSaveResult.Succeeded)
            {
                return BadRequest(new { errors = new[] { imageSaveResult.Error } });
            }

            newImagePath = imageSaveResult.Path;
        }

        if (!await _roleManager.RoleExistsAsync(input.RoleName))
        {
            var roleResult = await _roleManager.CreateAsync(new IdentityRole(input.RoleName));
            if (!roleResult.Succeeded && !await _roleManager.RoleExistsAsync(input.RoleName))
            {
                DeleteProfileImage(newImagePath);
                return BadRequest(new { errors = TranslateIdentityErrors(roleResult) });
            }
        }

        user.FirstName = input.FirstName.Trim();
        user.LastName = input.LastName.Trim();
        user.UserName = input.UserName.Trim();
        user.Email = input.Email.Trim();
        user.AuthorDescription = string.IsNullOrWhiteSpace(input.AuthorDescription)
            ? null
            : input.AuthorDescription.Trim();
        if (newImagePath is not null)
        {
            user.ProfileImagePath = newImagePath;
        }

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            DeleteProfileImage(newImagePath);
            return BadRequest(new { errors = TranslateIdentityErrors(updateResult) });
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Count != 1 || !currentRoles.Contains(input.RoleName, StringComparer.Ordinal))
        {
            if (currentRoles.Count > 0)
            {
                var removeRolesResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeRolesResult.Succeeded)
                {
                    DeleteProfileImage(newImagePath);
                    return BadRequest(new { errors = TranslateIdentityErrors(removeRolesResult) });
                }
            }

            var addRoleResult = await _userManager.AddToRoleAsync(user, input.RoleName);
            if (!addRoleResult.Succeeded)
            {
                DeleteProfileImage(newImagePath);
                return BadRequest(new { errors = TranslateIdentityErrors(addRoleResult) });
            }
        }

        if (!string.IsNullOrEmpty(input.Password))
        {
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var passwordResult = await _userManager.ResetPasswordAsync(user, resetToken, input.Password);
            if (!passwordResult.Succeeded)
            {
                DeleteProfileImage(newImagePath);
                return BadRequest(new { errors = TranslateIdentityErrors(passwordResult) });
            }
        }

        await transaction.CommitAsync();

        if (newImagePath is not null)
        {
            DeleteProfileImage(oldImagePath);
        }

        return Ok(new { message = "اطلاعات حساب به‌روزرسانی شد." });
    }

    [HttpPost]
    [Authorize(Roles = DashboardRoles.SuperAdmin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetAccountActive([FromForm] string id, [FromForm] bool isActive)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new { message = "حساب انتخاب‌شده معتبر نیست." });
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound(new { message = "حساب موردنظر پیدا نشد." });
        }

        if (!isActive && string.Equals(user.Id, _userManager.GetUserId(User), StringComparison.Ordinal))
        {
            return BadRequest(new { message = "نمی‌توانید حساب فعلی خودتان را غیرفعال کنید." });
        }

        user.IsActive = isActive;
        user.SecurityStamp = Guid.NewGuid().ToString();
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return BadRequest(new { message = "وضعیت حساب به‌روزرسانی نشد." });
        }

        return Ok(new { message = isActive ? "حساب فعال شد." : "حساب غیرفعال شد." });
    }

    private async Task<(bool Succeeded, string? Path, string? Error)> SaveProfileImageAsync(IFormFile image)
    {
        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
        if (image.Length == 0 || !IsSupportedImage(image, extension))
        {
            return (false, null, "فایل انتخاب‌شده تصویر معتبر PNG، JPG یا WebP نیست.");
        }

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var relativePath = Path.Combine("uploads", "admin-profiles", fileName)
            .Replace(Path.DirectorySeparatorChar, '/');
        var directory = Path.Combine(_environment.WebRootPath, "uploads", "admin-profiles");
        Directory.CreateDirectory(directory);

        var physicalPath = Path.Combine(directory, fileName);
        await using (var stream = System.IO.File.Create(physicalPath))
        {
            await image.CopyToAsync(stream);
        }

        return (true, $"/{relativePath}", null);
    }

    private void DeleteProfileImage(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return;
        }

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
            ".jpg" or ".jpeg" => bytesRead >= 3
                && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            ".png" => bytesRead >= 8
                && header[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
            ".webp" => bytesRead >= 12
                && header[..4].SequenceEqual("RIFF"u8)
                && header[8..12].SequenceEqual("WEBP"u8),
            _ => false,
        };
    }

    private static string[] TranslateIdentityErrors(IdentityResult result) => result.Errors
        .Select(error => error.Code switch
        {
            "DuplicateUserName" => "این نام کاربری قبلاً استفاده شده است.",
            "DuplicateEmail" => "این ایمیل قبلاً برای حساب دیگری ثبت شده است.",
            "PasswordTooShort" => "رمز عبور باید حداقل ۸ نویسه باشد.",
            "PasswordRequiresDigit" => "رمز عبور باید حداقل یک عدد داشته باشد.",
            "PasswordRequiresLower" => "رمز عبور باید حداقل یک حرف کوچک انگلیسی داشته باشد.",
            "PasswordRequiresUpper" => "رمز عبور باید حداقل یک حرف بزرگ انگلیسی داشته باشد.",
            "PasswordRequiresNonAlphanumeric" => "رمز عبور باید حداقل یک نماد داشته باشد.",
            _ => "ساخت حساب انجام نشد. اطلاعات واردشده را بررسی کنید.",
        })
        .Distinct()
        .ToArray();
}
