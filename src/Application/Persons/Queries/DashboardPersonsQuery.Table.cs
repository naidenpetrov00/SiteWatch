using Application.SeedWork.Queries;
using Domain.Entities;
using Domain.SeedWork.Enums;

namespace Application.Persons.Queries;

public sealed partial class DashboardPersonsQuery
{
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
                ["numberId"] = TableSortDescriptor<Person, DashboardPersonsQuery>.Create(
                    "numberId",
                    person => person.NumberId,
                    person => person.Id
                ),
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
