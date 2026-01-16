using AstraFuture.Application.Common.Interfaces;
using AstraFuture.Domain.Entities;
using MediatR;

namespace AstraFuture.Application.Customers.Queries;

public record GetCustomersQuery(Guid TenantId) : IRequest<IEnumerable<Customer>>;

public class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, IEnumerable<Customer>>
{
    private readonly ICustomerRepository _repository;

    public GetCustomersQueryHandler(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Customer>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetAllAsync(request.TenantId);
    }
}
