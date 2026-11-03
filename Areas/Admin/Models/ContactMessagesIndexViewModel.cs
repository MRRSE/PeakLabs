using PeakLabs.Models;

namespace PeakLabs.Areas.Admin.Models;

public sealed record ContactMessagesIndexViewModel(
    IReadOnlyList<ContactMessage> Messages,
    int CurrentPage,
    int TotalPages);
