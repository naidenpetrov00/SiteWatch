using Application.SeedWork.Interfaces;
using MediatR;

namespace Application.Persons.Commands;

public sealed record CreatePersonCommand : PersonUpsertDto, IRequest<Guid>;

public sealed class CreatePersonHandler(IPersonService personService)
    : IRequestHandler<CreatePersonCommand, Guid>
{
    public Task<Guid> Handle(CreatePersonCommand request, CancellationToken cancellationToken) =>
        personService.CreateAsync(request, cancellationToken);
}
