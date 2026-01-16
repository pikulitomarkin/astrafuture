using AstraFuture.Application.Common.Interfaces;
using MediatR;

namespace AstraFuture.Application.Customers.Commands;

public record UpdateCustomerCommand(
    Guid Id,
    string Name,
    string Email,
    string? Phone
) : IRequest<bool>;

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, bool>
{
    private readonly ICustomerRepository _repository;

    public UpdateCustomerCommandHandler(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _repository.GetByIdAsync(request.Id);
        if (customer == null)
            return false;

        customer.UpdateContactInfo(
            request.Name,
            request.Email,
            request.Phone
        );

        await _repository.UpdateAsync(customer);
        return true;
    }
}
