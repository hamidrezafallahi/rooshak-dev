using System.Text.Json;
using Application.Common;
using Application.DataTransfer.Excel;
using Common;
using Microsoft.EntityFrameworkCore.Metadata;
using OnlineShop.Domain.Common;
using OnlineShop.Infrastructure.Persistence;

namespace Api.Errors;

/// <summary>
/// Decides what the client is told about an exception. Messages of <see cref="DomainException"/>
/// are written for people; every other exception is named here by what it means, so a framework
/// or driver message never reaches a visitor (admins get it as Detail).
/// </summary>
public static class ApiExceptionTranslator
{
    public static ApiError Translate(Exception exception, HttpContext context)
    {
        var domain = DatabaseErrorTranslator.Translate(exception, ModelOf(context));
        if (domain is not null)
            return FromDomain(domain, context);

        var (status, code, message) = exception switch
        {
            ExcelFormatException or ExcelRowException =>
                (StatusCodes.Status400BadRequest, ErrorCodes.InvalidData, exception.Message),

            BadHttpRequestException bad => FromBadRequest(bad),

            InvalidDataException => FromInvalidBody(exception),

            JsonException =>
                (StatusCodes.Status400BadRequest, ErrorCodes.InvalidData,
                    "داده‌های ارسال‌شده یا ذخیره‌شده قالب JSON معتبری ندارند."),

            FormatException or OverflowException =>
                (StatusCodes.Status400BadRequest, ErrorCodes.InvalidData,
                    "قالب یکی از مقادیر ارسال‌شده درست نیست یا عدد آن بیش از حد بزرگ است."),

            ArgumentException argument when ValidationMessages.HasPersianText(argument.Message) =>
                (StatusCodes.Status400BadRequest, ErrorCodes.InvalidData, WithoutParameterName(argument)),

            HttpRequestException =>
                (StatusCodes.Status502BadGateway, ErrorCodes.ExternalServiceError,
                    "ارتباط سرور با یک سرویس بیرونی برقرار نشد؛ چند لحظه بعد دوباره تلاش کنید."),

            TimeoutException or OperationCanceledException =>
                (StatusCodes.Status504GatewayTimeout, ErrorCodes.RequestTimeout,
                    $"{OperationOf(context)} بیش از حد طول کشید و متوقف شد؛ چند لحظه بعد دوباره تلاش کنید."),

            UnauthorizedAccessException =>
                (StatusCodes.Status500InternalServerError, ErrorCodes.StorageError,
                    "سرور اجازهٔ دسترسی به فایل یا پوشهٔ موردنیاز این عملیات را ندارد."),

            IOException io when IsDiskFull(io) =>
                (StatusCodes.Status507InsufficientStorage, ErrorCodes.StorageError,
                    "فضای دیسک سرور پر شده است و فایل ذخیره نمی‌شود."),

            IOException =>
                (StatusCodes.Status500InternalServerError, ErrorCodes.StorageError,
                    "خواندن یا نوشتن فایل روی سرور انجام نشد."),

            OutOfMemoryException =>
                (StatusCodes.Status503ServiceUnavailable, ErrorCodes.ServiceUnavailable,
                    "حافظهٔ سرور برای انجام این درخواست کافی نیست."),

            _ => (StatusCodes.Status500InternalServerError, ErrorCodes.InternalError,
                $"{OperationOf(context)} به‌دلیل یک خطای پیش‌بینی‌نشده در سرور انجام نشد؛ جزئیات آن در گزارش سرور ثبت شد."),
        };

        return new ApiError(status, new ApiErrorBody
        {
            Error = message,
            ErrorCode = code,
            TraceId = context.TraceIdentifier,
            Detail = DetailFor(context, Technical(exception)),
        });
    }

