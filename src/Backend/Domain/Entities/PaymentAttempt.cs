using Backend.Domain.Enums;

namespace Backend.Domain.Entities;

public class PaymentAttempt : BaseEntity
{
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "PYG";
    public string? ProviderTxId { get; set; }
    public PaymentStatus Status { get; set; }
    public string RawPayload { get; set; } = string.Empty; // JSON

    // Navigation properties
    public User User { get; set; } = null!;
}
