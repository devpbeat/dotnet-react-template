using Backend.Domain.Enums;

namespace Backend.Domain.Entities;

public class Subscription : BaseEntity
{
    public Guid UserId { get; set; }
    public Plan Plan { get; set; }
    public SubscriptionStatus Status { get; set; }
    public DateTime CurrentPeriodStart { get; set; }
    public DateTime CurrentPeriodEnd { get; set; }
    public bool CancelAtPeriodEnd { get; set; }
    public DateTime? GraceUntil { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
}
