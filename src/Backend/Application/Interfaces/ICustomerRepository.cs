using Backend.Domain.Entities;

namespace Backend.Application.Interfaces;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByRucAsync(string ruc, CancellationToken cancellationToken = default);
}
