using Api.Controllers;
using Application.DataTransfer.Excel;
using Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Generic Excel export / import / template for admin entities.
/// Entities are provided by <see cref="IExcelEntityProfile"/> implementations; the route's
/// <c>entity</c> is the admin endpoint name (products, brands, ...).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]
public class ExcelController(IMediator mediator) : BaseController
{
    private const long MaxImportBytes = 10 * 1024 * 1024;

    /// <summary>Query keys with a fixed meaning; every other query key is passed to the profile as a filter.</summary>
    private static readonly HashSet<string> ReservedExportKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "q", "onlyActives", "ids", "sheets",
    };

    [HttpGet]
    public async Task<ActionResult<ServiceResult<List<ExcelEntityInfoDto>>>> GetEntities([FromQuery] string? entity)
        => Ok(await mediator.Send(new GetExcelEntitiesQuery { Entity = entity }));

    [HttpGet("{entity}/template")]
    public async Task<IActionResult> Template(string entity, CancellationToken cancellationToken)
        => ToFile(await mediator.Send(new GetExcelTemplateQuery { Entity = entity }, cancellationToken));

    /// <summary>
    /// Simple export: <c>?q=&amp;onlyActives=&amp;ids=1,2&amp;sheets=productOffers&amp;categoryId=3</c>.
    /// Unknown query keys become entity-specific filters.
    /// </summary>
    [HttpGet("{entity}/export")]
    public async Task<IActionResult> Export(string entity, CancellationToken cancellationToken)
    {
        var query = Request.Query;
        var request = new ExcelExportRequest
        {
            Q = query["q"].FirstOrDefault(),
            OnlyActives = bool.TryParse(query["onlyActives"].FirstOrDefault(), out var onlyActives) ? onlyActives : null,
            Ids = ParseIds(query["ids"].ToString()),
            Sheets = ParseList(query["sheets"].ToString()),
            Filters = query
                .Where(kv => !ReservedExportKeys.Contains(kv.Key))
                .ToDictionary(kv => kv.Key, kv => kv.Value.ToString(), StringComparer.OrdinalIgnoreCase),
        };
        return ToFile(await mediator.Send(new ExportExcelQuery { Entity = entity, Request = request }, cancellationToken));
    }

    /// <summary>Export with a JSON body, for filters that do not fit in a query string.</summary>
    [HttpPost("{entity}/export")]
    public async Task<IActionResult> ExportWithBody(string entity, [FromBody] ExcelExportRequest request, CancellationToken cancellationToken)
        => ToFile(await mediator.Send(new ExportExcelQuery { Entity = entity, Request = request }, cancellationToken));

    [HttpPost("{entity}/import")]
    [RequestSizeLimit(MaxImportBytes)]
    public async Task<ActionResult<ServiceResult<ExcelImportResult>>> Import(
        string entity,
        IFormFile? file,
        [FromQuery] bool dryRun = false,
        [FromQuery] bool allOrNothing = true,
        CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ServiceResult<ExcelImportResult>.Failed("فایلی ارسال نشده است."));
        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            return BadRequest(ServiceResult<ExcelImportResult>.Failed("فقط فایل اکسل با پسوند xlsx پذیرفته می‌شود."));
        if (file.Length > MaxImportBytes)
            return BadRequest(ServiceResult<ExcelImportResult>.Failed("حجم فایل بیش از ۱۰ مگابایت است."));

        await using var stream = new MemoryStream();
        await file.CopyToAsync(stream, cancellationToken);
        stream.Position = 0;

        var result = await mediator.Send(new ImportExcelCommand
        {
            Entity = entity,
            File = stream,
            Options = new ExcelImportOptions { DryRun = dryRun, AllOrNothing = allOrNothing },
        }, cancellationToken);

        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    private IActionResult ToFile(ServiceResult<ExcelFileDto> result)
    {
        if (!result.IsSuccess || result.Data is null)
            return NotFound(result);
        return File(result.Data.Content, ExcelFileDto.ContentType, result.Data.FileName);
    }

    private static List<int>? ParseIds(string? value)
    {
        var ids = ParseList(value)?
            .Select(v => int.TryParse(v, out var id) ? id : (int?)null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToList();
        return ids is { Count: > 0 } ? ids : null;
    }

    private static List<string>? ParseList(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
}
