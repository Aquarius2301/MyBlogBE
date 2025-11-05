using System;

namespace WebApi.Dtos;

public class PaginationRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class PaginationResponse
{
    public object Items { get; set; } = null!;
    public string Cursor { get; set; } = null!;
    public int PageSize { get; set; }
}


