using Application.Common.Interfaces;
using Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace Application.DataTransfer.Excel
{
    public sealed class GetExcelEntitiesQuery : IRequest<ServiceResult<List<ExcelEntityInfoDto>>>
    {
        /// <summary>Optional: only this entity.</summary>
        public string? Entity { get; set; }
    }

    public sealed class GetExcelTemplateQuery : IRequest<ServiceResult<ExcelFileDto>>
    {
        public string Entity { get; set; } = default!;
    }

    public sealed class ExportExcelQuery : IRequest<ServiceResult<ExcelFileDto>>
    {
        public string Entity { get; set; } = default!;
        public ExcelExportRequest Request { get; set; } = new();
    }

    public sealed class ImportExcelCommand : IRequest<ServiceResult<ExcelImportResult>>
    {
        public string Entity { get; set; } = default!;
        public Stream File { get; set; } = default!;
        public ExcelImportOptions Options { get; set; } = new();
    }

    public sealed class ExcelProfileRegistry(IEnumerable<IExcelEntityProfile> profiles)
    {
        public IReadOnlyList<IExcelEntityProfile> All { get; } = profiles.ToList();

        public IExcelEntityProfile? Find(string? entity, ClaimsPrincipal? user) =>
            All.FirstOrDefault(p => string.Equals(p.EntityKey, entity?.Trim(), StringComparison.OrdinalIgnoreCase)
                                    && user is not null && IsAllowed(p, user));

        public static bool IsAllowed(IExcelEntityProfile profile, ClaimsPrincipal user) =>
            profile.AllowedRoles.Any(user.IsInRole);
    }

    public sealed class ExcelRequestHandlers(
        ExcelProfileRegistry registry,
        IExcelWorkbookService workbook,
        IBulkOperationSession session,
        IHttpContextAccessor accessor) :
        IRequestHandler<GetExcelEntitiesQuery, ServiceResult<List<ExcelEntityInfoDto>>>,
        IRequestHandler<GetExcelTemplateQuery, ServiceResult<ExcelFileDto>>,
        IRequestHandler<ExportExcelQuery, ServiceResult<ExcelFileDto>>,
        IRequestHandler<ImportExcelCommand, ServiceResult<ExcelImportResult>>
    {
        private ClaimsPrincipal? User => accessor.HttpContext?.User;

        private const string NotSupported = "خروجی/ورود اکسل برای این موجودیت تعریف نشده است.";

        public Task<ServiceResult<List<ExcelEntityInfoDto>>> Handle(GetExcelEntitiesQuery request, CancellationToken cancellationToken)
        {
            var user = User;
            var list = registry.All
                .Where(p => user is not null && ExcelProfileRegistry.IsAllowed(p, user))
                .Where(p => request.Entity is null || string.Equals(p.EntityKey, request.Entity, StringComparison.OrdinalIgnoreCase))
                .Select(p => new ExcelEntityInfoDto
                {
                    Entity = p.EntityKey,
                    DisplayName = p.DisplayName,
                    CanExport = p.CanExport,
                    CanImport = p.CanImport,
                    Sheets = p.Sheets.Select(s => new ExcelSheetInfoDto { Key = s.Key, Caption = s.Caption }).ToList(),
                })
                .ToList();
            return Task.FromResult(ServiceResult<List<ExcelEntityInfoDto>>.Ok(list));
        }

        public async Task<ServiceResult<ExcelFileDto>> Handle(GetExcelTemplateQuery request, CancellationToken cancellationToken)
        {
            var profile = registry.Find(request.Entity, User);
            if (profile is null || !profile.CanImport)
                return ServiceResult<ExcelFileDto>.Failed(NotSupported);

            var lookups = await profile.GetLookupsAsync(cancellationToken);
            var content = workbook.Write(new ExcelWorkbookModel
            {
                Title = $"قالب ورود {profile.DisplayName}",
                Sheets = profile.Sheets.Select(s => new ExcelSheetData { Schema = s }).ToList(),
                Lookups = lookups,
            });

            return ServiceResult<ExcelFileDto>.Ok(new ExcelFileDto
            {
                FileName = $"{profile.EntityKey}-template.xlsx",
                Content = content,
            });
        }

        public async Task<ServiceResult<ExcelFileDto>> Handle(ExportExcelQuery request, CancellationToken cancellationToken)
        {
            var profile = registry.Find(request.Entity, User);
            if (profile is null || !profile.CanExport)
                return ServiceResult<ExcelFileDto>.Failed(NotSupported);

            var options = request.Request ?? new ExcelExportRequest();
            var rows = await profile.ExportAsync(options, cancellationToken);
            var lookups = await profile.GetLookupsAsync(cancellationToken);

            var sheets = profile.Sheets
                .Where((s, index) => index == 0 || options.IncludesSheet(s.Key))
                .Select(s => new ExcelSheetData
                {
                    Schema = s,
                    Rows = rows.TryGetValue(s.Key, out var r) ? r : [],
                })
                .ToList();

            var content = workbook.Write(new ExcelWorkbookModel
            {
                Title = $"خروجی {profile.DisplayName}",
                Sheets = sheets,
                // Lookups (and dropdowns) only matter if the file can be edited and re-imported.
                Lookups = profile.CanImport ? lookups : [],
                IncludeGuide = profile.CanImport,
            });

            return ServiceResult<ExcelFileDto>.Ok(new ExcelFileDto
            {
                FileName = $"{profile.EntityKey}-{DateTime.Now:yyyyMMdd-HHmm}.xlsx",
                Content = content,
            });
        }

        public async Task<ServiceResult<ExcelImportResult>> Handle(ImportExcelCommand request, CancellationToken cancellationToken)
        {
            var profile = registry.Find(request.Entity, User);
            if (profile is null || !profile.CanImport)
                return ServiceResult<ExcelImportResult>.Failed(NotSupported);

            IReadOnlyList<ExcelSheetData> sheets;
            try
            {
                sheets = workbook.Read(request.File, profile.Sheets);
            }
            catch (ExcelFormatException ex)
            {
                return ServiceResult<ExcelImportResult>.Failed(ex.Message);
            }

            if (sheets.All(s => s.Rows.All(r => r.IsEmpty)))
                return ServiceResult<ExcelImportResult>.Failed("فایل هیچ ردیف داده‌ای ندارد.");

            var options = request.Options ?? new ExcelImportOptions();
            var result = new ExcelImportResult { Entity = profile.EntityKey, DryRun = options.DryRun };

            await using var transaction = await session.BeginTransactionAsync(cancellationToken);
            await profile.ImportAsync(new ExcelImportContext(session, sheets, result, cancellationToken));

            var shouldCommit = !options.DryRun && (!options.AllOrNothing || result.Errors.Count == 0);
            if (shouldCommit)
                await transaction.CommitAsync(cancellationToken);
            else
                await transaction.RollbackAsync(cancellationToken);

            result.Committed = shouldCommit;
            return ServiceResult<ExcelImportResult>.Ok(result);
        }
    }

    public static class ExcelServiceCollectionExtensions
    {
        /// <summary>Registers the Excel handlers' registry and every <see cref="IExcelEntityProfile"/> in Application.</summary>
        public static IServiceCollection AddExcelDataTransfer(this IServiceCollection services)
        {
            var profileTypes = typeof(IExcelEntityProfile).Assembly.GetTypes()
                .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IExcelEntityProfile).IsAssignableFrom(t));

            foreach (var type in profileTypes)
                services.AddScoped(typeof(IExcelEntityProfile), type);

            services.AddScoped<ExcelProfileRegistry>();
            return services;
        }
    }
}
