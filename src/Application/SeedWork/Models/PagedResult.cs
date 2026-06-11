namespace Application.SeedWork.Models;

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int FilteredCount,
    int TotalCount
);