    private static ApiError FromDomain(DomainException domain, HttpContext context)
    {
        var status = domain switch
        {
            NotFoundException => StatusCodes.Status404NotFound,
            ConflictException => StatusCodes.Status409Conflict,
            DependencyFailureException => StatusCodes.Status503ServiceUnavailable,
            _ => StatusCodes.Status400BadRequest,
        };

        return new ApiError(status, new ApiErrorBody
        {
            Error = domain.Message,
            ErrorCode = domain.Code,
            TraceId = context.TraceIdentifier,
            FieldErrors = domain.FieldErrors?.Select(ApiFieldError.From).ToList(),
            Detail = DetailFor(context, domain.TechnicalDetail),
        });
    }

    private static (int Status, string Code, string Message) FromBadRequest(BadHttpRequestException bad) =>
        bad.StatusCode switch
        {
            StatusCodes.Status413PayloadTooLarge =>
                (bad.StatusCode, ErrorCodes.PayloadTooLarge,
                    "حجم داده یا فایل ارسال‌شده بیشتر از سقف مجاز سرور است."),
            StatusCodes.Status408RequestTimeout =>
                (bad.StatusCode, ErrorCodes.RequestTimeout,
                    "ارسال داده به سرور بیش از حد طول کشید؛ اتصال اینترنت را بررسی کنید و دوباره تلاش کنید."),
            _ => (StatusCodes.Status400BadRequest, ErrorCodes.BadRequest,
                "درخواست ناقص یا خراب به سرور رسید؛ دوباره تلاش کنید."),
        };

    private static (int Status, string Code, string Message) FromInvalidBody(Exception exception) =>
        exception.Message.Contains("limit", StringComparison.OrdinalIgnoreCase)
            ? (StatusCodes.Status413PayloadTooLarge, ErrorCodes.PayloadTooLarge,
                "حجم داده یا فایل ارسال‌شده بیشتر از سقف مجاز سرور است.")
            : (StatusCodes.Status400BadRequest, ErrorCodes.BadRequest,
                "فرم یا فایل ارسال‌شده خراب است و سرور نتوانست آن را بخواند.");

    // ArgumentException appends " (Parameter 'name')" to the text it was given.
    private static string WithoutParameterName(ArgumentException argument) =>
        argument.ParamName is null
            ? argument.Message
            : argument.Message.Replace($" (Parameter '{argument.ParamName}')", string.Empty);

    private static bool IsDiskFull(IOException exception)
    {
        const int WindowsDiskFull = 112;
        const int WindowsHandleDiskFull = 39;
        const int LinuxNoSpace = 28;
        var code = exception.HResult & 0xFFFF;
        return code is WindowsDiskFull or WindowsHandleDiskFull
            || exception.HResult == LinuxNoSpace
            || exception.Message.Contains("No space left", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>"ثبت «برند»": what the failed request was doing, in the reader's words.</summary>
    public static string OperationOf(HttpContext context)
    {
        var target = TargetLabel(context);
        if (target is null)
            return "این درخواست";

        var verb = context.Request.Method switch
        {
            "GET" or "HEAD" => "دریافت",
            "POST" => "ثبت",
            "PUT" or "PATCH" => "ویرایش",
            "DELETE" => "حذف",
            _ => "پردازش",
        };
        return $"{verb} «{target}»";
    }

    private static string? TargetLabel(HttpContext context)
    {
        var controller = context.Request.RouteValues["controller"]?.ToString();
        return string.IsNullOrWhiteSpace(controller) ? null : FieldLabels.Entity(controller);
    }

    private static string? DetailFor(HttpContext context, string? detail) =>
        detail is not null && ApiErrorWriter.CanSeeDetail(context) ? detail : null;

    private static string Technical(Exception exception)
    {
        var root = exception.GetBaseException();
        return $"{root.GetType().Name}: {root.Message}";
    }

    private static IModel? ModelOf(HttpContext context)
    {
        try
        {
            return context.RequestServices.GetService<AppDbContext>()?.Model;
        }
        catch (Exception)
        {
            // Only used to name tables and columns; the failure itself is still reported.
            return null;
        }
    }
}
