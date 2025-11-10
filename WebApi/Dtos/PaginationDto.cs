using WebApi.Settings;

namespace WebApi.Dtos;

public class PaginationRequest
{
    public DateTime? Cursor { get; set; }
    public int PageSize { get; set; }

    public void ApplyDefaults(BaseSettings settings)
    {
        if (PageSize <= 0)
            PageSize = settings.PageSize;
        else if (PageSize > settings.MaxPageSize)
            PageSize = settings.MaxPageSize;
    }
}

public class PaginationResponse
{
    public object Items { get; set; } = null!;
    public DateTime? Cursor { get; set; }
    public int PageSize { get; set; }
}
