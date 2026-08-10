using System.Diagnostics;
using System.Text.RegularExpressions;
using Application.Common.Interfaces;
using Application.Dtos;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OnlineShop.Domain.Interfaces;

namespace Infrastructure.Services;

public class SeoOpsService(
    IBlogRepository blogRepository,
    IProductRepository productRepository,
    ICategoryRepository categoryRepository,
    IBrandRepository brandRepository,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration) : ISeoOpsService
{
    private static readonly Regex InternalLinkRegex = new(
        @"href\s*=\s*[""'][^""']*(/products/|/categories/|/blog/)[^""']*[""']",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex HtmlTagRegex = new("<[^>]+>", RegexOptions.Compiled);

    public async Task<SeoHealthReportDto> ProbeHealthAsync(CancellationToken cancellationToken = default)
    {
        var siteBase = TrimTrailingSlash(
            configuration["SeoOps:SitePublicUrl"]
            ?? configuration["NEXT_PUBLIC_SITE_URL"]
            ?? "http://localhost");

        var apiBase = TrimTrailingSlash(
            configuration["SeoOps:ApiPublicUrl"]
            ?? configuration["API_PUBLIC_URL"]
            ?? "http://localhost:8080");

        var targets = new (string Name, string Url)[]
        {
            ("frontend-home", $"{siteBase}/"),
            ("frontend-fa", $"{siteBase}/fa"),
            ("frontend-blog", $"{siteBase}/fa/blog"),
            ("sitemap", $"{siteBase}/sitemap.xml"),
            ("robots", $"{siteBase}/robots.txt"),
            ("llms", $"{siteBase}/llms.txt"),
            ("api-health", $"{apiBase}/health"),
        };

        var client = httpClientFactory.CreateClient("SeoOpsProbe");
        var checks = new List<SeoHealthCheckDto>(targets.Length);

        foreach (var (name, url) in targets)
        {
            var sw = Stopwatch.StartNew();
            var check = new SeoHealthCheckDto { Name = name, Url = url };
            try
            {
                using var response = await client.GetAsync(url, cancellationToken);
                check.StatusCode = (int)response.StatusCode;
                check.Ok = response.IsSuccessStatusCode;
                if (!check.Ok)
                    check.Error = $"HTTP {(int)response.StatusCode}";
            }
            catch (Exception ex)
            {
                check.Ok = false;
                check.Error = ex.Message;
            }
            finally
            {
                sw.Stop();
                check.ElapsedMs = sw.ElapsedMilliseconds;
            }

            checks.Add(check);
        }

        var failed = checks.Count(c => !c.Ok);
        return new SeoHealthReportDto
        {
            CheckedAtUtc = DateTime.UtcNow,
            SiteBaseUrl = siteBase,
            ApiBaseUrl = apiBase,
            IsHealthy = failed == 0,
            FailedCount = failed,
            Checks = checks,
        };
    }

    public async Task<SeoOpsSnapshotDto> GetSnapshotAsync(
        int staleDays = 90,
        int take = 20,
        CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 100);
        staleDays = Math.Clamp(staleDays, 7, 730);
        var cutoff = DateTime.UtcNow.AddDays(-staleDays);

        var blogs = await blogRepository.Query(b => !b.IsDeleted)
            .Select(b => new
            {
                b.Id,
                b.Slug,
                b.TitleFa,
                b.MetaDescriptionFa,
                b.IsActive,
                b.CreatedAt,
                b.UpdatedAt,
            })
            .ToListAsync(cancellationToken);

        var activeProducts = await productRepository.Query(p => p.IsActive && !p.IsDeleted).CountAsync(cancellationToken);
        var activeCategories = await categoryRepository.Query(c => c.IsActive && !c.IsDeleted).CountAsync(cancellationToken);
        var activeBrands = await brandRepository.Query(b => b.IsActive && !b.IsDeleted).CountAsync(cancellationToken);

        var weakMeta = blogs
            .Select(b => ToInventoryItem(b.Id, b.Slug, b.TitleFa, b.MetaDescriptionFa, b.IsActive, b.CreatedAt, b.UpdatedAt, "/fa/blog/"))
            .Where(x => x.Issues.Count > 0)
            .OrderByDescending(x => x.Issues.Count)
            .ThenByDescending(x => x.UpdatedAt)
            .Take(take)
            .ToList();

        var refresh = blogs
            .Where(b => b.IsActive)
            .Select(b => ToInventoryItem(b.Id, b.Slug, b.TitleFa, b.MetaDescriptionFa, b.IsActive, b.CreatedAt, b.UpdatedAt, "/fa/blog/"))
            .Where(x => (x.UpdatedAt ?? x.CreatedAt) < cutoff)
            .OrderBy(x => x.UpdatedAt ?? x.CreatedAt)
            .Take(take)
            .ToList();

        var recentDrafts = blogs
            .Where(b => !b.IsActive)
            .OrderByDescending(b => b.UpdatedAt ?? b.CreatedAt)
            .Take(Math.Min(take, 10))
            .Select(b => ToInventoryItem(b.Id, b.Slug, b.TitleFa, b.MetaDescriptionFa, b.IsActive, b.CreatedAt, b.UpdatedAt, "/fa/blog/"))
            .ToList();

        return new SeoOpsSnapshotDto
        {
            GeneratedAtUtc = DateTime.UtcNow,
            ActiveBlogs = blogs.Count(b => b.IsActive),
            InactiveBlogs = blogs.Count(b => !b.IsActive),
            ActiveProducts = activeProducts,
            ActiveCategories = activeCategories,
            ActiveBrands = activeBrands,
            WeakMetaCount = blogs.Count(b => AnalyzeMetaIssues(b.TitleFa, b.MetaDescriptionFa).Count > 0),
            StaleBlogCount = blogs.Count(b => b.IsActive && (b.UpdatedAt ?? b.CreatedAt) < cutoff),
            DraftAiPipelineHint = recentDrafts.Count,
            WeakMetaBlogs = weakMeta,
            RefreshCandidates = refresh,
            RecentInactiveDrafts = recentDrafts,
        };
    }

    public async Task<IReadOnlyList<SeoMetaSuggestionItemDto>> GetMetaAuditAsync(
        int take = 30,
        CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 100);
        var blogs = await blogRepository.Query(b => !b.IsDeleted && b.IsActive)
            .Select(b => new { b.Id, b.Slug, b.TitleFa, b.MetaDescriptionFa })
            .ToListAsync(cancellationToken);

        return blogs
            .Select(b =>
            {
                var issues = AnalyzeMetaIssues(b.TitleFa, b.MetaDescriptionFa);
                return new SeoMetaSuggestionItemDto
                {
                    BlogId = b.Id,
                    Slug = b.Slug,
                    CurrentTitleFa = b.TitleFa,
                    CurrentMetaDescriptionFa = b.MetaDescriptionFa,
                    Issues = issues,
                    Path = $"/fa/blog/{b.Slug}",
                };
            })
            .Where(x => x.Issues.Count > 0)
            .OrderByDescending(x => x.Issues.Count)
            .Take(take)
            .ToList();
    }

    public async Task<IReadOnlyList<SeoInventoryItemDto>> GetRefreshCandidatesAsync(
        int staleDays = 90,
        int take = 20,
        CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 100);
        staleDays = Math.Clamp(staleDays, 7, 730);
        var cutoff = DateTime.UtcNow.AddDays(-staleDays);

        var blogs = await blogRepository.Query(b => !b.IsDeleted && b.IsActive)
            .Select(b => new { b.Id, b.Slug, b.TitleFa, b.MetaDescriptionFa, b.IsActive, b.CreatedAt, b.UpdatedAt })
            .ToListAsync(cancellationToken);

        return blogs
            .Select(b => ToInventoryItem(b.Id, b.Slug, b.TitleFa, b.MetaDescriptionFa, b.IsActive, b.CreatedAt, b.UpdatedAt, "/fa/blog/"))
            .Where(x => (x.UpdatedAt ?? x.CreatedAt) < cutoff)
            .OrderBy(x => x.UpdatedAt ?? x.CreatedAt)
            .Take(take)
            .ToList();
    }

    public async Task<SeoInternalLinkSuggestionDto?> SuggestInternalLinksAsync(
        int blogId,
        int take = 8,
        CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 20);
        var blog = await blogRepository.Query(b => b.Id == blogId && !b.IsDeleted)
            .Select(b => new
            {
                b.Id,
                b.Slug,
                b.TitleFa,
                b.IntroFa,
                b.ContentFa,
                b.ConclusionFa,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (blog is null)
            return null;

        var haystack = $"{blog.TitleFa} {StripHtml(blog.IntroFa)} {StripHtml(blog.ContentFa)} {StripHtml(blog.ConclusionFa)}"
            .ToLowerInvariant();
        var existingLinks = InternalLinkRegex.Matches($"{blog.IntroFa}{blog.ContentFa}{blog.ConclusionFa}").Count;

        var products = await productRepository.Query(p => p.IsActive && !p.IsDeleted)
            .OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt)
            .Take(80)
            .Select(p => new { p.Id, p.Slug, TitleFa = p.Name, Type = "product", PathPrefix = "/fa/products/" })
            .ToListAsync(cancellationToken);

        var categories = await categoryRepository.Query(c => c.IsActive && !c.IsDeleted)
            .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
            .Take(40)
            .Select(c => new { c.Id, c.Slug, TitleFa = c.PersianName ?? c.EnglishName ?? c.Slug, Type = "category", PathPrefix = "/fa/categories/" })
            .ToListAsync(cancellationToken);

        var otherBlogs = await blogRepository.Query(b => b.IsActive && !b.IsDeleted && b.Id != blogId)
            .OrderByDescending(b => b.UpdatedAt ?? b.CreatedAt)
            .Take(40)
            .Select(b => new { b.Id, b.Slug, b.TitleFa, Type = "blog", PathPrefix = "/fa/blog/" })
            .ToListAsync(cancellationToken);

        var candidates = products
            .Select(x => new { x.Id, x.Slug, x.TitleFa, x.Type, x.PathPrefix })
            .Concat(categories.Select(x => new { x.Id, x.Slug, TitleFa = x.TitleFa ?? x.Slug, x.Type, x.PathPrefix }))
            .Concat(otherBlogs.Select(x => new { x.Id, x.Slug, x.TitleFa, x.Type, x.PathPrefix }))
            .ToList();

        var scored = candidates
            .Select(c =>
            {
                var title = (c.TitleFa ?? string.Empty).Trim();
                var slug = (c.Slug ?? string.Empty).Trim();
                var score = 0;
                var reasons = new List<string>();

                foreach (var token in Tokenize(title).Take(6))
                {
                    if (token.Length < 3) continue;
                    if (haystack.Contains(token, StringComparison.Ordinal))
                    {
                        score += 3;
                        reasons.Add($"token:{token}");
                    }
                }

                if (!string.IsNullOrWhiteSpace(slug) && haystack.Contains(slug.Replace('-', ' '), StringComparison.Ordinal))
                {
                    score += 4;
                    reasons.Add("slug-overlap");
                }

                if (c.Type == "category") score += 1;
                if (c.Type == "product") score += 2;

                return new SeoLinkTargetDto
                {
                    Type = c.Type,
                    Id = c.Id,
                    Slug = slug,
                    TitleFa = title,
                    Path = $"{c.PathPrefix}{slug}",
                    Score = score,
                    Reason = reasons.Count == 0 ? "recent-inventory" : string.Join(",", reasons.Distinct().Take(4)),
                };
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.TitleFa)
            .Take(take)
            .ToList();

        if (scored.Count == 0)
        {
            scored = candidates
                .Take(take)
                .Select(c => new SeoLinkTargetDto
                {
                    Type = c.Type,
                    Id = c.Id,
                    Slug = c.Slug,
                    TitleFa = c.TitleFa ?? c.Slug,
                    Path = $"{c.PathPrefix}{c.Slug}",
                    Score = 1,
                    Reason = "fallback-recent",
                })
                .ToList();
        }

        return new SeoInternalLinkSuggestionDto
        {
            BlogId = blog.Id,
            BlogSlug = blog.Slug,
            BlogTitleFa = blog.TitleFa,
            ExistingInternalLinkCount = existingLinks,
            Suggestions = scored,
        };
    }

    private static SeoInventoryItemDto ToInventoryItem(
        int id,
        string slug,
        string titleFa,
        string? metaDescriptionFa,
        bool isActive,
        DateTime? createdAt,
        DateTime? updatedAt,
        string pathPrefix)
    {
        var stamp = updatedAt ?? createdAt;
        return new SeoInventoryItemDto
        {
            Id = id,
            Slug = slug,
            TitleFa = titleFa,
            MetaDescriptionFa = metaDescriptionFa,
            IsActive = isActive,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            AgeDays = stamp.HasValue ? (int)(DateTime.UtcNow - stamp.Value).TotalDays : null,
            Issues = AnalyzeMetaIssues(titleFa, metaDescriptionFa),
            Path = $"{pathPrefix}{slug}",
        };
    }

    private static List<string> AnalyzeMetaIssues(string? titleFa, string? metaDescriptionFa)
    {
        var issues = new List<string>();
        var title = (titleFa ?? string.Empty).Trim();
        var meta = (metaDescriptionFa ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(title))
            issues.Add("missing-title");
        else if (title.Length < 20)
            issues.Add("title-too-short");
        else if (title.Length > 70)
            issues.Add("title-too-long");

        if (string.IsNullOrWhiteSpace(meta))
            issues.Add("missing-meta-description");
        else if (meta.Length < 70)
            issues.Add("meta-too-short");
        else if (meta.Length > 160)
            issues.Add("meta-too-long");

        return issues;
    }

    private static IEnumerable<string> Tokenize(string value) =>
        Regex.Split((value ?? string.Empty).ToLowerInvariant(), @"[^\p{L}\p{N}]+")
            .Where(t => !string.IsNullOrWhiteSpace(t));

    private static string StripHtml(string? value) =>
        HtmlTagRegex.Replace(value ?? string.Empty, " ")
            .Replace("&nbsp;", " ", StringComparison.OrdinalIgnoreCase)
            .Trim();

    private static string TrimTrailingSlash(string value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().TrimEnd('/');
}
