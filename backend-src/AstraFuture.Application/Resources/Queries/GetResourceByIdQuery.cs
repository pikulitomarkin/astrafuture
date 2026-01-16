using AstraFuture.Application.Common.Interfaces;
using AstraFuture.Domain.Entities;
using MediatR;

namespace AstraFuture.Application.Resources.Queries;

public record GetResourceByIdQuery(Guid Id) : IRequest<Resource?>;

public class GetResourceByIdQueryHandler : IRequestHandler<GetResourceByIdQuery, Resource?>
{
    private readonly IResourceRepository _repository;

    public GetResourceByIdQueryHandler(IResourceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Resource?> Handle(GetResourceByIdQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetByIdAsync(request.Id);
    }
}
