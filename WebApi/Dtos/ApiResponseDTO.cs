using Microsoft.AspNetCore.Mvc;

namespace WebApi.Dtos;

public class ApiResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }

    public ApiResponse(int statusCode, string message, object? data = null)
    {
        StatusCode = statusCode;
        Message = message;
        Data = data;
    }

    public static IActionResult Success(object? data = null, string message = "Success")
    {
        var res = new ApiResponse(200, message, data);
        return new ObjectResult(res) { StatusCode = 200 };
    }

    public static IActionResult Created(
        object? data = null,
        string message = "Created successfully"
    )
    {
        var res = new ApiResponse(201, message, data);
        return new ObjectResult(res) { StatusCode = 201 };
    }

    public static IActionResult Conflict(string message = "Conflict", object? data = null)
    {
        var res = new ApiResponse(409, message, data);
        return new ObjectResult(res) { StatusCode = 409 };
    }

    public static IActionResult BadRequest(string message = "Bad request", object? data = null)
    {
        var res = new ApiResponse(400, message, data);
        return new ObjectResult(res) { StatusCode = 400 };
    }

    public static IActionResult Unauthorized(string message = "Unauthorized")
    {
        var res = new ApiResponse(401, message);
        return new ObjectResult(res) { StatusCode = 401 };
    }

    public static IActionResult Forbidden(string message = "Forbidden")
    {
        var res = new ApiResponse(403, message);
        return new ObjectResult(res) { StatusCode = 403 };
    }

    public static IActionResult NotFound(string message = "Not found")
    {
        var res = new ApiResponse(404, message);
        return new ObjectResult(res) { StatusCode = 404 };
    }

    public static IActionResult Error(string message = "Internal server error", object? data = null)
    {
        var res = new ApiResponse(500, message, data);
        return new ObjectResult(res) { StatusCode = 500 };
    }

    public static IActionResult Custom(int statusCode, string message, object? data = null)
    {
        var res = new ApiResponse(statusCode, message, data);
        return new ObjectResult(res) { StatusCode = statusCode };
    }
}
