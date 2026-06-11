namespace Application.SeedWork.Queries;

public abstract class TableQueryRequest
{
    public int PageIndex { get; set; } = 0;
    public int PageSize { get; set; } = 50;
    public string? SortActive { get; set; }
    public string? SortDirection { get; set; }
}
