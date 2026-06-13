using Application.SeedWork.Interfaces;
using Application.SeedWork.Models;
using Application.SeedWork.Queries;
using Domain.Entities;
using Domain.SeedWork.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Persons.Queries;

public sealed class DashboardPersonsQuery : TableQueryRequest, IRequest<PagedResult<PersonTableDto>>
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

    public static readonly TableQueryDefinition<Person, DashboardPersonsQuery> Table =
        new(
            Filters:
            [
                TableFilterDescriptorExtensions.GuidEquals<Person, DashboardPersonsQuery>(
                    "id",
                    query => query.Id,
                    person => person.Id
                ),
                TableFilterDescriptor<Person, DashboardPersonsQuery>.TextContains(
                    "type",
                    query => query.Type,
                    person => person.Type == PersonType.Company ? "Company" : "Individual"
                ),
                TableFilterDescriptor<Person, DashboardPersonsQuery>.TextContains(
                    "displayName",
                    query => query.DisplayName,
                    person =>
                        person.Type == PersonType.Company
                            ? person.CompanyName ?? string.Empty
                            : person.MiddleName == null
                                ? (person.FirstName ?? string.Empty) + " " + (person.LastName ?? string.Empty)
                                : (person.FirstName ?? string.Empty)
                                    + " "
                                    + (person.MiddleName ?? string.Empty)
                                    + " "
                                    + (person.LastName ?? string.Empty)
                ),
                TableFilterDescriptor<Person, DashboardPersonsQuery>.TextContains(
                    "firstName",
                    query => query.FirstName,
                    person => person.FirstName ?? string.Empty
                ),
                TableFilterDescriptor<Person, DashboardPersonsQuery>.TextContains(
                    "middleName",
                    query => query.MiddleName,
                    person => person.MiddleName ?? string.Empty
                ),
                TableFilterDescriptor<Person, DashboardPersonsQuery>.TextContains(
                    "lastName",
                    query => query.LastName,
                    person => person.LastName ?? string.Empty
                ),
                TableFilterDescriptor<Person, DashboardPersonsQuery>.TextContains(
                    "companyName",
                    query => query.CompanyName,
                    person => person.CompanyName ?? string.Empty
                ),
                TableFilterDescriptor<Person, DashboardPersonsQuery>.TextContains(
                    "egn",
                    query => query.Egn,
                    person => person.Egn ?? string.Empty
                ),
                TableFilterDescriptor<Person, DashboardPersonsQuery>.TextContains(
                    "eik",
                    query => query.Eik,
                    person => person.Eik ?? string.Empty
                ),
                TableFilterDescriptor<Person, DashboardPersonsQuery>.TextContains(
                    "vatNumber",
                    query => query.VatNumber,
                    person => person.VatNumber
                )
            ],
            Sorts: new Dictionary<string, TableSortDescriptor<Person, DashboardPersonsQuery>>(
                StringComparer.OrdinalIgnoreCase
            )
            {
                ["id"] = TableSortDescriptor<Person, DashboardPersonsQuery>.Create(
                    "id",
                    person => person.Id,
                    person => person.Id
                ),
                ["type"] = TableSortDescriptor<Person, DashboardPersonsQuery>.Create(
                    "type",
                    person => person.Type == PersonType.Company ? "Company" : "Individual",
                    person => person.Id
                ),
                ["displayName"] = TableSortDescriptor<Person, DashboardPersonsQuery>.Create(
                    "displayName",
                    person =>
                        person.Type == PersonType.Company
                            ? person.CompanyName ?? string.Empty
                            : person.MiddleName == null
                                ? (person.FirstName ?? string.Empty) + " " + (person.LastName ?? string.Empty)
                                : (person.FirstName ?? string.Empty)
                                    + " "
                                    + (person.MiddleName ?? string.Empty)
                                    + " "
                                    + (person.LastName ?? string.Empty),
                    person => person.Id
                ),
                ["firstName"] = TableSortDescriptor<Person, DashboardPersonsQuery>.Create(
                    "firstName",
                    person => person.FirstName ?? string.Empty,
                    person => person.Id
                ),
                ["middleName"] = TableSortDescriptor<Person, DashboardPersonsQuery>.Create(
                    "middleName",
                    person => person.MiddleName ?? string.Empty,
                    person => person.Id
                ),
                ["lastName"] = TableSortDescriptor<Person, DashboardPersonsQuery>.Create(
                    "lastName",
                    person => person.LastName ?? string.Empty,
                    person => person.Id
                ),
                ["companyName"] = TableSortDescriptor<Person, DashboardPersonsQuery>.Create(
                    "companyName",
                    person => person.CompanyName ?? string.Empty,
                    person => person.Id
                ),
                ["egn"] = TableSortDescriptor<Person, DashboardPersonsQuery>.Create(
                    "egn",
                    person => person.Egn ?? string.Empty,
                    person => person.Id
                ),
                ["eik"] = TableSortDescriptor<Person, DashboardPersonsQuery>.Create(
                    "eik",
                    person => person.Eik ?? string.Empty,
                    person => person.Id
                ),
                ["vatNumber"] = TableSortDescriptor<Person, DashboardPersonsQuery>.Create(
                    "vatNumber",
                    person => person.VatNumber,
                    person => person.Id
                )
            },
            DefaultSort: query =>
                query
                    .OrderBy(person =>
                        person.Type == PersonType.Company
                            ? person.CompanyName ?? string.Empty
                            : person.MiddleName == null
                                ? (person.FirstName ?? string.Empty) + " " + (person.LastName ?? string.Empty)
                                : (person.FirstName ?? string.Empty)
                                    + " "
                                    + (person.MiddleName ?? string.Empty)
                                    + " "
                                    + (person.LastName ?? string.Empty)
                    )
                    .ThenBy(person => person.Id)
        );
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
