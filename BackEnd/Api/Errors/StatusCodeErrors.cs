using Common;

namespace Api.Errors;

/// <summary>
/// Reasons for responses the pipeline ends with no body: the authentication challenge,
/// authorization, unknown routes, the rate limiter and the like.
/// </summary>
public static class StatusCodeErrors
{
    public static ApiError ForEmptyResponse(HttpContext context)
    {
        var status = context.Response.StatusCode;
        var (code, message) = Describe(status, context);

        if (context.Items.TryGetValue(ApiErrorItems.Message, out var reason) && reason is string text)
            message = text;

        return new ApiError(status, new ApiErrorBody
        {
            Error = message,
            ErrorCode = code,
            TraceId = context.TraceIdentifier,
        });
    }

    public static (string Code, string Message) Describe(int status, HttpContext context)
    {
        var path = context.Request.Path.Value ?? "/";

        return status switch
        {
            StatusCodes.Status401Unauthorized => (ErrorCodes.Unauthorized, ErrorMessages.LoginRequired),
            StatusCodes.Status403Forbidden => (ErrorCodes.Forbidden, ErrorMessages.Forbidden),
            StatusCodes.Status404NotFound => (ErrorCodes.NotFound,
                $"سرویسی با آدرس «{path}» روی سرور وجود ندارد."),
            StatusCodes.Status405MethodNotAllowed => (ErrorCodes.MethodNotAllowed,
                $"آدرس «{path}» این نوع درخواست ({context.Request.Method}) را نمی‌پذیرد."),
            StatusCodes.Status406NotAcceptable or StatusCodes.Status415UnsupportedMediaType =>
                (ErrorCodes.UnsupportedMediaType, "سرور قالب داده‌های ارسال‌شده را نمی‌پذیرد."),
            StatusCodes.Status408RequestTimeout => (ErrorCodes.RequestTimeout,
                "ارسال درخواست به سرور بیش از حد طول کشید؛ اتصال اینترنت را بررسی کنید و دوباره تلاش کنید."),
            StatusCodes.Status409Conflict => (ErrorCodes.Conflict,
                "این درخواست با وضعیت فعلی داده‌ها سازگار نیست؛ صفحه را تازه کنید و دوباره تلاش کنید."),
            StatusCodes.Status413PayloadTooLarge => (ErrorCodes.PayloadTooLarge,
                "حجم داده یا فایل ارسال‌شده بیشتر از سقف مجاز سرور است."),
            StatusCodes.Status429TooManyRequests => (ErrorCodes.RateLimited,
                "تعداد درخواست‌های شما در یک دقیقه از حد مجاز گذشته است؛ کمی صبر کنید و دوباره تلاش کنید."),
            StatusCodes.Status502BadGateway or StatusCodes.Status503ServiceUnavailable
                or StatusCodes.Status504GatewayTimeout => (ErrorCodes.ServiceUnavailable,
                    "سرور موقتاً پاسخگو نیست؛ چند لحظه بعد دوباره تلاش کنید."),
            >= 500 => (ErrorCodes.InternalError,
                $"{ApiExceptionTranslator.OperationOf(context)} به‌دلیل یک خطای پیش‌بینی‌نشده در سرور انجام نشد."),
            _ => (ErrorCodes.BadRequest, "این درخواست معتبر نیست و سرور آن را نپذیرفت."),
        };
    }
}
