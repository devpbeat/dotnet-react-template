using Backend.Application.Commands;
using Backend.Application.Interfaces;
using Backend.Domain.Entities;
using MediatR;

namespace Backend.Application.Handlers;

public class CustomerHandlers :
    IRequestHandler<CreateCustomerCommand, CustomerDto>,
    IRequestHandler<UpdateCustomerCommand, CustomerDto>,
    IRequestHandler<DeleteCustomerCommand, bool>,
    IRequestHandler<GetCustomerByIdQuery, CustomerDto?>,
    IRequestHandler<GetAllCustomersQuery, IEnumerable<CustomerDto>>
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerHandlers(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<CustomerDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var existingCustomer = await _customerRepository.GetByRucAsync(request.Ruc, cancellationToken);
        if (existingCustomer != null)
        {
            throw new Exception($"Customer with RUC {request.Ruc} already exists.");
        }

        var customer = new Customer
        {
            RazonSocial = request.RazonSocial,
            Name = request.Name,
            LastName = request.LastName,
            Ruc = request.Ruc,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber
        };

        await _customerRepository.AddAsync(customer, cancellationToken);
        await _customerRepository.SaveChangesAsync(cancellationToken);

        return MapToDto(customer);
    }

    public async Task<CustomerDto> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken);
        if (customer == null)
        {
            throw new Exception($"Customer with ID {request.Id} not found.");
        }

        // Check if RUC is being changed to one that already exists
        if (customer.Ruc != request.Ruc)
        {
            var existingCustomer = await _customerRepository.GetByRucAsync(request.Ruc, cancellationToken);
            if (existingCustomer != null)
            {
                throw new Exception($"Customer with RUC {request.Ruc} already exists.");
            }
        }

        customer.RazonSocial = request.RazonSocial;
        customer.Name = request.Name;
        customer.LastName = request.LastName;
        customer.Ruc = request.Ruc;
        customer.Email = request.Email;
        customer.PhoneNumber = request.PhoneNumber;
        customer.UpdatedAt = DateTime.UtcNow;

        await _customerRepository.UpdateAsync(customer, cancellationToken);
        await _customerRepository.SaveChangesAsync(cancellationToken);

        return MapToDto(customer);
    }

    public async Task<bool> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken);
        if (customer == null)
        {
            return false;
        }

        await _customerRepository.SoftDeleteAsync(customer, cancellationToken);
        await _customerRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<CustomerDto?> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken);
        return customer == null ? null : MapToDto(customer);
    }

    public async Task<IEnumerable<CustomerDto>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
    {
        var customers = await _customerRepository.GetAllAsync(cancellationToken);
        return customers.Select(MapToDto);
    }

    private static CustomerDto MapToDto(Customer customer)
    {
        return new CustomerDto(
            customer.Id,
            customer.RazonSocial,
            customer.Name,
            customer.LastName,
            customer.Ruc,
            customer.Email,
            customer.PhoneNumber
        );
    }
}
