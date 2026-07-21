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
}
