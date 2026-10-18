using Application.Common.Interfaces;

namespace Application.DataTransfer.Excel
{
    public enum ExcelRowOutcome
    {
        Created,
        Updated,
        Skipped,
    }

    /// <summary>
    /// State of one import run: the parsed sheets, the result being built, and the map of
    /// <c>Ref</c> values to the Ids they produced so later sheets can point at new rows.
    /// </summary>
    public sealed class ExcelImportContext
    {
        private readonly IBulkOperationSession _session;
        private readonly Dictionary<string, ExcelSheetData> _sheets;
        private readonly Dictionary<string, Dictionary<string, int>> _refs = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, HashSet<string>> _failedRefs = new(StringComparer.OrdinalIgnoreCase);
        private int _savepointCounter;

        public ExcelImportContext(
            IBulkOperationSession session,
            IEnumerable<ExcelSheetData> sheets,
            ExcelImportResult result,
            CancellationToken cancellationToken)
        {
            _session = session;
            _sheets = sheets.ToDictionary(s => s.Schema.Key, StringComparer.OrdinalIgnoreCase);
            Result = result;
            CancellationToken = cancellationToken;
        }

        public ExcelImportResult Result { get; }
        public CancellationToken CancellationToken { get; }

        public ExcelSheetData? Sheet(string key) => _sheets.GetValueOrDefault(key);

        /// <summary>
        /// Runs <paramref name="handler"/> for every non-empty row of the sheet. Each row gets its own
        /// savepoint; a failing row is rolled back and reported without stopping the others.
        /// </summary>
        public async Task ProcessSheetAsync(
            string sheetKey,
            Func<ExcelRow, Task<(ExcelRowOutcome Outcome, int? Id)>> handler,
            Func<IReadOnlyList<ExcelRow>, IEnumerable<ExcelRow>>? order = null)
        {
            var sheet = Sheet(sheetKey);
            if (sheet is null) return;

            var stats = new ExcelSheetImportStats { Sheet = sheet.Schema.Caption };
            Result.Sheets.Add(stats);

            var seenRefs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in order is null ? sheet.Rows : order(sheet.Rows))
            {
                CancellationToken.ThrowIfCancellationRequested();
                if (row.IsEmpty) continue;

                var savepoint = $"xl_{++_savepointCounter}";
                var rowRef = row.Ref;
                await _session.CreateSavepointAsync(savepoint, CancellationToken);
                try
                {
                    if (rowRef is not null && !seenRefs.Add(rowRef))
                        throw new ExcelRowException($"«کلید ارجاع» {rowRef} تکراری است.");

                    if (row.Id is null)
                        ValidateRequired(sheet.Schema, row);

                    var (outcome, id) = await handler(row);
                    await _session.ReleaseSavepointAsync(savepoint, CancellationToken);

                    switch (outcome)
                    {
                        case ExcelRowOutcome.Created: stats.Created++; break;
                        case ExcelRowOutcome.Updated: stats.Updated++; break;
                        default: stats.Skipped++; break;
                    }

                    if (rowRef is not null && id is not null)
                        RefsOf(sheetKey)[rowRef] = id.Value;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    await _session.RollbackToSavepointAsync(savepoint, CancellationToken);
                    stats.Failed++;
                    if (rowRef is not null)
                        FailedRefsOf(sheetKey).Add(rowRef);
                    Result.Errors.Add(new ExcelImportError
                    {
                        Sheet = sheet.Schema.Caption,
                        Row = row.RowNumber,
                        Message = Describe(ex),
                    });
                }
            }
        }

        /// <summary>
        /// Reads a foreign-key cell: an Id, a dropdown value ("12 - name") or, when the column
        /// declares <see cref="ExcelColumn.ReferencesSheet"/>, the <c>Ref</c> of a row in that sheet.
        /// </summary>
        public int? ResolveId(ExcelSheetSchema schema, ExcelRow row, string columnKey)
        {
            var column = schema.FindColumn(columnKey);
            var text = row.GetString(columnKey);
            if (text is null) return null;

            if (column?.ReferencesSheet is { } refSheet)
            {
                if (RefsOf(refSheet).TryGetValue(text, out var id))
                    return id;
                if (FailedRefsOf(refSheet).Contains(text))
                    throw new ExcelRowException($"ردیفِ مرجع «{text}» در شیت «{Sheet(refSheet)?.Schema.Caption ?? refSheet}» خطا داشت و ثبت نشد.");
            }

            try
            {
                return row.GetInt(columnKey);
            }
            catch (ExcelRowException) when (column?.ReferencesSheet is not null)
            {
                throw new ExcelRowException($"مقدار «{text}» در ستون «{column.Caption}» نه شناسه است و نه «کلید ارجاع» یک ردیف در همین فایل.");
            }
        }

        public int RequireId(ExcelSheetSchema schema, ExcelRow row, string columnKey) =>
            ResolveId(schema, row, columnKey)
            ?? throw new ExcelRowException($"ستون «{schema.FindColumn(columnKey)?.Caption ?? columnKey}» اجباری است.");

        /// <summary>
        /// Orders a self-referencing sheet (e.g. categories) so a row whose parent column holds the
        /// <c>Ref</c> of another row comes after that row. Cycles keep their original order.
        /// </summary>
        public static IEnumerable<ExcelRow> ParentsFirst(IReadOnlyList<ExcelRow> rows, string parentColumnKey)
        {
            var byRef = rows.Where(r => r.Ref is not null)
                .GroupBy(r => r.Ref!, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
            var emitted = new HashSet<ExcelRow>();
            var visiting = new HashSet<ExcelRow>();
            var ordered = new List<ExcelRow>(rows.Count);

            void Visit(ExcelRow row)
            {
                if (emitted.Contains(row) || !visiting.Add(row)) return;
                if (row.GetString(parentColumnKey) is { } parentRef && byRef.TryGetValue(parentRef, out var parent))
                    Visit(parent);
                visiting.Remove(row);
                if (emitted.Add(row)) ordered.Add(row);
            }

            foreach (var row in rows) Visit(row);
            return ordered;
        }

        private static void ValidateRequired(ExcelSheetSchema schema, ExcelRow row)
        {
            var missing = schema.Columns
                .Where(c => c.Required && !c.ReadOnly && !row.Has(c.Key))
                .Select(c => c.Caption)
                .ToList();
            if (missing.Count > 0)
                throw new ExcelRowException($"برای ثبت رکورد جدید این ستون‌ها اجباری است: {string.Join("، ", missing)}");
        }

        private Dictionary<string, int> RefsOf(string sheetKey)
        {
            if (!_refs.TryGetValue(sheetKey, out var map))
                _refs[sheetKey] = map = new(StringComparer.OrdinalIgnoreCase);
            return map;
        }

        private HashSet<string> FailedRefsOf(string sheetKey)
        {
            if (!_failedRefs.TryGetValue(sheetKey, out var set))
                _failedRefs[sheetKey] = set = new(StringComparer.OrdinalIgnoreCase);
            return set;
        }

        private static string Describe(Exception ex)
        {
            if (ex is ExcelRowException) return ex.Message;
            var inner = ex;
            while (inner.InnerException is not null) inner = inner.InnerException;
            return inner.Message;
        }
    }
}
