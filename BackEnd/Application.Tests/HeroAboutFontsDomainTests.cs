using OnlineShop.Domain.Common;
using OnlineShop.Domain.Entities;
using Xunit;

namespace Application.Tests;

/// <summary>Hero second link, home slider, about sections, text sizes and uploaded fonts.</summary>
public class HeroAboutFontsDomainTests
{
    private static ThemePalette Palette() => new(
        "#000000", "#f2f2f2", "#a38a52", "#f7f7f7", "#0d9488", "#c8102e", "#b45309", "#3b82f6",
        "#ffffff", "#f7f7f7", "#e3e3e3", "#000000", "#6b6b6b");

    // ===== Hero slide =====

    [Fact]
    public void Hero_title_and_description_are_optional()
    {
        var slide = Slide.Create(1, "products", null, null, null);

        Assert.Equal(string.Empty, slide.BannerTitle);
        Assert.Equal(string.Empty, slide.BannerDescription);
    }

    [Fact]
    public void Hero_update_with_empty_title_clears_it_and_null_keeps_it()
    {
        var slide = Slide.Create(1, "products", null, "Title", "Text");

        slide.Update(1, null, null, null, bannerTitle: null, bannerDescription: "");
        Assert.Equal("Title", slide.BannerTitle);
        Assert.Equal(string.Empty, slide.BannerDescription);

        slide.Update(1, null, null, null, bannerTitle: "", bannerDescription: null);
        Assert.Equal(string.Empty, slide.BannerTitle);
    }

    [Fact]
    public void Hero_second_url_needs_both_link_texts()
    {
        Assert.Throws<DomainException>(() => Slide.Create(1, "products", "discounts", null, null, "تخفیف‌ها", null));

        var slide = Slide.Create(1, "products", "discounts", null, null, "تخفیف‌ها", "Offers");
        Assert.Equal("discounts", slide.SecondUrl);
        Assert.Equal("تخفیف‌ها", slide.SecondUrlTextFa);
        Assert.Equal("Offers", slide.SecondUrlTextEn);
    }

    [Fact]
    public void Hero_clearing_second_url_clears_its_texts()
    {
        var slide = Slide.Create(1, "products", "discounts", null, null, "تخفیف‌ها", "Offers");

        slide.Update(1, null, null, secondUrl: "", null, null);

        Assert.Equal(string.Empty, slide.SecondUrl);
        Assert.Equal(string.Empty, slide.SecondUrlTextFa);
        Assert.Equal(string.Empty, slide.SecondUrlTextEn);
    }

    // ===== Home slider =====

    [Fact]
    public void Home_slide_keeps_optional_fields_on_null_update()
    {
        var slide = HomeSlide.Create("products", "عنوان", "Title", 2, 1);

        slide.Update(null, null, "", null, 1);

        Assert.Equal("products", slide.LinkUrl);
        Assert.Equal("عنوان", slide.TitleFa);
        Assert.Equal(string.Empty, slide.TitleEn);
        Assert.Equal(2, slide.DisplayOrder);
    }

    // ===== About sections =====

    [Fact]
    public void About_section_update_merges_and_clears()
    {
        var section = AboutSection.Create(" Hero ", 1,
            new AboutSectionText("e", "e", "t", "t", "b", "b", null, null, null), 1);

        section.Update(new AboutSectionText(null, null, "New", null, "", null, null, null, null), null, 1);

        Assert.Equal("hero", section.SectionKey);
        Assert.Equal("New", section.TitleFa);
        Assert.Equal("t", section.TitleEn);
        Assert.Equal(string.Empty, section.BodyFa);
        Assert.Equal("b", section.BodyEn);
    }

    // ===== Text sizes =====

    [Fact]
    public void Theme_text_sizes_default_to_medium()
    {
        var theme = ThemeSetting.Create("x", Palette(), 1);

        Assert.Equal(ThemeTextSizes.Medium, theme.BodyTextSize);
        Assert.Null(theme.BodyTextSizePx);
        Assert.Equal(ThemeTextSizes.Medium, theme.HeadingTextSize);
    }

