using MediatR;

namespace Backend.Application.Commands;

public record CustomerDto(Guid Id, string RazonSocial, string Name, string LastName, string Ruc, string Email, string PhoneNumber);

public record CreateCustomerCommand(string RazonSocial, string Name, string LastName, string Ruc, string Email, string PhoneNumber) : IRequest<CustomerDto>;

public record UpdateCustomerCommand(Guid Id, string RazonSocial, string Name, string LastName, string Ruc, string Email, string PhoneNumber) : IRequest<CustomerDto>;

public record DeleteCustomerCommand(Guid Id) : IRequest<bool>;

public record GetCustomerByIdQuery(Guid Id) : IRequest<CustomerDto?>;

public record GetAllCustomersQuery() : IRequest<IEnumerable<CustomerDto>>;
