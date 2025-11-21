using System;
using WebApi.Dtos;

namespace WebApi.Middlewares;

public class AppException : Exception
{
    public int StatusCode { get; }

    public AppException(string message, int statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }
}

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AppException ex)
        {
            context.Response.StatusCode = ex.StatusCode;
            context.Response.ContentType = "application/json";

            var response = new ApiResponse(ex.StatusCode, ex.Message);

            await context.Response.WriteAsJsonAsync(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");

            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";

            var response = new ApiResponse(500, "Internal Server Error");

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
