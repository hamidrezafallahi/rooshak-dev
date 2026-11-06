using System.Text.RegularExpressions;
using Application.Common;
using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Api.Errors;

/// <summary>
/// Model binding and data annotations report failures in English and by property name.
/// This rewrites them per field, using the labels people see in the forms.
/// </summary>
public static partial class ValidationMessages
{
    public static bool HasPersianText(string? text) =>
        !string.IsNullOrEmpty(text) && PersianLetter().IsMatch(text);

    /// <summary>InvalidModelStateResponseFactory: every automatic 400 goes through here.</summary>
    public static IActionResult ToResult(ActionContext context) =>
        new BadRequestObjectResult(FromModelState(context.ModelState, context.HttpContext));

    public static ApiErrorBody FromModelState(ModelStateDictionary state, HttpContext context)
    {
        var errors = state
            .Where(entry => entry.Value is { Errors.Count: > 0 })
            .Select(entry => (
                Key: entry.Key,
                Messages: entry.Value!.Errors
                    .Select(error => string.IsNullOrEmpty(error.ErrorMessage)
                        ? error.Exception?.Message ?? string.Empty
                        : error.ErrorMessage)
                    .ToList()))
            .ToList();

        return Build(errors, context);
    }

    public static ApiErrorBody FromDictionary(IEnumerable<KeyValuePair<string, string[]>> errors, HttpContext context) =>
        Build(errors.Select(e => (e.Key, e.Value.ToList())).ToList(), context);

    private static ApiErrorBody Build(List<(string Key, List<string> Messages)> errors, HttpContext context)
    {
        // An empty or broken body also reports the action parameter itself as missing.
        static bool IsBodyError((string Key, List<string> Messages) error) =>
            error.Key.Length == 0 || error.Key.StartsWith('$');

        if (errors.Any(IsBodyError))
            errors = errors.Where(e => IsBodyError(e) || !e.Messages.All(IsRequiredMessage)).ToList();

        var fields = errors
            .Select(e => new ApiFieldError(
                ApiFieldError.ToFormName(e.Key),
                FieldLabels.Field(e.Key),
                Translate(e.Key, e.Messages.FirstOrDefault(m => m.Length > 0) ?? string.Empty)))
            .ToList();

        var summary = fields.Count == 0
            ? "اطلاعات ارسال‌شده معتبر نیست."
            : string.Join(" ", fields.Select(f => f.Message).Distinct());

        return new ApiErrorBody
        {
            Error = summary,
            ErrorCode = ErrorCodes.ValidationFailed,
            TraceId = context.TraceIdentifier,
            FieldErrors = fields.Count == 0 ? null : fields,
        };
    }

    public static string Translate(string key, string message)
    {
        if (HasPersianText(message))
            return message;

        if (EmptyBody().IsMatch(message))
            return "اطلاعات فرم به سرور نرسید؛ بدنهٔ درخواست خالی است.";

        var label = FieldLabels.Field(key);

        if (IsRequiredMessage(message))
            return key.Length == 0 ? "اطلاعات موردنیاز ارسال نشده است." : $"«{label}» الزامی است.";

        Match match;
        if ((match = JsonConversion().Match(message)).Success)
            return $"مقدار «{label}» باید {DescribeType(match.Groups["type"].Value)} باشد.";

        if ((match = ValueInvalid().Match(message)).Success)
            return $"مقدار «{match.Groups["value"].Value}» برای «{label}» معتبر نیست.";

        if (MustBeNumber().IsMatch(message))
            return $"«{label}» باید عدد باشد.";

        if ((match = MaxLength().Match(message)).Success)
            return $"«{label}» حداکثر {match.Groups["max"].Value} نویسه می‌تواند باشد.";

        if ((match = MinLength().Match(message)).Success)
            return $"«{label}» باید حداقل {match.Groups["min"].Value} نویسه باشد.";

        if ((match = Between().Match(message)).Success)
            return $"«{label}» باید بین {match.Groups["min"].Value} و {match.Groups["max"].Value} باشد.";

        if (InvalidEmail().IsMatch(message))
            return "ایمیل واردشده معتبر نیست.";

        if (BodyTooLarge().IsMatch(message))
            return "حجم داده یا فایل ارسال‌شده بیشتر از سقف مجاز سرور است.";

        if (FormUnreadable().IsMatch(message))
            return "فرم یا فایل ارسال‌شده خراب است و سرور نتوانست آن را بخواند.";

        if (key.StartsWith('$') || BrokenJson().IsMatch(message))
            return key.Length > 1 && key != "$"
                ? $"مقدار «{label}» قالب درستی ندارد."
                : "داده‌های ارسال‌شده قالب JSON معتبری ندارند.";

        return key.Length == 0 ? "اطلاعات ارسال‌شده معتبر نیست." : $"مقدار «{label}» معتبر نیست.";
    }

