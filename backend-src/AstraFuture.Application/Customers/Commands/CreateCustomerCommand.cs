using AstraFuture.Application.Common.Interfaces;
using AstraFuture.Domain.Entities;
using MediatR;

namespace AstraFuture.Application.Customers.Commands;

public record CreateCustomerCommand(
    Guid TenantId,
    string Name,
    string Email,
    string? Phone = null,
    string CustomerType = "individual"
) : IRequest<Guid>;

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Guid>
{
    private readonly ICustomerRepository _repository;

    public CreateCustomerCommandHandler(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = Customer.Create(
            request.TenantId,
            request.Name,
            request.Email,
            request.Phone,
            request.CustomerType
        );

        return await _repository.CreateAsync(customer);
    }
}
