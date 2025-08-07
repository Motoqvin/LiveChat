using LiveChatApp.Exceptions;
using LiveChatApp.Models;
using LiveChatApp.Services.Base;

namespace LiveChatApp.Middlewares;
public class LoggingMiddleware
{
    private readonly RequestDelegate next;

    public LoggingMiddleware(RequestDelegate next)
    {
        this.next = next;
    }
    
    private string? SanitizeString(string input)
    {
        return input?.Replace("\0", string.Empty);
    }

    public async Task InvokeAsync(HttpContext context, IHttpLogger logger)
    {
        var requestId = Guid.NewGuid();
        var creationDateTime = DateTime.UtcNow;

        context.Request.EnableBuffering();
        var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
        requestBody = SanitizeString(requestBody);
        context.Request.Body.Position = 0;

        var requestHeaders = string.Join(", ", context.Request.Headers.Select(h => $"{h.Key}: {h.Value}"));
        requestHeaders = SanitizeString(requestHeaders);
        var responseHeaders = string.Join(", ", context.Response.Headers.Select(h => $"{h.Key}: {h.Value}"));
        responseHeaders = SanitizeString(responseHeaders);
        var methodType = 1;
        methodType = context.Request.Method switch
        {
            "GET" => 1,
            "POST" => 2,
            "DELETE" => 3,
            "PUT" => 4,
            _ => throw new BadRequestException(message: $"Method {context.Request.Method} is not supported.", param: nameof(context.Request.Method)),
        };
        var originalBodyStream = context.Response.Body;
        using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        await this.next(context);

        responseBodyStream.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(responseBodyStream).ReadToEndAsync();
        responseBody = SanitizeString(responseBody);
        responseBodyStream.Seek(0, SeekOrigin.Begin);

        await responseBodyStream.CopyToAsync(originalBodyStream);

        var endDateTime = DateTime.UtcNow;

        var log = new HttpLog
        {
            Id = requestId,
            Url = context.Request.Path,
            RequestBody = requestBody,
            RequestHeaders = requestHeaders,
            MethodType = methodType,
            ResponseBody = responseBody,
            ResponseHeaders = responseHeaders,
            StatusCode = (int)context.Response.StatusCode,
            CreationDateTime = creationDateTime,
            EndDateTime = endDateTime,
            ClientIp = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
        };

        await logger.LogAsync(log);
    }
}