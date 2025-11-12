using WebApi.Settings;

namespace WebApi.Dtos;

public class PaginationRequest
{
    public DateTime? Cursor { get; set; }

    private int _pageSize;
    public int PageSize
    {
        get { return _pageSize; }
        set
        {
            if (value <= 0 || value > 50)
                _pageSize = 10;
            else
                _pageSize = value;
        }
    }
}

public class PaginationResponse
{
    public object Items { get; set; } = null!;
    public DateTime? Cursor { get; set; }
    public int PageSize { get; set; }
}
