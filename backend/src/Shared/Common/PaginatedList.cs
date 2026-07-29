namespace MoleculeByMakeover.Shared.Common;

public class PaginatedList<T>
{
    public IReadOnlyCollection<T> Items { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public int TotalPages { get; }

    public PaginatedList(IReadOnlyCollection<T> items, int totalCount, int page, int pageSize)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = pageSize == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
    }

    public static PaginatedList<T> Create(IReadOnlyCollection<T> items, int totalCount, int page, int pageSize) =>
        new(items, totalCount, page, pageSize);
}

public class PaginationRequest
{
    private const int MaxPageSize = 100;
    private int _pageSize = 20;

    public int Page { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value <= 0 ? 20 : Math.Min(value, MaxPageSize);
    }

    public int Skip => (Math.Max(Page, 1) - 1) * PageSize;
}
