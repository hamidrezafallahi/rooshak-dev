using System.Reflection;
using Application.Common;
using OnlineShop.Domain.Common;
using OnlineShop.Domain.Entities;
using Xunit;

namespace Application.Tests;

public class ProductVariantAndThemeFontTests
{
    private static ProductOffers Offer(int id, string? value, decimal price, int inventory, int supplierId = 1, string? name = "بزرگنمایی")
    {
        var offer = ProductOffers.Create(10, supplierId, price, inventory, currentUserId: 1, variantName: value == null ? null : name, variantValue: value);
        typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id), BindingFlags.Public | BindingFlags.Instance)!
            .SetValue(offer, id);
        return offer;
    }

    [Fact]
    public void Variants_are_sorted_by_magnification_and_cheapest_in_stock_offer_wins()
    {
        var offers = new[]
        {
            Offer(1, "3.5x", 30_000_000, 2),
            Offer(2, "2.5x", 20_000_000, 1),
            Offer(3, "3x", 25_000_000, 0),
            Offer(4, "2.5x", 19_000_000, 0, supplierId: 2),   // cheaper but out of stock
            Offer(5, null, 1, 5),                             // no variant: not listed
        };

        var variants = ProductVariantMapper.Build(offers, DateTime.UtcNow);

        Assert.Equal(new[] { "2.5x", "3x", "3.5x" }, variants.Select(v => v.Value));
        Assert.Equal(2, variants[0].OfferId);
        Assert.Equal(20_000_000, variants[0].FinalPrice);
        Assert.False(variants[1].InStock);
        Assert.Equal("بزرگنمایی", variants[2].Name);
    }

    [Fact]
    public void Variant_value_is_required_when_name_is_given_and_display_name_includes_it()
    {
        Assert.Throws<DomainException>(() => ProductOffers.Create(1, 1, 10, 1, 1, variantName: "بزرگنمایی", variantValue: " "));
        Assert.Equal("لوپ C300 - بزرگنمایی 3x", ProductOffers.BuildDisplayName("لوپ C300", "بزرگنمایی", "3x"));
        Assert.Equal("لوپ C300", ProductOffers.BuildDisplayName("لوپ C300", null, null));
    }

    [Fact]
    public void Offer_update_changes_variant_only_when_sent()
    {
        var offer = Offer(1, "3x", 10, 1);

        offer.Update(basePrice: 12, inventory: null, currentUserId: 1);
        Assert.Equal("3x", offer.VariantValue);

        offer.Update(basePrice: null, inventory: null, currentUserId: 1, variantValue: "3.5x");
        Assert.Equal("3.5x", offer.VariantValue);
        Assert.Equal("بزرگنمایی", offer.VariantName);
    }

    private static ThemePalette Palette() => new(
        "#000000", "#f2f2f2", "#a38a52", "#f7f7f7", "#0d9488", "#c8102e", "#b45309", "#3b82f6",
        "#ffffff", "#f7f7f7", "#e3e3e3", "#000000", "#6b6b6b");

    [Fact]
    public void Theme_font_defaults_to_iransans_and_accepts_only_whitelisted_keys()
    {
        var theme = ThemeSetting.Create("x", Palette(), 1);
        Assert.Equal(ThemeFonts.Default, theme.FontFamily);

        theme.Update(null, Palette(), 1, " Lalezar ");
        Assert.Equal("lalezar", theme.FontFamily);

        theme.Update(null, Palette(), 1, fontFamily: null);
        Assert.Equal("lalezar", theme.FontFamily);

        Assert.Throws<DomainException>(() => theme.Update(null, Palette(), 1, "Comic Sans; } body{display:none"));
    }

    [Fact]
    public void Heading_font_is_independent_of_body_font()
    {
        var theme = ThemeSetting.Create("x", Palette(), 1);
        Assert.Equal(ThemeFonts.DefaultHeading, theme.HeadingFontFamily);

        theme.Update(null, Palette(), 1, fontFamily: null, headingFontFamily: "lalezar");
        Assert.Equal("lalezar", theme.HeadingFontFamily);
        Assert.Equal(ThemeFonts.Default, theme.FontFamily);

        Assert.Throws<DomainException>(() => theme.Update(null, Palette(), 1, headingFontFamily: "unknown"));
    }
}
