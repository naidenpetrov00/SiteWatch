using Application.SeedWork.Queries;
using Domain.Entities;

namespace Application.Retailers.Queries;

public sealed partial class DashboardRetailersQuery
{
    public static readonly TableQueryDefinition<Retailer, DashboardRetailersQuery> Table =
        new(
            Filters:
            [
                TableFilterDescriptor<Retailer, DashboardRetailersQuery>.TextContains(
                    "displayName", query => query.DisplayName, retailer => retailer.DisplayName),
                TableFilterDescriptor<Retailer, DashboardRetailersQuery>.TextContains(
                    "companyDisplayName",
                    query => query.CompanyDisplayName,
                    retailer => retailer.CompanyPerson.CompanyName ?? string.Empty),
                TableFilterDescriptor<Retailer, DashboardRetailersQuery>.TextContains(
                    "websiteHost",
                    query => query.WebsiteHost,
                    retailer => retailer.NormalizedWebsiteHost),
                TableFilterDescriptor<Retailer, DashboardRetailersQuery>.BooleanEquals(
                    "isActive", query => query.IsActive, retailer => retailer.IsActive)
            ],
            Sorts: new Dictionary<string, TableSortDescriptor<Retailer, DashboardRetailersQuery>>(
                StringComparer.OrdinalIgnoreCase)
            {
                ["displayName"] = TableSortDescriptor<Retailer, DashboardRetailersQuery>.Create(
                    "displayName", retailer => retailer.DisplayName, retailer => retailer.Id),
                ["companyDisplayName"] = TableSortDescriptor<Retailer, DashboardRetailersQuery>.Create(
                    "companyDisplayName",
                    retailer => retailer.CompanyPerson.CompanyName ?? string.Empty,
                    retailer => retailer.Id),
                ["websiteHost"] = TableSortDescriptor<Retailer, DashboardRetailersQuery>.Create(
                    "websiteHost", retailer => retailer.NormalizedWebsiteHost, retailer => retailer.Id),
                ["isActive"] = TableSortDescriptor<Retailer, DashboardRetailersQuery>.Create(
                    "isActive", retailer => retailer.IsActive, retailer => retailer.Id)
            },
            DefaultSort: query =>
                query.OrderBy(retailer => retailer.DisplayName).ThenBy(retailer => retailer.Id));
}
