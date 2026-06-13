using Application.Persons.Commands;

namespace Application.SeedWork.Interfaces;

public interface IPersonService
{
    Task<Guid> CreateAsync(PersonUpsertDto request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
