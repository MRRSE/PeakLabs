using PeakLabs.Models;

namespace PeakLabs.Areas.Admin.Models;

public sealed record ContactMessagesIndexViewModel(
    IReadOnlyList<ContactMessage> Messages,
    bool Answered,
    int PendingCount,
    int AnsweredCount,
    int CurrentPage,
    int TotalPages);
