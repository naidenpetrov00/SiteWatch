using System.Linq.Expressions;

namespace Application.SeedWork.Queries;

public static class TableFilterDescriptorExtensions
{
    public static TableFilterDescriptor<TEntity, TRequest> GuidEquals<TEntity, TRequest>(
        string key,
        Func<TRequest, string?> valueSelector,
        Expression<Func<TEntity, Guid>> selector
    )
        where TRequest : TableQueryRequest
    {
        return new TableFilterDescriptor<TEntity, TRequest>(
            key,
            request => TableFilterPredicates.GuidEquals(selector, valueSelector(request))
        );
    }

    public static TableFilterDescriptor<TEntity, TRequest> IntEquals<TEntity, TRequest>(
        string key,
        Func<TRequest, string?> valueSelector,
        Expression<Func<TEntity, int>> selector
    )
        where TRequest : TableQueryRequest
    {
        return new TableFilterDescriptor<TEntity, TRequest>(
            key,
            request => TableFilterPredicates.IntEquals(selector, valueSelector(request))
        );
    }

    public static TableFilterDescriptor<TEntity, TRequest> DateOnlySearch<TEntity, TRequest>(
        string key,
        Func<TRequest, string?> valueSelector,
        Expression<Func<TEntity, DateOnly?>> selector
    )
        where TRequest : TableQueryRequest
    {
        return new TableFilterDescriptor<TEntity, TRequest>(
            key,
            request => TableFilterPredicates.DateOnlySearch(selector, valueSelector(request))
        );
    }
}
