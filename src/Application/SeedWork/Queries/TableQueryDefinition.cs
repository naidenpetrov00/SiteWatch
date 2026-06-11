using System.Linq.Expressions;

namespace Application.SeedWork.Queries;

public sealed record TableFilterDescriptor<TEntity, TRequest>(
    string Key,
    Func<TRequest, Expression<Func<TEntity, bool>>?> BuildPredicate
)
    where TRequest : TableQueryRequest
{
    public static TableFilterDescriptor<TEntity, TRequest> TextContains(
        string key,
        Func<TRequest, string?> valueSelector,
        Expression<Func<TEntity, string>> selector
    )
    {
        return new TableFilterDescriptor<TEntity, TRequest>(
            key,
            request => TableFilterPredicates.TextContains(selector, valueSelector(request))
        );
    }

    public static TableFilterDescriptor<TEntity, TRequest> BooleanEquals(
        string key,
        Func<TRequest, bool?> valueSelector,
        Expression<Func<TEntity, bool>> selector
    )
    {
        return new TableFilterDescriptor<TEntity, TRequest>(
            key,
            request => TableFilterPredicates.BooleanEquals(selector, valueSelector(request))
        );
    }

    public static TableFilterDescriptor<TEntity, TRequest> DateTimeOffsetSearch(
        string key,
        Func<TRequest, string?> valueSelector,
        Expression<Func<TEntity, DateTimeOffset?>> selector
    )
    {
        return new TableFilterDescriptor<TEntity, TRequest>(
            key,
            request => TableFilterPredicates.DateTimeOffsetSearch(selector, valueSelector(request))
        );
    }
}

public sealed record TableSortDescriptor<TEntity, TRequest>(
    string Key,
    Func<IQueryable<TEntity>, bool, IOrderedQueryable<TEntity>> Apply
)
    where TRequest : TableQueryRequest
{
    public static TableSortDescriptor<TEntity, TRequest> Create<TKey>(
        string key,
        Expression<Func<TEntity, TKey>> selector
    )
        where TKey : IComparable
    {
        return new TableSortDescriptor<TEntity, TRequest>(
            key,
            (query, descending) =>
                descending ? query.OrderByDescending(selector) : query.OrderBy(selector)
        );
    }

    public static TableSortDescriptor<TEntity, TRequest> Create<TKey, TTieBreaker>(
        string key,
        Expression<Func<TEntity, TKey>> selector,
        Expression<Func<TEntity, TTieBreaker>> thenBy
    )
        where TKey : IComparable
        where TTieBreaker : IComparable
    {
        return new TableSortDescriptor<TEntity, TRequest>(
            key,
            (query, descending) =>
                descending
                    ? query.OrderByDescending(selector).ThenBy(thenBy)
                    : query.OrderBy(selector).ThenBy(thenBy)
        );
    }
}

public sealed record TableQueryDefinition<TEntity, TRequest>(
    IReadOnlyList<TableFilterDescriptor<TEntity, TRequest>> Filters,
    IReadOnlyDictionary<string, TableSortDescriptor<TEntity, TRequest>> Sorts,
    Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> DefaultSort
)
    where TRequest : TableQueryRequest;
