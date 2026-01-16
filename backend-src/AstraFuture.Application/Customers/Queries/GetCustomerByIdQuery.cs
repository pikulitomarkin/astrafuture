using AstraFuture.Application.Common.Interfaces;
using AstraFuture.Domain.Entities;
using MediatR;

namespace AstraFuture.Application.Customers.Queries;

public record GetCustomerByIdQuery(Guid Id) : IRequest<Customer?>;

public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, Customer?>
{
    private readonly ICustomerRepository _repository;

    public GetCustomerByIdQueryHandler(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<Customer?> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetByIdAsync(request.Id);
    }
}
