using System.Text.Json.Serialization;
using Application.Common;
using Common;
using OnlineShop.Domain.Common;

namespace Api.Errors;

/// <summary>One field the user has to fix. Field is the name the form knows it by.</summary>
public sealed record ApiFieldError(string Field, string Label, string Message)
{
    public static ApiFieldError From(DomainFieldError error) =>
        new(ToFormName(error.Field), FieldLabels.Field(error.Field), error.Message);

    public static string ToFormName(string field)
    {
        var name = FieldLabels.LastSegment(field);
        return name.Length == 0 ? field : char.ToLowerInvariant(name[0]) + name[1..];
    }
}

/// <summary>
/// The one shape of every failed response, whichever layer it comes from. It keeps the
/// isSuccess / error pair of <see cref="ServiceResponse"/>, which all frontend code reads.
/// </summary>
public sealed class ApiErrorBody
{
    public bool IsSuccess => false;

    /// <summary>What went wrong, in words the reader can act on.</summary>
    public required string Error { get; init; }

    /// <summary>See <see cref="ErrorCodes"/>.</summary>
    public required string ErrorCode { get; init; }

    /// <summary>Reference to the server log entry of this request.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TraceId { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<ApiFieldError>? FieldErrors { get; init; }

    /// <summary>Technical cause. Only sent to admins and in Development.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Detail { get; init; }
}

public sealed record ApiError(int Status, ApiErrorBody Body);

public static class ApiErrorItems
{
    /// <summary>HttpContext.Items key: the reason to report when the pipeline ends with an empty error response.</summary>
    public const string Message = "ApiError.Message";
}

public static class ApiErrorWriter
{
    public static async Task WriteAsync(HttpContext context, ApiError error)
    {
        context.Response.Clear();
        context.Response.StatusCode = error.Status;
        await context.Response.WriteAsJsonAsync(error.Body);
    }

    public static Task WriteAsync(HttpContext context, int status, string errorCode, string message) =>
        WriteAsync(context, new ApiError(status, new ApiErrorBody
        {
            Error = message,
            ErrorCode = errorCode,
            TraceId = context.TraceIdentifier,
        }));

    /// <summary>Admins and developers may read the technical cause; visitors never do.</summary>
    public static bool CanSeeDetail(HttpContext context)
    {
        var environment = context.RequestServices.GetService<IWebHostEnvironment>();
        if (environment?.IsDevelopment() == true)
            return true;

        return context.IsCatalogStaff();
    }
}
