namespace CleanArchTemplate.Application.Common.Models;

public class PaginatedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = new List<T>();
    public PaginationMetadata Pagination { get; set; } = default!;
}

public class PaginationMetadata
{
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
    public int ItemsPerPage { get; set; }
    public bool HasNextPage => CurrentPage < TotalPages;
    public bool HasPreviousPage => CurrentPage > 1;

    public static PaginationMetadata Create(int currentPage, int pageSize, int totalItems)
    {
        return new PaginationMetadata
        {
            CurrentPage = currentPage,
            ItemsPerPage = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }
}
