using Application.Dtos;

namespace Application.Common.Interfaces;

public interface ISeoOpsService
{
    Task<SeoHealthReportDto> ProbeHealthAsync(CancellationToken cancellationToken = default);

    Task<SeoOpsSnapshotDto> GetSnapshotAsync(
        int staleDays = 90,
        int take = 20,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SeoMetaSuggestionItemDto>> GetMetaAuditAsync(
        int take = 30,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SeoInventoryItemDto>> GetRefreshCandidatesAsync(
        int staleDays = 90,
        int take = 20,
        CancellationToken cancellationToken = default);

    Task<SeoInternalLinkSuggestionDto?> SuggestInternalLinksAsync(
        int blogId,
        int take = 8,
        CancellationToken cancellationToken = default);
}
