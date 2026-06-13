using Application.SeedWork.Interfaces;
using MediatR;

namespace Application.Persons.Commands;

public sealed record DeletePersonCommand : IRequest
{
    public Guid Id { get; init; }
}

public sealed class DeletePersonHandler(IPersonService personService)
    : IRequestHandler<DeletePersonCommand>
{
    public Task Handle(DeletePersonCommand request, CancellationToken cancellationToken) =>
        personService.DeleteAsync(request.Id, cancellationToken);
}
