using System.Text.RegularExpressions;
using Application.Common.Interfaces;
using Application.Dtos;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class BlogContentQualityService(IBlogRepository blogRepository) : IBlogContentQualityService
{
    private static readonly Regex HtmlTagRegex = new("<[^>]+>", RegexOptions.Compiled);
    private static readonly Regex FaqHeadingRegex = new(
        @"<(h2|h3)[^>]*>\s*(سوالات متداول|پرسش‌های متداول|faq|frequently asked questions)\s*</\1>",
        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex InternalLinkRegex = new(
        @"href\s*=\s*[""'][^""']*(/products/|/categories/|/blog/)[^""']*[""']",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public async Task<BlogContentQualityResultDto> ValidateAsync(
        BlogContentQualityRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var result = new BlogContentQualityResultDto();
        var titleFa = request.TitleFa?.Trim() ?? string.Empty;
        var introFa = request.IntroFa?.Trim() ?? string.Empty;
        var contentFa = request.ContentFa?.Trim() ?? string.Empty;
        var conclusionFa = request.ConclusionFa?.Trim() ?? string.Empty;
        var slug = NormalizeSlug(request.Slug, titleFa);

        if (string.IsNullOrWhiteSpace(titleFa))
            result.Errors.Add("TitleFa is required.");

        if (string.IsNullOrWhiteSpace(introFa) || StripHtml(introFa).Length < 80)
            result.Errors.Add("IntroFa must contain at least 80 characters of text.");

        var contentText = StripHtml(contentFa);
        if (contentText.Length < 500)
            result.Errors.Add("ContentFa must contain at least 500 characters of text.");

        if (string.IsNullOrWhiteSpace(conclusionFa) || StripHtml(conclusionFa).Length < 60)
            result.Errors.Add("ConclusionFa must contain at least 60 characters of text.");

        if (!FaqHeadingRegex.IsMatch(contentFa))
            result.Errors.Add("ContentFa must include an FAQ section heading (سوالات متداول / FAQ).");

        var metaFa = request.MetaDescriptionFa?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(metaFa))
            result.Errors.Add("MetaDescriptionFa is required.");
        else if (metaFa.Length is < 70 or > 160)
            result.Errors.Add("MetaDescriptionFa must be between 70 and 160 characters.");

        var excerptFa = request.ExcerptFa?.Trim() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(excerptFa) && excerptFa.Length > 160)
            result.Errors.Add("ExcerptFa must be at most 160 characters.");

        var keywordsFa = request.MetaKeywordsFa?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(keywordsFa))
            result.Warnings.Add("MetaKeywordsFa is empty.");
        else
        {
            var keywordCount = keywordsFa.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Length;
            if (keywordCount is < 3 or > 12)
                result.Warnings.Add("MetaKeywordsFa should contain between 3 and 12 keywords.");
        }

        if (string.IsNullOrWhiteSpace(slug))
            result.Errors.Add("Slug is required.");
        else
        {
            var slugExists = await blogRepository.Query(b =>
                    !b.IsDeleted &&
                    b.Slug == slug &&
                    (!request.ExcludeBlogId.HasValue || b.Id != request.ExcludeBlogId.Value))
                .AnyAsync(cancellationToken);

            if (slugExists)
                result.Errors.Add($"مقاله‌ای با نامک «{slug}» از قبل وجود دارد؛ نامک دیگری انتخاب کنید.");
        }

        var internalLinkCount = InternalLinkRegex.Matches(contentFa + introFa + conclusionFa).Count;
        if (internalLinkCount < 1)
            result.Errors.Add("Content must include at least one internal link to /products/, /categories/, or /blog/.");
        else if (internalLinkCount < 2)
            result.Warnings.Add("Prefer at least two internal links (product/category/blog cluster).");

        var primaryKeyword = request.PrimaryKeyword?.Trim();
        if (!string.IsNullOrWhiteSpace(primaryKeyword))
        {
            var haystack = $"{titleFa} {introFa} {contentText}".ToLowerInvariant();
            var needle = primaryKeyword.ToLowerInvariant();
            var occurrences = CountOccurrences(haystack, needle);
            result.Metrics["primaryKeywordOccurrences"] = occurrences;

            if (occurrences == 0)
                result.Errors.Add($"Primary keyword '{primaryKeyword}' was not found in the article.");
            else if (occurrences > 25)
                result.Warnings.Add("Primary keyword density looks high; avoid keyword stuffing.");
        }

        if (!string.IsNullOrWhiteSpace(request.TitleEn) && StripHtml(request.ContentEn ?? string.Empty).Length < 200)
            result.Warnings.Add("ContentEn looks short for a bilingual article.");

        result.Metrics["introLength"] = StripHtml(introFa).Length;
        result.Metrics["contentLength"] = contentText.Length;
        result.Metrics["conclusionLength"] = StripHtml(conclusionFa).Length;
        result.Metrics["metaDescriptionLength"] = metaFa.Length;
        result.Metrics["internalLinkCount"] = internalLinkCount;
        result.Metrics["slug"] = slug;

        result.IsValid = result.Errors.Count == 0;
        return result;
    }

    private static string StripHtml(string value) =>
        HtmlTagRegex.Replace(value ?? string.Empty, " ")
            .Replace("&nbsp;", " ", StringComparison.OrdinalIgnoreCase)
            .Trim();

    private static string NormalizeSlug(string? slug, string titleFa)
    {
        if (!string.IsNullOrWhiteSpace(slug))
            return slug.Trim().ToLowerInvariant();

        return titleFa.Trim().ToLowerInvariant().Replace(' ', '-');
    }

    private static int CountOccurrences(string haystack, string needle)
    {
        if (string.IsNullOrEmpty(haystack) || string.IsNullOrEmpty(needle))
            return 0;

        var count = 0;
        var index = 0;
        while ((index = haystack.IndexOf(needle, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += needle.Length;
        }

        return count;
    }
}
