namespace PeakLabs.Models.Identity;

public static class DashboardRoles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string ContentManager = "ContentManager";
    public const string Editor = "Editor";
    public const string CustomerSupport = "CustomerSupport";

    public static bool IsKnown(string? roleName) => roleName is
        SuperAdmin or ContentManager or Editor or CustomerSupport;

    public static string GetDisplayName(string? roleName) => roleName switch
    {
        SuperAdmin => "مدیر ارشد",
        ContentManager => "مدیر محتوا",
        Editor => "ویرایشگر",
        CustomerSupport => "پشتیبان",
        _ => "بدون نقش مشخص",
    };

    public static string GetAccessDescription(string? roleName) => roleName switch
    {
        SuperAdmin => "همه بخش‌های داشبورد",
        ContentManager => "مدیریت مقاله‌ها (به‌جز مقاله‌های مدیر ارشد)، نمونه‌کار، تیم، پیام‌ها و دسته‌بندی‌ها",
        Editor => "مشاهده و ویرایش مقاله‌های خودش؛ بدون ایجاد یا حذف مقاله",
        CustomerSupport => "مشاهده مقالات و رسیدگی به پیام‌های کاربران",
        _ => "تعریف‌نشده",
    };
}
