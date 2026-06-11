using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Application.SeedWork.Models;

namespace Application.SeedWork.Queries;

public static class TableQueryEvaluator
{
    public static async Task<PagedResult<TResult>> ToPagedResultAsync<TEntity, TResult, TRequest>(
        this IQueryable<TEntity> source,
        TRequest request,
        TableQueryDefinition<TEntity, TRequest> definition,
        Func<IQueryable<TEntity>, IQueryable<TResult>> projection,
        CancellationToken cancellationToken
    )
        where TRequest : TableQueryRequest
    {
        var totalCount = await source.CountAsync(cancellationToken);
        var filteredQuery = ApplyFilters(source, request, definition);
        var filteredCount = await filteredQuery.CountAsync(cancellationToken);
        var sortedQuery = ApplySort(filteredQuery, request, definition);
        var pagedQuery = sortedQuery
            .Skip(request.PageIndex * request.PageSize)
            .Take(request.PageSize);

        var items = await projection(pagedQuery).ToListAsync(cancellationToken);

        return new PagedResult<TResult>(items, filteredCount, totalCount);
    }

    private static IQueryable<TEntity> ApplyFilters<TEntity, TRequest>(
        IQueryable<TEntity> source,
        TRequest request,
        TableQueryDefinition<TEntity, TRequest> definition
    )
        where TRequest : TableQueryRequest
    {
        Expression<Func<TEntity, bool>>? predicate = null;

        foreach (var filter in definition.Filters)
        {
            var filterPredicate = filter.BuildPredicate(request);
            if (filterPredicate is null)
            {
                continue;
            }

            predicate = predicate is null
                ? filterPredicate
                : predicate.AndAlso(filterPredicate);
        }

        return predicate is null ? source : source.Where(predicate);
    }

    private static IQueryable<TEntity> ApplySort<TEntity, TRequest>(
        IQueryable<TEntity> source,
        TRequest request,
        TableQueryDefinition<TEntity, TRequest> definition
    )
        where TRequest : TableQueryRequest
    {
        var sortKey = request.SortActive?.Trim();
        var isDescending = string.Equals(
            request.SortDirection?.Trim(),
            "desc",
            StringComparison.OrdinalIgnoreCase
        );

        if (
            !string.IsNullOrWhiteSpace(sortKey)
            && definition.Sorts.TryGetValue(sortKey, out var sortDescriptor)
        )
        {
            return sortDescriptor.Apply(source, isDescending);
        }

        return definition.DefaultSort(source);
    }
}
