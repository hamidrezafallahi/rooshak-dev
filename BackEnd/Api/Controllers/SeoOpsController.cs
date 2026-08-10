using Application.Common.Interfaces;
using Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin,Admin,ContentEditor")]
public class SeoOpsController(ISeoOpsService seoOpsService) : BaseController
{
    /// <summary>
    /// Probe public SEO endpoints (sitemap, robots, homepage, blog, API health).
    /// </summary>
    [HttpGet("health")]
    public async Task<ActionResult<SeoHealthReportDto>> Health(CancellationToken cancellationToken)
    {
        var report = await seoOpsService.ProbeHealthAsync(cancellationToken);
        return Ok(new { isSuccess = true, data = report });
    }

    /// <summary>
    /// Inventory snapshot for weekly digests and prioritization.
    /// </summary>
    [HttpGet("snapshot")]
    public async Task<ActionResult<SeoOpsSnapshotDto>> Snapshot(
        [FromQuery] int staleDays = 90,
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await seoOpsService.GetSnapshotAsync(staleDays, take, cancellationToken);
        return Ok(new { isSuccess = true, data = snapshot });
    }

    /// <summary>
    /// Active blogs with weak title/meta for human-gated optimization.
    /// </summary>
    [HttpGet("meta-audit")]
    public async Task<ActionResult<IReadOnlyList<SeoMetaSuggestionItemDto>>> MetaAudit(
        [FromQuery] int take = 30,
        CancellationToken cancellationToken = default)
    {
        var items = await seoOpsService.GetMetaAuditAsync(take, cancellationToken);
        return Ok(new { isSuccess = true, data = items });
    }

    /// <summary>
    /// Stale active blogs that should be refreshed.
    /// </summary>
    [HttpGet("refresh-candidates")]
    public async Task<ActionResult<IReadOnlyList<SeoInventoryItemDto>>> RefreshCandidates(
        [FromQuery] int staleDays = 90,
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default)
    {
        var items = await seoOpsService.GetRefreshCandidatesAsync(staleDays, take, cancellationToken);
        return Ok(new { isSuccess = true, data = items });
    }

    /// <summary>
    /// Internal link suggestions for a blog (human applies in admin).
    /// </summary>
    [HttpGet("internal-links/{blogId:int}")]
    public async Task<ActionResult<SeoInternalLinkSuggestionDto>> InternalLinks(
        int blogId,
        [FromQuery] int take = 8,
        CancellationToken cancellationToken = default)
    {
        var result = await seoOpsService.SuggestInternalLinksAsync(blogId, take, cancellationToken);
        if (result is null)
            return NotFound(new { isSuccess = false, error = "Blog not found" });

        return Ok(new { isSuccess = true, data = result });
    }
}
