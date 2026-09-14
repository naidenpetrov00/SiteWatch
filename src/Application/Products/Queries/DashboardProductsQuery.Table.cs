using System.Globalization;
using System.Linq.Expressions;
using Application.SeedWork.Queries;
using Domain.Entities;
using Domain.SeedWork.Enums;

namespace Application.Products.Queries;

public sealed partial class DashboardProductsQuery
{
    public static readonly TableQueryDefinition<Product, DashboardProductsQuery> Table =
        new(
            Filters:
            [
                TableFilterDescriptorExtensions.IntEquals<Product, DashboardProductsQuery>(
                    "numberId", query => query.NumberId, product => product.NumberId),
                TableFilterDescriptorExtensions.GuidEquals<Product, DashboardProductsQuery>(
                    "id", query => query.Id, product => product.Id),
                TableFilterDescriptor<Product, DashboardProductsQuery>.TextContains(
                    "title", query => query.Title, product => product.Title),
                TableFilterDescriptor<Product, DashboardProductsQuery>.TextContains(
                    "category", query => query.Category, product => product.Category),
                TableFilterDescriptor<Product, DashboardProductsQuery>.TextContains(
                    "brand", query => query.Brand, product => product.Brand ?? string.Empty),
                TableFilterDescriptor<Product, DashboardProductsQuery>.TextContains(
                    "model", query => query.Model, product => product.Model ?? string.Empty),
                new TableFilterDescriptor<Product, DashboardProductsQuery>(
                    "packageQuantity",
                    request => BuildDecimalEqualsPredicate(
                        request.PackageQuantity,
                        product => product.PackageQuantity)),
                TableFilterDescriptor<Product, DashboardProductsQuery>.TextContains(
                    "packageUnit",
                    query => query.PackageUnit,
                    product => product.PackageUnit ?? string.Empty),
                new TableFilterDescriptor<Product, DashboardProductsQuery>(
                    "status",
                    request => BuildStatusPredicate(request.Status))
            ],
            Sorts: new Dictionary<string, TableSortDescriptor<Product, DashboardProductsQuery>>(
                StringComparer.OrdinalIgnoreCase)
            {
                ["numberId"] = TableSortDescriptor<Product, DashboardProductsQuery>.Create(
                    "numberId", product => product.NumberId, product => product.Id),
                ["id"] = TableSortDescriptor<Product, DashboardProductsQuery>.Create(
                    "id", product => product.Id),
                ["title"] = TableSortDescriptor<Product, DashboardProductsQuery>.Create(
                    "title", product => product.Title, product => product.Id),
                ["category"] = TableSortDescriptor<Product, DashboardProductsQuery>.Create(
                    "category", product => product.Category, product => product.Id),
                ["brand"] = TableSortDescriptor<Product, DashboardProductsQuery>.Create(
                    "brand", product => product.Brand ?? string.Empty, product => product.Id),
                ["model"] = TableSortDescriptor<Product, DashboardProductsQuery>.Create(
                    "model", product => product.Model ?? string.Empty, product => product.Id),
                ["packageQuantity"] = TableSortDescriptor<Product, DashboardProductsQuery>.Create(
                    "packageQuantity",
                    product => product.PackageQuantity ?? decimal.MinValue,
                    product => product.Id),
                ["packageUnit"] = TableSortDescriptor<Product, DashboardProductsQuery>.Create(
                    "packageUnit", product => product.PackageUnit ?? string.Empty, product => product.Id),
                ["status"] = TableSortDescriptor<Product, DashboardProductsQuery>.Create(
                    "status", product => product.Status, product => product.Id)
            },
            DefaultSort: query => query.OrderBy(product => product.Title).ThenBy(product => product.Id));

    private static Expression<Func<Product, bool>>? BuildStatusPredicate(string? rawValue)
    {
        if (!Enum.TryParse<ProductStatus>(rawValue?.Trim(), true, out var status)
            || !Enum.IsDefined(status))
        {
            return null;
        }

        return product => product.Status == status;
    }

    private static Expression<Func<Product, bool>>? BuildDecimalEqualsPredicate(
        string? rawValue,
        Expression<Func<Product, decimal?>> selector)
    {
        if (!decimal.TryParse(
                rawValue?.Trim(),
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var quantity))
        {
            return null;
        }

        var hasValue = Expression.Property(selector.Body, nameof(Nullable<decimal>.HasValue));
        var value = Expression.Property(selector.Body, nameof(Nullable<decimal>.Value));
        var body = Expression.AndAlso(
            hasValue,
            Expression.Equal(value, Expression.Constant(quantity)));

        return Expression.Lambda<Func<Product, bool>>(body, selector.Parameters);
    }
}
