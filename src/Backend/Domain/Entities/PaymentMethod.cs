namespace Backend.Domain.Entities;

public class PaymentMethod
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Provider { get; set; } = "bancard";
    public string AliasToken { get; set; } = string.Empty; // The "card token"
    public string? Brand { get; set; }
    public string? Last4 { get; set; }
    public bool IsDefault { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
}
