using Api.Errors;

/// <summary>
/// Outermost guard of the API: no request leaves with a bare status code. Unhandled exceptions
/// and empty error responses (401, 403, 404, 429 ...) both become an <see cref="ApiErrorBody"/>
/// that says what went wrong.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);

            if (HasNoExplanation(context))
                // Status and headers such as Retry-After or WWW-Authenticate stay as they are.
                await context.Response.WriteAsJsonAsync(StatusCodeErrors.ForEmptyResponse(context).Body);
        }
        catch (Exception) when (context.RequestAborted.IsCancellationRequested)
        {
            // The client went away; there is nobody left to answer.
            _logger.LogDebug("Request aborted by the client: {Method} {Path}",
                context.Request.Method, context.Request.Path);
        }
        catch (Exception ex) when (!context.Response.HasStarted)
        {
            var error = ApiExceptionTranslator.Translate(ex, context);
            Log(context, ex, error);
            await ApiErrorWriter.WriteAsync(context, error);
        }
    }

    private static bool HasNoExplanation(HttpContext context) =>
        context.Response.StatusCode >= StatusCodes.Status400BadRequest
        && !context.Response.HasStarted
        && !HttpMethods.IsHead(context.Request.Method);

    private void Log(HttpContext context, Exception ex, ApiError error)
    {
        if (error.Status >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(ex, "{Code} for {Method} {Path} (trace {TraceId}): {Message}",
                error.Body.ErrorCode, context.Request.Method, context.Request.Path,
                context.TraceIdentifier, error.Body.Error);
            return;
        }

        _logger.LogInformation("{Code} for {Method} {Path}: {Message}",
            error.Body.ErrorCode, context.Request.Method, context.Request.Path, error.Body.Error);
    }
}
