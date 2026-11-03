using System.ComponentModel.DataAnnotations;

namespace PeakLabs.Models;

public sealed class ContactMessage
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(160)]
    public string Subject { get; set; } = string.Empty;

    [Required, StringLength(5000, MinimumLength = 1)]
    public string Message { get; set; } = string.Empty;

    public DateTimeOffset SentAt { get; set; }
}
