namespace Application.Dtos;

public class SeoHealthCheckDto
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public int? StatusCode { get; set; }
    public long ElapsedMs { get; set; }
    public bool Ok { get; set; }
    public string? Error { get; set; }
}

public class SeoHealthReportDto
{
    public DateTime CheckedAtUtc { get; set; }
    public string SiteBaseUrl { get; set; } = string.Empty;
    public string ApiBaseUrl { get; set; } = string.Empty;
    public bool IsHealthy { get; set; }
    public int FailedCount { get; set; }
    public IReadOnlyList<SeoHealthCheckDto> Checks { get; set; } = Array.Empty<SeoHealthCheckDto>();
}

public class SeoInventoryItemDto
{
    public int Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string TitleFa { get; set; } = string.Empty;
    public string? MetaDescriptionFa { get; set; }
    public bool IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? AgeDays { get; set; }
    public IReadOnlyList<string> Issues { get; set; } = Array.Empty<string>();
    public string Path { get; set; } = string.Empty;
}

public class SeoLinkTargetDto
{
    public string Type { get; set; } = string.Empty;
    public int Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string TitleFa { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int Score { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class SeoInternalLinkSuggestionDto
{
    public int BlogId { get; set; }
    public string BlogSlug { get; set; } = string.Empty;
    public string BlogTitleFa { get; set; } = string.Empty;
    public int ExistingInternalLinkCount { get; set; }
    public IReadOnlyList<SeoLinkTargetDto> Suggestions { get; set; } = Array.Empty<SeoLinkTargetDto>();
}

public class SeoOpsSnapshotDto
{
    public DateTime GeneratedAtUtc { get; set; }
    public int ActiveBlogs { get; set; }
    public int InactiveBlogs { get; set; }
    public int ActiveProducts { get; set; }
    public int ActiveCategories { get; set; }
    public int ActiveBrands { get; set; }
    public int WeakMetaCount { get; set; }
    public int StaleBlogCount { get; set; }
    public int DraftAiPipelineHint { get; set; }
    public IReadOnlyList<SeoInventoryItemDto> WeakMetaBlogs { get; set; } = Array.Empty<SeoInventoryItemDto>();
    public IReadOnlyList<SeoInventoryItemDto> RefreshCandidates { get; set; } = Array.Empty<SeoInventoryItemDto>();
    public IReadOnlyList<SeoInventoryItemDto> RecentInactiveDrafts { get; set; } = Array.Empty<SeoInventoryItemDto>();
}

public class SeoMetaSuggestionItemDto
{
    public int BlogId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string CurrentTitleFa { get; set; } = string.Empty;
    public string? CurrentMetaDescriptionFa { get; set; }
    public IReadOnlyList<string> Issues { get; set; } = Array.Empty<string>();
    public string Path { get; set; } = string.Empty;
}
