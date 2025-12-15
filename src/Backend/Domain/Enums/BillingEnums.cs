namespace Backend.Domain.Enums;

public enum Plan
{
    Free,
    Starter,
    Pro
}

public enum SubscriptionStatus
{
    Trialing,
    Active,
    PastDue,
    Canceled
}

public enum PaymentStatus
{
    Pending,
    Approved,
    Rejected
}
