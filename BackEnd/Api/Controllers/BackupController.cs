using Api.Controllers;
using Application.Commands;
using Application.Dtos;
using Application.Interfaces;
using Application.Queries;
using Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class BackupController : BaseController
{
    private const long MaxRestoreBytes = 4L * 1024 * 1024 * 1024;
    private const string RestoreConfirmation = "RESTORE";

    private readonly IMediator _mediator;
    private readonly IBackupService _backupService;

    public BackupController(IMediator mediator, IBackupService backupService)
    {
        _mediator = mediator;
        _backupService = backupService;
    }

    [HttpGet]
    public async Task<ActionResult<BackupListDto>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetBackupsQuery(), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<BackupFileDto>> Create(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateBackupCommand(), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>Full backup: database + uploads in one zip.</summary>
    [HttpPost("full")]
    public async Task<ActionResult<ServiceResult<BackupFileDto>>> CreateFull(CancellationToken cancellationToken)
    {
        var file = await _backupService.CreateFullAsync(cancellationToken);
        return Ok(ServiceResult<BackupFileDto>.Ok(file));
    }

    /// <summary>Replaces the database and uploads with the content of a full backup zip. SuperAdmin only.</summary>
    [HttpPost("restore")]
    [Authorize(Roles = "SuperAdmin")]
    [RequestSizeLimit(MaxRestoreBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxRestoreBytes)]
    public async Task<ActionResult<ServiceResult<RestoreResultDto>>> Restore(
        [FromForm] IFormFile file,
        [FromForm] string confirm,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(confirm?.Trim(), RestoreConfirmation, StringComparison.Ordinal))
            return BadRequest(ServiceResult<RestoreResultDto>.Failed("متن تأیید بازیابی وارد نشده یا درست نیست."));

        if (file is null || file.Length == 0)
            return BadRequest(ServiceResult<RestoreResultDto>.Failed("فایل پشتیبان انتخاب نشده است."));

        await using var stream = file.OpenReadStream();
        var result = await _backupService.RestoreFullAsync(stream, cancellationToken);
        return Ok(ServiceResult<RestoreResultDto>.Ok(result));
    }

    [HttpGet("settings")]
    public async Task<ActionResult<ServiceResult<BackupSettingsDto>>> GetSettings(CancellationToken cancellationToken)
    {
        var settings = await _backupService.GetSettingsAsync(cancellationToken);
        return Ok(ServiceResult<BackupSettingsDto>.Ok(settings));
    }

    [HttpPut("settings")]
    public async Task<ActionResult<ServiceResult<BackupSettingsDto>>> SaveSettings(
        [FromBody] BackupSettingsDto settings,
        CancellationToken cancellationToken)
    {
        var saved = await _backupService.SaveSettingsAsync(settings, cancellationToken);
        return Ok(ServiceResult<BackupSettingsDto>.Ok(saved));
    }

    [HttpGet("{fileName}/download")]
    public async Task<IActionResult> Download(string fileName, CancellationToken cancellationToken)
    {
        var file = await _backupService.OpenDownloadAsync(fileName, cancellationToken);
        if (file is null)
            return NotFound();

        await _backupService.MarkDownloadedAsync(file.Value.FileName, cancellationToken);
        return File(file.Value.Stream, file.Value.ContentType, file.Value.FileName);
    }

    [HttpDelete("{fileName}")]
    public async Task<ActionResult<BackupFileDto>> Delete(string fileName, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteBackupCommand { FileName = fileName }, cancellationToken);
        if (!result.IsSuccess)
        {
            if (string.Equals(result.Error, "فایل پشتیبان موردنظر پیدا نشد.", StringComparison.OrdinalIgnoreCase))
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }
}
