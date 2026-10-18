using Common;
using MediatR;

namespace Application.DataTransfer.Excel
{
    /// <summary>
    /// Describes how one admin entity is exported to / imported from Excel.
    /// Add a new entity by subclassing <see cref="ExcelEntityProfile"/>; profiles are discovered
    /// automatically (see <see cref="ExcelServiceCollectionExtensions"/>).
    /// </summary>
    public interface IExcelEntityProfile
    {
        /// <summary>Admin endpoint name, e.g. "products" (matched case-insensitively).</summary>
        string EntityKey { get; }

        string DisplayName { get; }

        bool CanExport { get; }

        bool CanImport { get; }

        /// <summary>Roles allowed to use this profile (checked on top of the controller-level policy).</summary>
        IReadOnlyList<string> AllowedRoles { get; }

        /// <summary>Sheets in workbook order; the first one is the main entity.</summary>
        IReadOnlyList<ExcelSheetSchema> Sheets { get; }

        Task<IReadOnlyList<ExcelLookup>> GetLookupsAsync(CancellationToken cancellationToken);

        /// <summary>Returns rows per sheet key.</summary>
        Task<IReadOnlyDictionary<string, List<ExcelRow>>> ExportAsync(ExcelExportRequest request, CancellationToken cancellationToken);

        /// <summary>Imports the parsed sheets. Process parent sheets before child sheets.</summary>
        Task ImportAsync(ExcelImportContext context);
    }

    public abstract class ExcelEntityProfile(IMediator mediator) : IExcelEntityProfile
    {
        private IReadOnlyList<ExcelSheetSchema>? _sheets;

        protected IMediator Mediator { get; } = mediator;

        public abstract string EntityKey { get; }
        public abstract string DisplayName { get; }
        public virtual bool CanExport => true;
        public virtual bool CanImport => true;
        public virtual IReadOnlyList<string> AllowedRoles => ["SuperAdmin", "Admin", "ContentEditor"];

        public IReadOnlyList<ExcelSheetSchema> Sheets => _sheets ??= DefineSheets().ToList();

        protected ExcelSheetSchema MainSheet => Sheets[0];

        protected ExcelSheetSchema SheetOf(string key) =>
            Sheets.First(s => string.Equals(s.Key, key, StringComparison.OrdinalIgnoreCase));

        /// <summary>Declare sheets with <see cref="ExcelSheetBuilder"/>; called once.</summary>
        protected abstract IEnumerable<ExcelSheetSchema> DefineSheets();

        public virtual Task<IReadOnlyList<ExcelLookup>> GetLookupsAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<ExcelLookup>>([]);

        public abstract Task<IReadOnlyDictionary<string, List<ExcelRow>>> ExportAsync(ExcelExportRequest request, CancellationToken cancellationToken);

        public virtual Task ImportAsync(ExcelImportContext context) =>
            throw new NotSupportedException($"Import is not supported for '{EntityKey}'.");

        // ─── Helpers for subclasses ───────────────────────────────────────

        protected static ExcelSheetBuilder Sheet(string key, string caption) => new(key, caption);

        protected static ExcelRow Row(IDictionary<string, object?> values) => new(0, values);

        /// <summary>
        /// Sends an existing Create/Update command so all domain rules, slug handling and
        /// permissions apply exactly as in the admin form. A failed result aborts the row.
        /// </summary>
        protected async Task<int> SendAsync(IRequest<ServiceResult<IdDto>> command, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(command, cancellationToken);
            if (result is null || !result.IsSuccess)
                throw new ExcelRowException(result?.Error ?? "عملیات ناموفق بود.");
            return result.Data?.Id ?? 0;
        }

        protected static ExcelLookup Lookup(string key, string caption, IEnumerable<ExcelLookupItem> items) =>
            new() { Key = key, Caption = caption, Items = items.ToList() };

        /// <summary>Runs <paramref name="create"/> when the row has no Id, otherwise <paramref name="update"/>.</summary>
        protected static async Task<(ExcelRowOutcome Outcome, int? Id)> UpsertAsync(
            ExcelRow row, Func<Task<int>> create, Func<int, Task<int>> update) =>
            row.Id is { } id
                ? (ExcelRowOutcome.Updated, await update(id))
                : (ExcelRowOutcome.Created, await create());

        /// <summary>Applies the optional <c>IsActive</c> column through the entity's existing Active command.</summary>
        protected async Task SetActiveAsync<TCommand>(ExcelRow row, int id, CancellationToken cancellationToken)
            where TCommand : ActiveCommand, IRequest<ServiceResult<IdDto>>, new()
        {
            if (row.GetBool("IsActive") is { } isActive)
                await SendAsync(new TCommand { Id = id, IsActive = isActive }, cancellationToken);
        }
    }
}
