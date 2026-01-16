using AstraFuture.Application.Common.Interfaces;
using MediatR;

namespace AstraFuture.Application.Resources.Commands;

public record DeleteResourceCommand(Guid Id) : IRequest<bool>;

public class DeleteResourceCommandHandler : IRequestHandler<DeleteResourceCommand, bool>
{
    private readonly IResourceRepository _repository;

    public DeleteResourceCommandHandler(IResourceRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteResourceCommand request, CancellationToken cancellationToken)
    {
        var exists = await _repository.ExistsAsync(request.Id);
        if (!exists)
            return false;

        await _repository.DeleteAsync(request.Id);
        return true;
    }
}
