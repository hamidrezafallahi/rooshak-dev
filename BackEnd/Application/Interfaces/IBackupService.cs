using Application.Dtos;

namespace Application.Interfaces
{
    public interface IBackupService
    {
        /// <summary>Database-only plain SQL dump (legacy).</summary>
        Task<BackupFileDto> CreateAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<BackupFileDto>> ListAsync(CancellationToken cancellationToken = default);
        Task<(Stream Stream, string ContentType, string FileName)?> OpenDownloadAsync(
            string fileName,
            CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string fileName, CancellationToken cancellationToken = default);

        /// <summary>Full backup: database (pg_dump -Fc) + uploads folder in a single zip.</summary>
        Task<BackupFileDto> CreateFullAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Restores a full backup zip: takes a safety backup first, then replaces the database and uploads.
        /// </summary>
        Task<RestoreResultDto> RestoreFullAsync(Stream zip, CancellationToken cancellationToken = default);

        Task<BackupSettingsDto> GetSettingsAsync(CancellationToken cancellationToken = default);
        Task<BackupSettingsDto> SaveSettingsAsync(BackupSettingsDto settings, CancellationToken cancellationToken = default);
        Task MarkDownloadedAsync(string fileName, CancellationToken cancellationToken = default);

        /// <summary>Runs the scheduled backup if it is due. Returns true when a backup was created.</summary>
        Task<bool> RunScheduledIfDueAsync(CancellationToken cancellationToken = default);
    }
}
