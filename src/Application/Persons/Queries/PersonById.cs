using Application.SeedWork.Interfaces;
using Application.SeedWork.Security;
using Ardalis.GuardClauses;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Persons.Queries;

[Authorize(Roles = UserRoles.Administrator)]
public sealed record PersonByIdQuery : IRequest<PersonDetailsDto>
{
    public Guid PersonId { get; init; }
}

public sealed class PersonByIdQueryHandler(IApplicationDbContext dbContext, IMapper mapper)
    : IRequestHandler<PersonByIdQuery, PersonDetailsDto>
{
    public async Task<PersonDetailsDto> Handle(PersonByIdQuery request, CancellationToken cancellationToken)
    {
        var person = await dbContext.Persons
            .AsNoTracking()
            .Where(x => x.Id == request.PersonId)
            .ProjectTo<PersonDetailsDto>(mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(cancellationToken);

        if (person is null)
        {
            throw new NotFoundException(nameof(Person), request.PersonId.ToString());
        }

        return person;
    }
}
