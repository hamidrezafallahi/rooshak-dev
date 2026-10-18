namespace Application.DataTransfer.Excel
{
    public sealed class ExcelSheetData
    {
        public required ExcelSheetSchema Schema { get; init; }
        public List<ExcelRow> Rows { get; init; } = [];
    }

    /// <summary>Everything the workbook writer needs; independent of the Excel library used.</summary>
    public sealed class ExcelWorkbookModel
    {
        public required string Title { get; init; }
        public required IReadOnlyList<ExcelSheetData> Sheets { get; init; }
        public IReadOnlyList<ExcelLookup> Lookups { get; init; } = [];
        public bool IncludeGuide { get; init; } = true;
    }

    /// <summary>
    /// Export options. Profiles apply what they support; unknown keys in <see cref="Filters"/> are ignored,
    /// so new filters can be added per entity without changing the endpoint contract.
    /// </summary>
    public sealed class ExcelExportRequest
    {
        /// <summary>Free-text search, same meaning as the admin list's search box.</summary>
        public string? Q { get; set; }

        /// <summary>null/false = all records (admin default), true = only active ones.</summary>
        public bool? OnlyActives { get; set; }

        /// <summary>Restrict the main sheet to these Ids (e.g. the rows selected in the admin grid).</summary>
        public List<int>? Ids { get; set; }

        /// <summary>Entity-specific filters, e.g. { "categoryId": "3", "brandId": "5" }.</summary>
        public Dictionary<string, string>? Filters { get; set; }

        /// <summary>Optional subset of sheet keys to include (main sheet is always included).</summary>
        public List<string>? Sheets { get; set; }

        public string? Filter(string key) =>
            Filters?.FirstOrDefault(f => string.Equals(f.Key, key, StringComparison.OrdinalIgnoreCase)).Value is { Length: > 0 } v
                ? v.Trim()
                : null;

        public int? FilterInt(string key) =>
            Filter(key) is { } v && int.TryParse(ExcelValue.NormalizeDigits(v), out var i) ? i : null;

        /// <summary>Comma-separated Ids, e.g. "productIds=1,2,3".</summary>
        public List<int>? FilterIntList(string key) =>
            Filter(key)?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(v => int.TryParse(ExcelValue.NormalizeDigits(v), out var i) ? i : (int?)null)
                .Where(i => i.HasValue).Select(i => i!.Value).ToList();

        public bool? FilterBool(string key) =>
            Filter(key) is { } v && bool.TryParse(v, out var b) ? b : null;

        public bool IncludesSheet(string sheetKey) =>
            Sheets is not { Count: > 0 } || Sheets.Contains(sheetKey, StringComparer.OrdinalIgnoreCase);
    }

    public sealed class ExcelImportOptions
    {
        /// <summary>Validate and run everything, then roll back. Nothing is saved.</summary>
        public bool DryRun { get; set; }

        /// <summary>
        /// true (default): if any row fails, nothing is saved. false: valid rows are saved and
        /// failed rows are reported.
        /// </summary>
        public bool AllOrNothing { get; set; } = true;
    }

    public sealed class ExcelImportError
    {
        public string Sheet { get; init; } = default!;
        public int Row { get; init; }
        public string Message { get; init; } = default!;
    }

    public sealed class ExcelSheetImportStats
    {
        public string Sheet { get; init; } = default!;
        public int Created { get; set; }
        public int Updated { get; set; }
        public int Failed { get; set; }
        public int Skipped { get; set; }
    }

    public sealed class ExcelImportResult
    {
        public string Entity { get; init; } = default!;
        public bool DryRun { get; init; }
        public bool Committed { get; set; }
        public List<ExcelSheetImportStats> Sheets { get; init; } = [];
        public List<ExcelImportError> Errors { get; init; } = [];

        public int Created => Sheets.Sum(s => s.Created);
        public int Updated => Sheets.Sum(s => s.Updated);
        public int Failed => Sheets.Sum(s => s.Failed);
    }

    public sealed class ExcelFileDto
    {
        public required string FileName { get; init; }
        public required byte[] Content { get; init; }
        public const string ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    }

    public sealed class ExcelEntityInfoDto
    {
        public string Entity { get; init; } = default!;
        public string DisplayName { get; init; } = default!;
        public bool CanExport { get; init; }
        public bool CanImport { get; init; }
        public List<ExcelSheetInfoDto> Sheets { get; init; } = [];
    }

    public sealed class ExcelSheetInfoDto
    {
        public string Key { get; init; } = default!;
        public string Caption { get; init; } = default!;
    }
}
