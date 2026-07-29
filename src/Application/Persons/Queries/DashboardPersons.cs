using Application.SeedWork.Interfaces;
using Application.SeedWork.Models;
using Application.SeedWork.Queries;
using Application.SeedWork.Security;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Persons.Queries;

[Authorize(Roles = UserRoles.Administrator)]
public sealed partial class DashboardPersonsQuery
    : TableQueryRequest,
        IRequest<PagedResult<PersonTableDto>>
{
    public string? Id { get; set; }
    public string? Type { get; set; }
    public string? DisplayName { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string? CompanyName { get; set; }
    public string? Egn { get; set; }
    public string? Eik { get; set; }
    public string? VatNumber { get; set; }
}

public sealed class DashboardPersonsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<DashboardPersonsQuery, PagedResult<PersonTableDto>>
{
    public async Task<PagedResult<PersonTableDto>> Handle(
        DashboardPersonsQuery request,
        CancellationToken cancellationToken
    )
    {
        var result = await dbContext.Persons
            .AsNoTracking()
            .ToPagedResultAsync<Person, Person, DashboardPersonsQuery>(
                request,
                DashboardPersonsQuery.Table,
                query => query,
                cancellationToken
            );

        return new PagedResult<PersonTableDto>(
            result.Items.Select(PersonTableDto.From).ToList(),
            result.FilteredCount,
            result.TotalCount
        );
    }
}