    private static bool IsRequiredMessage(string message) => Required().IsMatch(message);

    private static string DescribeType(string type)
    {
        var name = type.Trim();
        if (name.StartsWith("System.Nullable`1[", StringComparison.Ordinal))
            name = name["System.Nullable`1[".Length..].TrimEnd(']');

        return name switch
        {
            "System.Int16" or "System.Int32" or "System.Int64" or "System.Byte" => "عدد صحیح",
            "System.Decimal" or "System.Double" or "System.Single" => "عدد",
            "System.Boolean" => "بله یا خیر",
            "System.DateTime" or "System.DateTimeOffset" or "System.DateOnly" => "تاریخ معتبر",
            "System.String" => "متن",
            "System.Guid" => "شناسهٔ معتبر",
            _ when name.Contains("List") || name.EndsWith("[]", StringComparison.Ordinal) => "فهرستی از مقادیر",
            _ => "یکی از گزینه‌های مجاز",
        };
    }

    [GeneratedRegex(@"[؀-ۿ]")]
    private static partial Regex PersianLetter();

    [GeneratedRegex(@"field is required\.?$|^A value (for|is required)|is required\.?$", RegexOptions.IgnoreCase)]
    private static partial Regex Required();

    [GeneratedRegex(@"The JSON value could not be converted to (?<type>[^.]+(\.[^.\s]+)*)\.\s*Path:", RegexOptions.IgnoreCase)]
    private static partial Regex JsonConversion();

    [GeneratedRegex(@"^The value '(?<value>.*)' is (not valid|invalid)", RegexOptions.IgnoreCase)]
    private static partial Regex ValueInvalid();

    [GeneratedRegex(@"must be a number\.?$", RegexOptions.IgnoreCase)]
    private static partial Regex MustBeNumber();

    [GeneratedRegex(@"maximum length of '?(?<max>\d+)'?", RegexOptions.IgnoreCase)]
    private static partial Regex MaxLength();

    [GeneratedRegex(@"minimum length of '?(?<min>\d+)'?", RegexOptions.IgnoreCase)]
    private static partial Regex MinLength();

    [GeneratedRegex(@"must be between (?<min>\S+) and (?<max>\S+?)\.?$", RegexOptions.IgnoreCase)]
    private static partial Regex Between();

    [GeneratedRegex(@"not a valid e-?mail", RegexOptions.IgnoreCase)]
    private static partial Regex InvalidEmail();

    [GeneratedRegex(@"non-empty request body", RegexOptions.IgnoreCase)]
    private static partial Regex EmptyBody();

    [GeneratedRegex(@"body too large|length limit|limit \d+ exceeded", RegexOptions.IgnoreCase)]
    private static partial Regex BodyTooLarge();

    [GeneratedRegex(@"failed to read the request form", RegexOptions.IgnoreCase)]
    private static partial Regex FormUnreadable();

    [GeneratedRegex(@"invalid start of a value|LineNumber:|BytePositionInLine|JSON", RegexOptions.IgnoreCase)]
    private static partial Regex BrokenJson();
}
