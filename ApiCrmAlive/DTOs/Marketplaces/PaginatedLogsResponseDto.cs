namespace ApiCrmAlive.DTOs.Marketplaces;

public class PaginatedLogsResponseDto
{
    public IEnumerable<MarketplaceLogDto> Data { get; set; } = new List<MarketplaceLogDto>();
    public object Pagination { get; set; } = new();
}
