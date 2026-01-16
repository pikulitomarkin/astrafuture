using AstraFuture.Application.Common.Interfaces;
using AstraFuture.Domain.Entities;
using MediatR;

namespace AstraFuture.Application.Resources.Commands;

public record UpdateResourceCommand(
    Guid Id,
    string Name,
    string? Description,
    string? Email,
    string? Phone,
    string? Color
) : IRequest<bool>;

public class UpdateResourceCommandHandler : IRequestHandler<UpdateResourceCommand, bool>
{
    private readonly IResourceRepository _repository;

    public UpdateResourceCommandHandler(IResourceRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(UpdateResourceCommand request, CancellationToken cancellationToken)
    {
        var resource = await _repository.GetByIdAsync(request.Id);
        if (resource == null)
            return false;

        resource.UpdateDetails(
            request.Name,
            request.Description,
            request.Email,
            request.Phone
        );

        if (!string.IsNullOrWhiteSpace(request.Color))
        {
            resource.ChangeColor(request.Color);
        }

        await _repository.UpdateAsync(resource);
        return true;
    }
}
