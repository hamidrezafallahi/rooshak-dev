using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Application.DataTransfer.Excel
{
    /// <summary>Thrown while processing a row; the message is shown to the admin as-is.</summary>
    public sealed class ExcelRowException(string message) : Exception(message);

    /// <summary>Thrown when an uploaded file cannot be read at all (wrong format, missing sheet...).</summary>
    public sealed class ExcelFormatException(string message) : Exception(message);

    /// <summary>
    /// One data row keyed by column <see cref="ExcelColumn.Key"/>. Values are kept raw
    /// (string / double / bool / DateTime) and converted on access so Persian digits,
    /// Jalali dates and "بله/خیر" all work no matter how the admin typed them.
    /// </summary>
    public sealed class ExcelRow
    {
        private readonly Dictionary<string, object?> _values;

        public ExcelRow(int rowNumber, IDictionary<string, object?>? values = null)
        {
            RowNumber = rowNumber;
            _values = new Dictionary<string, object?>(values ?? new Dictionary<string, object?>(), StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>1-based row number in the sheet (0 for rows built for export).</summary>
        public int RowNumber { get; }

        public IReadOnlyDictionary<string, object?> Values => _values;

        public object? this[string key]
        {
            get => _values.TryGetValue(key, out var v) ? v : null;
            set => _values[key] = value;
        }

        public bool Has(string key) => !ExcelValue.IsBlank(this[key]);

        public bool IsEmpty => _values.Values.All(ExcelValue.IsBlank);

        public int? Id => GetInt(ExcelSheetSchema.IdColumn);

        public string? Ref => GetString(ExcelSheetSchema.RefColumn);

        public string? GetString(string key) => ExcelValue.ToText(this[key]);

        public string RequireString(string key) => GetString(key) ?? throw Missing(key);

        public int? GetInt(string key) => ExcelValue.ToInt(this[key], key);

        public int RequireInt(string key) => GetInt(key) ?? throw Missing(key);

        public decimal? GetDecimal(string key) => ExcelValue.ToDecimal(this[key], key);

        public decimal RequireDecimal(string key) => GetDecimal(key) ?? throw Missing(key);

        public bool? GetBool(string key) => ExcelValue.ToBool(this[key], key);

        public bool RequireBool(string key) => GetBool(key) ?? throw Missing(key);

        public DateTime? GetDate(string key) => ExcelValue.ToDate(this[key], key);

        public DateTime RequireDate(string key) => GetDate(key) ?? throw Missing(key);

        private static ExcelRowException Missing(string key) => new($"ستون «{key}» اجباری است.");
    }

    /// <summary>Conversions between raw cell values and CLR values (Persian-aware).</summary>
    public static partial class ExcelValue
    {
        private static readonly PersianCalendar Jalali = new();

        /// <summary>Excel cells hold at most 32,767 characters.</summary>
        public const int MaxCellLength = 32_767;

        /// <summary>
        /// Written instead of text that does not fit in a cell. Read back as blank ("no change"),
        /// so re-importing an export never truncates long content.
        /// </summary>
        public const string TooLongMarker = "[[متن طولانی‌تر از ظرفیت اکسل است؛ این سلول را تغییر ندهید]]";

        public static bool IsBlank(object? value) =>
            value is null || (value is string s && (string.IsNullOrWhiteSpace(s) || s == TooLongMarker));

        public static string? ToText(object? value) => value switch
        {
            null => null,
            string s => string.IsNullOrWhiteSpace(s) || s == TooLongMarker ? null : s.Trim(),
            double d => d.ToString(CultureInfo.InvariantCulture),
            bool b => b ? "true" : "false",
            DateTime dt => dt.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
            _ => value.ToString(),
        };

        public static int? ToInt(object? value, string key)
        {
            if (IsBlank(value)) return null;
            if (value is double d)
            {
                if (d % 1 != 0) throw Invalid(key, value, "عدد صحیح");
                return checked((int)d);
            }

            var text = NormalizeDigits(ToText(value)!);
            // Dropdown values look like "12 - نام"; the leading number is the Id.
            var match = LeadingIntRegex().Match(text);
            if (match.Success && int.TryParse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
                return id;

            throw Invalid(key, value, "عدد صحیح");
        }

        public static decimal? ToDecimal(object? value, string key)
        {
            if (IsBlank(value)) return null;
            if (value is double d) return (decimal)d;

            var text = NormalizeDigits(ToText(value)!).Replace(",", "").Replace(" ", "");
            if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var result))
                return result;

            throw Invalid(key, value, "عدد");
        }

        public static bool? ToBool(object? value, string key)
        {
            if (IsBlank(value)) return null;
            if (value is bool b) return b;
            if (value is double d) return d != 0;

            return NormalizeDigits(ToText(value)!).ToLowerInvariant() switch
            {
                "true" or "1" or "yes" or "y" or "بله" or "بلی" or "آری" or "فعال" or "دارد" => true,
                "false" or "0" or "no" or "n" or "خیر" or "نه" or "غیرفعال" or "غیر فعال" or "ندارد" => false,
                _ => throw Invalid(key, value, "بله / خیر"),
            };
        }

        public static DateTime? ToDate(object? value, string key)
        {
            if (IsBlank(value)) return null;
            if (value is DateTime dt) return dt;
            if (value is double oa) return DateTime.FromOADate(oa);

            var text = NormalizeDigits(ToText(value)!);
            var match = DateRegex().Match(text);
            if (!match.Success) throw Invalid(key, value, "تاریخ (مثل 1404/07/26)");

            var y = int.Parse(match.Groups["y"].Value, CultureInfo.InvariantCulture);
            var m = int.Parse(match.Groups["m"].Value, CultureInfo.InvariantCulture);
            var day = int.Parse(match.Groups["d"].Value, CultureInfo.InvariantCulture);
            var hour = match.Groups["h"].Success ? int.Parse(match.Groups["h"].Value, CultureInfo.InvariantCulture) : 0;
            var minute = match.Groups["min"].Success ? int.Parse(match.Groups["min"].Value, CultureInfo.InvariantCulture) : 0;

            try
            {
                // Years below 1700 are treated as Jalali (Persian) dates.
                return y < 1700
                    ? Jalali.ToDateTime(y, m, day, hour, minute, 0, 0)
                    : new DateTime(y, m, day, hour, minute, 0);
            }
            catch (ArgumentOutOfRangeException)
            {
                throw Invalid(key, value, "تاریخ معتبر");
            }
        }

        public static string ToJalali(DateTime value, bool includeTime) =>
            includeTime
                ? $"{Jalali.GetYear(value):0000}/{Jalali.GetMonth(value):00}/{Jalali.GetDayOfMonth(value):00} {value.Hour:00}:{value.Minute:00}"
                : $"{Jalali.GetYear(value):0000}/{Jalali.GetMonth(value):00}/{Jalali.GetDayOfMonth(value):00}";

        /// <summary>Converts a CLR value to what the workbook writer should put in the cell.</summary>
        public static object? ToCell(object? value, ExcelValueType type) => value switch
        {
            null => null,
            DateTime dt => ToJalali(dt, type == ExcelValueType.DateTime),
            bool b => b ? "بله" : "خیر",
            Enum e => e.ToString(),
            int or long or short or decimal or double or float => Convert.ToDouble(value, CultureInfo.InvariantCulture),
            _ => value.ToString() is { Length: > MaxCellLength } ? TooLongMarker : value.ToString(),
        };

        public static string NormalizeDigits(string text)
        {
            var sb = new StringBuilder(text.Length);
            foreach (var ch in text.Trim())
            {
                sb.Append(ch switch
                {
                    >= '۰' and <= '۹' => (char)('0' + (ch - '۰')),
                    >= '٠' and <= '٩' => (char)('0' + (ch - '٠')),
                    '٫' => '.',
                    '٬' => ',',
                    _ => ch,
                });
            }
            return sb.ToString();
        }

        private static ExcelRowException Invalid(string key, object? value, string expected) =>
            new($"مقدار «{ToText(value)}» در ستون «{key}» معتبر نیست؛ {expected} وارد کنید.");

        [GeneratedRegex(@"^\s*(-?\d+)(\s*-.*)?$", RegexOptions.Singleline)]
        private static partial Regex LeadingIntRegex();

        [GeneratedRegex(@"^(?<y>\d{4})[/\-.](?<m>\d{1,2})[/\-.](?<d>\d{1,2})(?:[ T]+(?<h>\d{1,2}):(?<min>\d{1,2})(?::\d{1,2})?)?$")]
        private static partial Regex DateRegex();
    }
}