    [Fact]
    public void Theme_custom_text_size_needs_pixels_in_range()
    {
        Assert.Throws<DomainException>(() =>
            ThemeSetting.Create("x", Palette(), 1, typography: new ThemeTypography("custom", null, null, null)));
        Assert.Throws<DomainException>(() =>
            ThemeSetting.Create("x", Palette(), 1, typography: new ThemeTypography("custom", 40, null, null)));
        Assert.Throws<DomainException>(() =>
            ThemeSetting.Create("x", Palette(), 1, typography: new ThemeTypography("huge", null, null, null)));

        var theme = ThemeSetting.Create("x", Palette(), 1,
            typography: new ThemeTypography("custom", 18, "large", 99));

        Assert.Equal(ThemeTextSizes.Custom, theme.BodyTextSize);
        Assert.Equal(18, theme.BodyTextSizePx);
        Assert.Equal(ThemeTextSizes.Large, theme.HeadingTextSize);
        Assert.Null(theme.HeadingTextSizePx); // a preset drops any pixel value
    }

    // ===== Uploaded fonts =====

    [Theory]
    [InlineData("custom-7", 7)]
    [InlineData("custom-0", null)]
    [InlineData("custom-x", null)]
    [InlineData("vazirmatn", null)]
    [InlineData(null, null)]
    public void Custom_font_key_parsing(string? key, int? expected)
    {
        Assert.Equal(expected, CustomFont.IdFromKey(key));
    }

    [Fact]
    public void Theme_accepts_custom_font_only_when_it_is_active()
    {
        Assert.Throws<DomainException>(() => ThemeSetting.Create("x", Palette(), 1, "custom-5"));
        Assert.Throws<DomainException>(() =>
            ThemeSetting.Create("x", Palette(), 1, "custom-5", isCustomFont: k => k == "custom-6"));

        var theme = ThemeSetting.Create("x", Palette(), 1, "custom-5", "custom-6", isCustomFont: _ => true);

        Assert.Equal("custom-5", theme.FontFamily);
        Assert.Equal("custom-6", theme.HeadingFontFamily);
    }

    [Fact]
    public void Custom_font_requires_name_and_known_category()
    {
        Assert.Throws<DomainException>(() => CustomFont.Create(" ", " ", "general", 1));
        Assert.Throws<DomainException>(() => CustomFont.Create("نام", null, "script", 1));

        var font = CustomFont.Create(null, "Shabnam", "FORMAL", 1);
        Assert.Equal("Shabnam", font.NameFa);
        Assert.Equal("formal", font.Category);
    }

    [Fact]
    public void Theme_font_weights_default_and_validate()
    {
        var theme = ThemeSetting.Create("x", Palette(), 1);
        Assert.Equal(ThemeFontWeights.DefaultBody, theme.BodyFontWeight);
        Assert.Equal(ThemeFontWeights.DefaultHeading, theme.HeadingFontWeight);

        theme.Update(null, Palette(), 1, typography: new ThemeTypography(null, null, null, null, 300, 800));
        Assert.Equal(300, theme.BodyFontWeight);
        Assert.Equal(800, theme.HeadingFontWeight);

        // null keeps the stored weight
        theme.Update(null, Palette(), 1, typography: new ThemeTypography(null, null, null, null));
        Assert.Equal(300, theme.BodyFontWeight);

        Assert.Throws<DomainException>(() =>
            theme.Update(null, Palette(), 1, typography: new ThemeTypography(null, null, null, null, 450, null)));
        Assert.Throws<DomainException>(() =>
            theme.Update(null, Palette(), 1, typography: new ThemeTypography(null, null, null, null, null, 1000)));
    }

    [Fact]
    public void Hero_title_opacity_defaults_to_full_and_stays_in_range()
    {
        var slide = Slide.Create(1, "products", null, "Title", null);
        Assert.Equal(100, slide.TitleOpacity);

        slide.Update(1, null, null, null, null, null, titleOpacity: 0);
        Assert.Equal(0, slide.TitleOpacity);
        Assert.Equal("Title", slide.BannerTitle);

        slide.Update(1, null, null, null, null, null);
        Assert.Equal(0, slide.TitleOpacity);

        Assert.Throws<DomainException>(() => slide.Update(1, null, null, null, null, null, titleOpacity: 101));
        Assert.Throws<DomainException>(() => Slide.Create(1, "products", null, null, null, titleOpacity: -1));
    }
}
