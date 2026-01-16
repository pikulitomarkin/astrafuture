using AstraFuture.Application.Common.Interfaces;
using AstraFuture.Domain.Entities;
using MediatR;

namespace AstraFuture.Application.Resources.Queries;

public record GetResourcesQuery(
    Guid TenantId,
    ResourceType? Type = null,
    bool? ActiveOnly = null
) : IRequest<IEnumerable<Resource>>;

public class GetResourcesQueryHandler : IRequestHandler<GetResourcesQuery, IEnumerable<Resource>>
{
    private readonly IResourceRepository _repository;

    public GetResourcesQueryHandler(IResourceRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Resource>> Handle(GetResourcesQuery request, CancellationToken cancellationToken)
    {
        if (request.ActiveOnly == true)
        {
            return await _repository.GetActiveAsync(request.TenantId);
        }

        if (request.Type.HasValue)
        {
            return await _repository.GetByTypeAsync(request.TenantId, request.Type.Value);
        }

        return await _repository.GetAllAsync(request.TenantId);
    }
}
