using AstraFuture.Application.Common.Interfaces;
using MediatR;

namespace AstraFuture.Application.Customers.Commands;

public record DeleteCustomerCommand(Guid Id) : IRequest<bool>;

public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, bool>
{
    private readonly ICustomerRepository _repository;

    public DeleteCustomerCommandHandler(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var exists = await _repository.ExistsAsync(request.Id);
        if (!exists)
            return false;

        await _repository.DeleteAsync(request.Id);
        return true;
    }
}
