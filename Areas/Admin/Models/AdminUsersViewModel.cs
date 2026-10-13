namespace PeakLabs.Areas.Admin.Models;

public sealed class AdminUsersViewModel
{
    public string? CurrentUserId { get; init; }

    public IReadOnlyList<DashboardAccountListItemViewModel> Accounts { get; init; } = [];

    public int TotalCount => Accounts.Count;

    public int ActiveCount => Accounts.Count(account => account.IsActive);

    public int InactiveCount => TotalCount - ActiveCount;
}

public sealed class DashboardAccountListItemViewModel
{
    public string Id { get; init; } = string.Empty;

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public string UserName { get; init; } = string.Empty;

    public string RoleName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string? ProfileImagePath { get; init; }

    public string? AuthorDescription { get; init; }

    public bool IsActive { get; init; }

    public string RoleDisplayName { get; init; } = string.Empty;

    public string AccessDescription { get; init; } = string.Empty;

    public string Initials => string.Concat(
        FirstName.FirstOrDefault(),
        LastName.FirstOrDefault()).Trim('\0');
}
