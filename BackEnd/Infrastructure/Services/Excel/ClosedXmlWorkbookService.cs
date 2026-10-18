using Application.Common.Interfaces;
using Application.DataTransfer.Excel;
using ClosedXML.Excel;

namespace OnlineShop.Infrastructure.Services.Excel
{
    /// <summary>
    /// .xlsx reader/writer. Layout of every data sheet:
    /// row 1 = Persian captions (required ones end with *), row 2 = technical keys, data from row 3.
    /// Import matches columns by key (row 2), falling back to captions, so column order is free.
    /// </summary>
    public sealed class ClosedXmlWorkbookService : IExcelWorkbookService
    {
        private const int CaptionRow = 1;
        private const int KeyRow = 2;
        private const int FirstDataRow = 3;
        private const int MinValidationRows = 1000;
        private const string GuideSheetName = "راهنما";
        private const string LookupSheetPrefix = "لیست ";

        private static readonly XLColor HeaderFill = XLColor.FromHtml("#DCE6F1");
        private static readonly XLColor RequiredFill = XLColor.FromHtml("#FCE4D6");
        private static readonly XLColor ReadOnlyFill = XLColor.FromHtml("#EDEDED");
        private static readonly XLColor KeyFont = XLColor.FromHtml("#808080");

        public byte[] Write(ExcelWorkbookModel model)
        {
            using var workbook = new XLWorkbook();
            var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (model.IncludeGuide)
                WriteGuide(workbook.Worksheets.Add(UniqueName(GuideSheetName, usedNames)), model);

            var dataSheets = model.Sheets
                .Select(s => (Data: s, Sheet: workbook.Worksheets.Add(UniqueName(s.Schema.Caption, usedNames))))
                .ToList();

            var lookupRanges = new Dictionary<string, IXLRange>(StringComparer.OrdinalIgnoreCase);
            foreach (var lookup in model.Lookups)
            {
                var sheet = workbook.Worksheets.Add(UniqueName(LookupSheetPrefix + lookup.Caption, usedNames));
                lookupRanges[lookup.Key] = WriteLookup(sheet, lookup);
            }

            foreach (var (data, sheet) in dataSheets)
                WriteDataSheet(sheet, data, model.Lookups, lookupRanges, model.IncludeGuide);

            // Open on the main data sheet rather than the guide.
            if (dataSheets.Count > 0)
                dataSheets[0].Sheet.SetTabActive();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public IReadOnlyList<ExcelSheetData> Read(Stream stream, IReadOnlyList<ExcelSheetSchema> schemas)
        {
            XLWorkbook workbook;
            try
            {
                workbook = new XLWorkbook(stream);
            }
            catch (Exception)
            {
                throw new ExcelFormatException("فایل اکسل قابل خواندن نیست. لطفاً فایل را با فرمت xlsx ذخیره کنید.");
            }

            using (workbook)
            {
                var result = new List<ExcelSheetData>();
                var candidates = workbook.Worksheets
                    .Where(w => w.Name != GuideSheetName && !w.Name.StartsWith(LookupSheetPrefix, StringComparison.Ordinal))
                    .ToList();
                var foundAny = false;

                foreach (var schema in schemas)
                {
                    var sheet = candidates.FirstOrDefault(w => MatchesSheet(w, schema))
                                // A single-sheet entity tolerates a renamed sheet.
                                ?? (schemas.Count == 1 && candidates.Count == 1 ? candidates[0] : null);
                    foundAny |= sheet is not null;

                    result.Add(new ExcelSheetData
                    {
                        Schema = schema,
                        Rows = sheet is null ? [] : ReadRows(sheet, schema),
                    });
                }

                if (!foundAny)
                    throw new ExcelFormatException($"شیت «{schemas[0].Caption}» در فایل پیدا نشد. از «دریافت قالب» استفاده کنید.");

                return result;
            }
        }

        // ─── Writing ──────────────────────────────────────────────────────

        private static void WriteDataSheet(
            IXLWorksheet sheet,
            ExcelSheetData data,
            IReadOnlyList<ExcelLookup> lookups,
            IReadOnlyDictionary<string, IXLRange> lookupRanges,
            bool editable)
        {
            sheet.RightToLeft = true;
            var columns = data.Schema.Columns;

            for (var i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                var index = i + 1;

                var caption = sheet.Cell(CaptionRow, index);
                caption.Value = column.Required ? $"{column.Caption} *" : column.Caption;
                caption.Style.Font.Bold = true;
                caption.Style.Fill.BackgroundColor = column.ReadOnly ? ReadOnlyFill : column.Required ? RequiredFill : HeaderFill;
                caption.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                if (editable && !string.IsNullOrWhiteSpace(column.Description))
                    caption.CreateComment().AddText(column.Description);

                var key = sheet.Cell(KeyRow, index);
                key.Value = column.Key;
                key.Style.Font.FontSize = 8;
                key.Style.Font.FontColor = KeyFont;
                key.Style.Fill.BackgroundColor = column.ReadOnly ? ReadOnlyFill : HeaderFill;
                key.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                if (column.Type == ExcelValueType.Text && column.LookupKey is null)
                    sheet.Column(index).Style.NumberFormat.Format = "@";
            }

            var lookupByKey = lookups.ToDictionary(l => l.Key, StringComparer.OrdinalIgnoreCase);
            var rowIndex = FirstDataRow;
            foreach (var row in data.Rows)
            {
                for (var i = 0; i < columns.Count; i++)
                {
                    var column = columns[i];
                    var raw = row[column.Key];
                    object? value = column.LookupKey is not null
                                    && raw is int id
                                    && lookupByKey.TryGetValue(column.LookupKey, out var lookup)
                        ? lookup.DisplayOf(id)
                        : ExcelValue.ToCell(raw, column.Type);

                    var cell = sheet.Cell(rowIndex, i + 1);
                    SetCell(cell, value);
                    if (column.ReadOnly)
                        cell.Style.Fill.BackgroundColor = ReadOnlyFill;
                }
                rowIndex++;
            }

            var lastValidationRow = Math.Max(rowIndex + 500, MinValidationRows);
            for (var i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                if (column.ReadOnly) continue;
                var range = sheet.Range(FirstDataRow, i + 1, lastValidationRow, i + 1);

                if (column.LookupKey is not null && lookupRanges.TryGetValue(column.LookupKey, out var source))
                    AddListValidation(range, v => v.List(source, true));
                else if (column.Type == ExcelValueType.Boolean)
                    AddListValidation(range, v => v.List("\"بله,خیر\"", true));
            }

            sheet.SheetView.FreezeRows(KeyRow);
            if (columns.Count > 0)
                sheet.Range(KeyRow, 1, Math.Max(rowIndex - 1, KeyRow), columns.Count).SetAutoFilter();

            for (var i = 0; i < columns.Count; i++)
            {
                var xlColumn = sheet.Column(i + 1);
                if (columns[i].Width is { } width)
                {
                    xlColumn.Width = width;
                }
                else
                {
                    xlColumn.AdjustToContents(CaptionRow, Math.Min(rowIndex, FirstDataRow + 200));
                    xlColumn.Width = Math.Clamp(xlColumn.Width + 2, 10, 60);
                }
            }
        }

        private static void AddListValidation(IXLRange range, Action<IXLDataValidation> configure)
        {
            var validation = range.CreateDataValidation();
            configure(validation);
            validation.IgnoreBlanks = true;
            // Information style: the dropdown is a help, other values (Ids, Ref keys) are still allowed.
            validation.ErrorStyle = XLErrorStyle.Information;
            validation.ShowErrorMessage = false;
        }

        private static IXLRange WriteLookup(IXLWorksheet sheet, ExcelLookup lookup)
        {
            sheet.RightToLeft = true;
            sheet.Cell(1, 1).Value = "مقدار قابل انتخاب";
            sheet.Cell(1, 2).Value = "شناسه";
            sheet.Cell(1, 3).Value = "عنوان";
            sheet.Range(1, 1, 1, 3).Style.Font.Bold = true;
            sheet.Range(1, 1, 1, 3).Style.Fill.BackgroundColor = HeaderFill;

            var row = 2;
            foreach (var item in lookup.Items)
            {
                sheet.Cell(row, 1).Value = item.Display;
                sheet.Cell(row, 2).Value = item.Id;
                sheet.Cell(row, 3).Value = item.Label;
                row++;
            }

            sheet.Columns(1, 3).AdjustToContents();
            sheet.SheetView.FreezeRows(1);
            return sheet.Range(2, 1, Math.Max(row - 1, 2), 1);
        }

        private static void WriteGuide(IXLWorksheet sheet, ExcelWorkbookModel model)
        {
            sheet.RightToLeft = true;
            var r = 1;
            sheet.Cell(r, 1).Value = model.Title;
            sheet.Cell(r, 1).Style.Font.Bold = true;
            sheet.Cell(r, 1).Style.Font.FontSize = 14;
            r += 2;

            string[] rules =
            [
                "ردیف اول عنوان ستون و ردیف دوم کلید فنی ستون است؛ این دو ردیف را تغییر ندهید. داده‌ها از ردیف سوم شروع می‌شوند.",
                "اگر ستون «شناسه» پر باشد همان رکورد ویرایش می‌شود و اگر خالی باشد رکورد جدید ساخته می‌شود.",
                "در ویرایش، سلول خالی یعنی «بدون تغییر». ستون‌هایی را که نمی‌خواهید تغییر دهید می‌توانید حذف کنید (ستون شناسه باید بماند).",
                "ستون‌های ستاره‌دار (*) برای رکورد جدید اجباری هستند. ستون‌های خاکستری فقط جهت اطلاع‌اند و هنگام ورود نادیده گرفته می‌شوند.",
                "ستون‌های دارای لیست کشویی را از لیست انتخاب کنید یا فقط شناسه را بنویسید. لیست‌ها در شیت‌های «لیست ...» آمده‌اند.",
                "برای ثبت همزمان رکورد جدید و زیرمجموعه‌هایش، در شیت اصلی «کلید ارجاع» (مثلاً p1) بنویسید و همان مقدار را در ستون والدِ شیت زیرمجموعه وارد کنید.",
                "تاریخ‌ها را شمسی (مثل 1404/07/26) یا میلادی بنویسید. اعداد فارسی هم پذیرفته می‌شوند.",
                "به‌طور پیش‌فرض اگر حتی یک ردیف خطا داشته باشد هیچ تغییری ذخیره نمی‌شود و فهرست خطاها با شماره ردیف نمایش داده می‌شود.",
            ];
            foreach (var rule in rules)
            {
                sheet.Cell(r, 1).Value = "• " + rule;
                r++;
            }
            r++;

            foreach (var data in model.Sheets)
            {
                sheet.Cell(r, 1).Value = $"شیت «{data.Schema.Caption}»";
                sheet.Cell(r, 1).Style.Font.Bold = true;
                sheet.Cell(r, 1).Style.Font.FontSize = 12;
                r++;
                if (!string.IsNullOrWhiteSpace(data.Schema.Description))
                {
                    sheet.Cell(r, 1).Value = data.Schema.Description;
                    r++;
                }

                string[] headers = ["ستون", "کلید", "نوع", "اجباری", "توضیح"];
                for (var i = 0; i < headers.Length; i++)
                {
                    var cell = sheet.Cell(r, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = HeaderFill;
                }
                r++;

                foreach (var column in data.Schema.Columns)
                {
                    sheet.Cell(r, 1).Value = column.Caption;
                    sheet.Cell(r, 2).Value = column.Key;
                    sheet.Cell(r, 3).Value = column.ReadOnly ? "فقط خواندنی" : TypeCaption(column);
                    sheet.Cell(r, 4).Value = column.Required ? "بله" : "";
                    sheet.Cell(r, 5).Value = column.Description ?? "";
                    r++;
                }
                r++;
            }

            sheet.Column(1).Width = 30;
            sheet.Column(2).Width = 22;
            sheet.Column(3).Width = 14;
            sheet.Column(4).Width = 8;
            sheet.Column(5).Width = 90;
        }

        private static string TypeCaption(ExcelColumn column) => column.LookupKey is not null
            ? "انتخاب از لیست"
            : column.Type switch
            {
                ExcelValueType.Integer => "عدد صحیح",
                ExcelValueType.Decimal => "عدد",
                ExcelValueType.Boolean => "بله/خیر",
                ExcelValueType.Date => "تاریخ",
                ExcelValueType.DateTime => "تاریخ و ساعت",
                _ => "متن",
            };

        private static void SetCell(IXLCell cell, object? value)
        {
            switch (value)
            {
                case null: cell.Value = Blank.Value; break;
                case double d: cell.Value = d; break;
                case bool b: cell.Value = b; break;
                default: cell.Value = value.ToString(); break;
            }
        }

        // ─── Reading ──────────────────────────────────────────────────────

        private static List<ExcelRow> ReadRows(IXLWorksheet sheet, ExcelSheetSchema schema)
        {
            var lastColumn = sheet.LastColumnUsed()?.ColumnNumber() ?? 0;
            var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 0;
            if (lastColumn == 0 || lastRow == 0) return [];

            // Prefer the key row; fall back to captions in row 1 for hand-made files.
            var (headerRow, map) = new[] { KeyRow, CaptionRow }
                .Select(r => (Row: r, Map: MapColumns(sheet, r, lastColumn, schema)))
                .OrderByDescending(x => x.Map.Count)
                .First();

            if (map.Count == 0)
                throw new ExcelFormatException($"سرستون‌های شیت «{sheet.Name}» شناخته نشد. از «دریافت قالب» استفاده کنید.");

            var rows = new List<ExcelRow>();
            for (var r = headerRow + 1; r <= lastRow; r++)
            {
                var values = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                foreach (var (columnIndex, column) in map)
                    values[column.Key] = ReadCell(sheet.Cell(r, columnIndex));

                var row = new ExcelRow(r, values);
                if (!row.IsEmpty)
                    rows.Add(row);
            }
            return rows;
        }

        private static Dictionary<int, ExcelColumn> MapColumns(IXLWorksheet sheet, int row, int lastColumn, ExcelSheetSchema schema)
        {
            var map = new Dictionary<int, ExcelColumn>();
            for (var c = 1; c <= lastColumn; c++)
            {
                var header = sheet.Cell(row, c).GetString().Trim().TrimEnd('*').Trim();
                if (header.Length == 0) continue;

                var column = schema.Columns.FirstOrDefault(x =>
                    string.Equals(x.Key, header, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(x.Caption, header, StringComparison.OrdinalIgnoreCase));

                if (column is not null && !column.ReadOnly && !map.ContainsValue(column))
                    map[c] = column;
            }
            return map;
        }

        private static object? ReadCell(IXLCell cell)
        {
            var value = cell.Value;
            return value.Type switch
            {
                XLDataType.Blank => null,
                XLDataType.Boolean => value.GetBoolean(),
                XLDataType.Number => value.GetNumber(),
                XLDataType.DateTime => value.GetDateTime(),
                XLDataType.Text => value.GetText(),
                _ => cell.GetFormattedString(),
            };
        }

        private static bool MatchesSheet(IXLWorksheet sheet, ExcelSheetSchema schema) =>
            string.Equals(sheet.Name, SafeName(schema.Caption), StringComparison.OrdinalIgnoreCase)
            || string.Equals(sheet.Name, schema.Key, StringComparison.OrdinalIgnoreCase);

        // ─── Sheet names ──────────────────────────────────────────────────

        private static string SafeName(string name)
        {
            var cleaned = new string(name.Where(ch => !"[]:*?/\\".Contains(ch)).ToArray()).Trim();
            if (cleaned.Length == 0) cleaned = "Sheet";
            return cleaned.Length > 31 ? cleaned[..31] : cleaned;
        }

        private static string UniqueName(string name, HashSet<string> used)
        {
            var baseName = SafeName(name);
            var candidate = baseName;
            for (var i = 2; !used.Add(candidate); i++)
            {
                var suffix = $" {i}";
                candidate = (baseName.Length + suffix.Length > 31 ? baseName[..(31 - suffix.Length)] : baseName) + suffix;
            }
            return candidate;
        }
    }
}
