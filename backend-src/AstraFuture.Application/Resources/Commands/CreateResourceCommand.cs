using AstraFuture.Application.Common.Interfaces;
using AstraFuture.Domain.Entities;
using MediatR;

namespace AstraFuture.Application.Resources.Commands;

public record CreateResourceCommand(
    Guid TenantId,
    string Name,
    ResourceType Type,
    string? Description = null,
    string? Email = null,
    string? Phone = null,
    string Color = "#3B82F6"
) : IRequest<Guid>;

public class CreateResourceCommandHandler : IRequestHandler<CreateResourceCommand, Guid>
{
    private readonly IResourceRepository _repository;

    public CreateResourceCommandHandler(IResourceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateResourceCommand request, CancellationToken cancellationToken)
    {
        var resource = Resource.Create(
            request.TenantId,
            request.Name,
            request.Type,
            request.Description,
            request.Email,
            request.Phone,
            request.Color
        );

        return await _repository.CreateAsync(resource);
    }
}
