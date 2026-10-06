namespace PeakLabs.Models.Identity;

public static class DashboardRoles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string ContentManager = "ContentManager";
    public const string Editor = "Editor";

    public static bool IsKnown(string? roleName) => roleName is
        SuperAdmin or ContentManager or Editor;

    public static string GetDisplayName(string? roleName) => roleName switch
    {
        SuperAdmin => "مدیر ارشد",
        ContentManager => "مدیر محتوا",
        Editor => "ویرایشگر",
        _ => "بدون نقش مشخص",
    };

    public static string GetAccessDescription(string? roleName) => roleName switch
    {
        SuperAdmin => "همه بخش‌های داشبورد",
        ContentManager => "مدیریت محتوا و تیم",
        Editor => "نمای کلی و مقالات خودش",
        _ => "تعریف‌نشده",
    };
}
