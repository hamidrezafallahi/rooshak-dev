namespace Application.DataTransfer.Excel
{
    public enum ExcelValueType
    {
        Text,
        Integer,
        Decimal,
        Boolean,
        /// <summary>Exported as a Jalali date (yyyy/MM/dd); import accepts Jalali or Gregorian.</summary>
        Date,
        /// <summary>Exported as a Jalali date-time (yyyy/MM/dd HH:mm); import accepts Jalali or Gregorian.</summary>
        DateTime,
    }

    /// <summary>
    /// One column of an Excel sheet. <see cref="Key"/> is the stable technical name written in the
    /// header (row 2) and used for matching on import; <see cref="Caption"/> is the Persian title (row 1).
    /// </summary>
    public sealed class ExcelColumn
    {
        public required string Key { get; init; }
        public required string Caption { get; init; }
        public ExcelValueType Type { get; init; } = ExcelValueType.Text;
        public bool Required { get; init; }

        /// <summary>Exported for information only; ignored on import (rendered grey).</summary>
        public bool ReadOnly { get; init; }

        public string? Description { get; init; }

        /// <summary>Key of an <see cref="ExcelLookup"/>; the template gets a dropdown of "Id - Label" values.</summary>
        public string? LookupKey { get; init; }

        /// <summary>
        /// Key of a sheet in the same workbook whose rows this column may reference by their <c>Ref</c>
        /// value (e.g. a new product row referenced from the price sheet before it has an Id).
        /// </summary>
        public string? ReferencesSheet { get; init; }

        public double? Width { get; init; }
    }

    public sealed class ExcelSheetSchema
    {
        public const string IdColumn = "Id";
        public const string RefColumn = "Ref";

        public required string Key { get; init; }
        public required string Caption { get; init; }
        public string? Description { get; init; }
        public required IReadOnlyList<ExcelColumn> Columns { get; init; }

        public ExcelColumn? FindColumn(string key) =>
            Columns.FirstOrDefault(c => string.Equals(c.Key, key, StringComparison.OrdinalIgnoreCase));
    }

    public sealed record ExcelLookupItem(int Id, string Label)
    {
        /// <summary>Value written into cells and offered in dropdowns. Import reads the leading Id.</summary>
        public string Display => $"{Id} - {Label}";
    }

    /// <summary>A list of valid values for a foreign-key column, written to its own sheet.</summary>
    public sealed class ExcelLookup
    {
        public required string Key { get; init; }
        public required string Caption { get; init; }
        public required IReadOnlyList<ExcelLookupItem> Items { get; init; }

        public string? DisplayOf(int? id) =>
            id is null ? null : Items.FirstOrDefault(i => i.Id == id)?.Display ?? id.ToString();
    }

    /// <summary>Fluent builder used by profiles to declare a sheet.</summary>
    public sealed class ExcelSheetBuilder(string key, string caption)
    {
        private readonly List<ExcelColumn> _columns = [];
        private string? _description;

        public ExcelSheetBuilder Describe(string description)
        {
            _description = description;
            return this;
        }

        /// <summary>Adds the <c>Id</c> column: filled → update that record, empty → create a new one.</summary>
        public ExcelSheetBuilder Id() => Column(new ExcelColumn
        {
            Key = ExcelSheetSchema.IdColumn,
            Caption = "شناسه",
            Type = ExcelValueType.Integer,
            Description = "برای ویرایش رکورد موجود پر شود؛ برای افزودن رکورد جدید خالی بماند.",
            Width = 10,
        });

        /// <summary>Adds the optional <c>Ref</c> column so other sheets can point at rows that are new in this file.</summary>
        public ExcelSheetBuilder Ref() => Column(new ExcelColumn
        {
            Key = ExcelSheetSchema.RefColumn,
            Caption = "کلید ارجاع",
            Description = "اختیاری. یک متن یکتا (مثلاً p1) تا در شیت‌های دیگر به این ردیفِ جدید ارجاع دهید.",
            Width = 12,
        });

        public ExcelSheetBuilder Text(string key, string caption, bool required = false, string? description = null, double? width = null)
            => Column(new ExcelColumn { Key = key, Caption = caption, Required = required, Description = description, Width = width });

        public ExcelSheetBuilder Integer(string key, string caption, bool required = false, string? description = null)
            => Column(new ExcelColumn { Key = key, Caption = caption, Type = ExcelValueType.Integer, Required = required, Description = description });

        public ExcelSheetBuilder Decimal(string key, string caption, bool required = false, string? description = null)
            => Column(new ExcelColumn { Key = key, Caption = caption, Type = ExcelValueType.Decimal, Required = required, Description = description });

        public ExcelSheetBuilder Boolean(string key, string caption, bool required = false, string? description = null)
            => Column(new ExcelColumn { Key = key, Caption = caption, Type = ExcelValueType.Boolean, Required = required, Description = description ?? "بله / خیر" });

        public ExcelSheetBuilder Date(string key, string caption, bool required = false, string? description = null)
            => Column(new ExcelColumn { Key = key, Caption = caption, Type = ExcelValueType.Date, Required = required, Description = description ?? "تاریخ شمسی مثل 1404/07/26" });

        public ExcelSheetBuilder DateTime(string key, string caption, bool required = false, string? description = null)
            => Column(new ExcelColumn { Key = key, Caption = caption, Type = ExcelValueType.DateTime, Required = required, Description = description ?? "تاریخ شمسی مثل 1404/07/26 14:30" });

        /// <summary>Foreign key to existing records, offered as a dropdown from <paramref name="lookupKey"/>.</summary>
        public ExcelSheetBuilder Lookup(string key, string caption, string lookupKey, bool required = false, string? referencesSheet = null, string? description = null)
            => Column(new ExcelColumn
            {
                Key = key,
                Caption = caption,
                Type = ExcelValueType.Integer,
                Required = required,
                LookupKey = lookupKey,
                ReferencesSheet = referencesSheet,
                Description = description ?? (referencesSheet is null
                    ? "از لیست انتخاب کنید یا شناسه را وارد کنید."
                    : "از لیست انتخاب کنید، شناسه را وارد کنید، یا «کلید ارجاع» یک ردیف جدید در همین فایل را بنویسید."),
                Width = 28,
            });

        /// <summary>Informational column: exported, never imported.</summary>
        public ExcelSheetBuilder ReadOnly(string key, string caption, ExcelValueType type = ExcelValueType.Text, string? description = null)
            => Column(new ExcelColumn { Key = key, Caption = caption, Type = type, ReadOnly = true, Description = description ?? "فقط جهت اطلاع؛ در ورود اطلاعات نادیده گرفته می‌شود." });

        public ExcelSheetBuilder Column(ExcelColumn column)
        {
            if (_columns.Any(c => string.Equals(c.Key, column.Key, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Duplicate Excel column '{column.Key}' in sheet '{key}'.");
            _columns.Add(column);
            return this;
        }

        public ExcelSheetSchema Build() => new()
        {
            Key = key,
            Caption = caption,
            Description = _description,
            Columns = _columns.ToList(),
        };
    }
}
