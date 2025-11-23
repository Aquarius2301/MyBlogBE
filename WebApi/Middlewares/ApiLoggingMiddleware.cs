using WebApi.Loggers;

namespace WebApi.Middlewares;

public class ApiLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ApiLogger _logger;

    public ApiLoggingMiddleware(RequestDelegate next, ApiLogger logger)
    {
        _next = next;
        Directory.CreateDirectory("Logs");
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Enable buffering so we can read the request body multiple times
        context.Request.EnableBuffering();

        await LogRequest(context);

        // Capture the original response body stream
        var originalResponseBody = context.Response.Body;

        // Create a new memory stream to capture the response
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        // Call the next middleware
        if (_next != null)
        {
            await _next(context);
        }

        // Log the response
        await LogResponse(context, responseBody);

        // Copy the response back to the original stream
        responseBody.Seek(0, SeekOrigin.Begin);
        await responseBody.CopyToAsync(originalResponseBody);
    }

    private async Task LogRequest(HttpContext context)
    {
        var method = context.Request.Method;
        var path = context.Request.Path;
        var query = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : "";
        var contentType = context.Request.ContentType ?? "";

        // Read the request body
        string bodyContent = "";

        // Check if content type is multipart/form-data (file upload)
        if (contentType.Contains("multipart/form-data"))
        {
            // Log form fields and file information instead of binary data
            var form = await context.Request.ReadFormAsync();
            var formData = new System.Text.StringBuilder();

            // Log form fields
            foreach (var field in form)
            {
                if (field.Value.Count > 0)
                {
                    formData.AppendLine($"  {field.Key}: {string.Join(", ", [])}");
                }
            }

            // Log file information
            if (form.Files.Count > 0)
            {
                formData.AppendLine("Files:");
                foreach (var file in form.Files)
                {
                    formData.AppendLine($"  - FileName: {file.FileName}");
                    formData.AppendLine($"    ContentType: {file.ContentType}");
                    formData.AppendLine($"    Size: {file.Length} bytes");
                }
            }

            bodyContent = formData.ToString();
        }
        // Check if content type is image or other binary content
        else if (contentType.Contains("image/") || contentType.Contains("application/octet-stream"))
        {
            bodyContent =
                $"[Binary content - {contentType}, Size: {context.Request.ContentLength ?? 0} bytes]";
        }
        // For JSON or text content
        else if (context.Request.ContentLength > 0)
        {
            context.Request.Body.Position = 0;
            using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
            bodyContent = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0; // Reset for next middleware
        }

        await _logger.LogRequest(method, path + query, contentType, bodyContent);
    }

    private async Task LogResponse(HttpContext context, MemoryStream responseBody)
    {
        var statusCode = context.Response.StatusCode;

        // Read the response body
        responseBody.Seek(0, SeekOrigin.Begin);
        var responseContent = await new StreamReader(responseBody).ReadToEndAsync();
        responseBody.Seek(0, SeekOrigin.Begin);

        await _logger.LogResponse(statusCode, responseContent);
    }
}
