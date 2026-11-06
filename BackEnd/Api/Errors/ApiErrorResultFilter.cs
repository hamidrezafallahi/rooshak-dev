using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Api.Errors;

/// <summary>
/// Last stop of every controller result. Whatever an action returned for a failure
/// (BadRequest(ModelState), a plain string, a ServiceResult) leaves in the one shape
/// the frontend reads, with a reason and a trace id.
/// </summary>
public sealed class ApiErrorResultFilter : IAsyncAlwaysRunResultFilter
{
    public Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult result)
            Normalize(result, context.HttpContext);

        return next();
    }

    private static void Normalize(ObjectResult result, HttpContext context)
    {
        var status = result.StatusCode ?? StatusCodes.Status200OK;

        switch (result.Value)
        {
            case ServiceResponse { IsSuccess: false } failure:
                Complete(failure, status, context);
                break;

            case ValidationProblemDetails validation when status >= 400:
                result.Value = ValidationMessages.FromDictionary(validation.Errors, context);
                break;

            case SerializableError errors when status >= 400:
                result.Value = ValidationMessages.FromDictionary(
                    errors.Select(e => new KeyValuePair<string, string[]>(
                        e.Key,
                        e.Value as string[] ?? [e.Value?.ToString() ?? string.Empty])),
                    context);
                break;

            case ProblemDetails when status >= 400:
            case null when status >= 400:
                result.Value = Body(status, context, null);
                break;

            case string text when status >= 400:
                result.Value = Body(status, context, text);
                break;
        }
    }

    private static void Complete(ServiceResponse failure, int status, HttpContext context)
    {
        var (code, message) = StatusCodeErrors.Describe(status, context);
        if (status >= 400)
            failure.ErrorCode ??= code;
        if (string.IsNullOrWhiteSpace(failure.Error))
            failure.Error = message;
        failure.TraceId ??= context.TraceIdentifier;
    }

    private static ApiErrorBody Body(int status, HttpContext context, string? text)
    {
        var (code, message) = StatusCodeErrors.Describe(status, context);
        return new ApiErrorBody
        {
            Error = ValidationMessages.HasPersianText(text) ? text! : message,
            ErrorCode = code,
            TraceId = context.TraceIdentifier,
        };
    }
}
