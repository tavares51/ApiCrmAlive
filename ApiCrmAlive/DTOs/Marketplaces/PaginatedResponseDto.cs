namespace ApiCrmAlive.DTOs.Marketplaces;

public class PaginatedResponseDto<T>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }

    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();

    public PaginatedResponseDto() { }

    public PaginatedResponseDto(IEnumerable<T> items, int totalItems, int page, int pageSize)
    {
        Page = page;
        PageSize = pageSize;
        TotalItems = totalItems;
        TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        Items = [.. items];
    }
}