using Application.SeedWork.Queries;
using Domain.Entities;

namespace Application.Sites.Queries;

public sealed partial class DashboardSitesQuery
{
    public static readonly TableQueryDefinition<Site, DashboardSitesQuery> Table =
        new(
            Filters:
            [
                TableFilterDescriptorExtensions.IntEquals<Site, DashboardSitesQuery>(
                    "numberId",
                    query => query.NumberId,
                    site => site.NumberId
                ),
                TableFilterDescriptorExtensions.GuidEquals<Site, DashboardSitesQuery>(
                    "id",
                    query => query.Id,
                    site => site.Id
                ),
                TableFilterDescriptor<Site, DashboardSitesQuery>.TextContains(
                    "name",
                    query => query.Name,
                    site => site.Name.Value
                ),
                TableFilterDescriptor<Site, DashboardSitesQuery>.TextContains(
                    "address",
                    query => query.Address,
                    site => site.Address.Value
                )
            ],
            Sorts: new Dictionary<string, TableSortDescriptor<Site, DashboardSitesQuery>>(
                StringComparer.OrdinalIgnoreCase
            )
            {
                ["numberId"] = TableSortDescriptor<Site, DashboardSitesQuery>.Create(
                    "numberId",
                    site => site.NumberId,
                    site => site.Id
                ),
                ["id"] = TableSortDescriptor<Site, DashboardSitesQuery>.Create(
                    "id",
                    site => site.Id
                ),
                ["name"] = TableSortDescriptor<Site, DashboardSitesQuery>.Create(
                    "name",
                    site => site.Name.Value,
                    site => site.Id
                ),
                ["address"] = TableSortDescriptor<Site, DashboardSitesQuery>.Create(
                    "address",
                    site => site.Address.Value,
                    site => site.Id
                )
            },
            DefaultSort: query => query
                .OrderBy(site => site.Name.Value)
                .ThenBy(site => site.Id)
        );
}
