using Backend.Domain.Entities;

namespace Backend.Domain.Entities;

public class Customer : BaseEntity
{
    public string RazonSocial { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Ruc { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}
