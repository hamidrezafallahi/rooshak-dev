namespace Application.Dtos
{
    public class BackupFileDto
    {
        public string FileName { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        /// <summary>database (plain .sql) | full (manual zip) | auto (scheduled zip) | pre-restore (safety zip).</summary>
        public string Kind { get; set; } = "database";
    }

    public class BackupListDto
    {
        public List<BackupFileDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
    }

    public class BackupSettingsDto
    {
        public bool AutoEnabled { get; set; }

        /// <summary>Hour of day (0-23, UTC) at which the scheduled backup runs.</summary>
        public int HourUtc { get; set; } = 23;

        /// <summary>How many scheduled backups to keep.</summary>
        public int KeepCount { get; set; } = 7;

        public DateTime? LastAutoBackupUtc { get; set; }

        /// <summary>When an admin last downloaded a full backup (off-server copy reminder).</summary>
        public DateTime? LastDownloadedUtc { get; set; }
    }

    public class RestoreResultDto
    {
        public string SafetyBackupFileName { get; set; } = string.Empty;
        public int RestoredFileCount { get; set; }
    }
}
