using OnlineShop.Domain.Entities;
using Xunit;

namespace Application.Tests;

public class ThemeAndAnnouncementDomainTests
{
    private static ThemePalette Palette(string? primary = "#000000") => new(
        primary, "#f2f2f2", "#a38a52", "#f7f7f7", "#0d9488", "#c8102e", "#b45309", "#3b82f6",
        "#ffffff", "#f7f7f7", "#e3e3e3", "#000000", "#6b6b6b");

    [Fact]
    public void Theme_create_normalizes_hex_and_requires_name()
    {
        var theme = ThemeSetting.Create("  Crystal ", Palette("#ABCDEF"), currentUserId: 1);

        Assert.Equal("Crystal", theme.Name);
        Assert.Equal("#abcdef", theme.PrimaryColor);
        Assert.Throws<ArgumentException>(() => ThemeSetting.Create(" ", Palette(), 1));
    }

    [Theory]
    [InlineData("red")]
    [InlineData("#fff")]
    [InlineData("#12345g")]
    public void Theme_rejects_invalid_hex(string value)
    {
        Assert.Throws<ArgumentException>(() => ThemeSetting.Create("x", Palette(value), 1));
    }

    [Fact]
    public void Theme_update_keeps_existing_colors_when_input_is_blank()
    {
        var theme = ThemeSetting.Create("x", Palette("#111111"), 1);

        theme.Update("renamed", Palette(primary: null), 2);

        Assert.Equal("renamed", theme.Name);
        Assert.Equal("#111111", theme.PrimaryColor);
    }

    [Fact]
    public void Announcement_is_visible_only_inside_its_window_and_when_active()
    {
        var now = new DateTime(2026, 10, 7, 12, 0, 0, DateTimeKind.Utc);
        var bar = AnnouncementBar.Create(
            "سلام", "Hello", "discounts", "#000000", "#ffffff", 40,
            now.AddDays(-1), now.AddDays(1), 0, 1);

        Assert.True(bar.IsVisibleAt(now));
        Assert.False(bar.IsVisibleAt(now.AddDays(2)));
        Assert.False(bar.IsVisibleAt(now.AddDays(-2)));

        bar.SetActive(false, 1);
        Assert.False(bar.IsVisibleAt(now));
    }

    [Fact]
    public void Announcement_without_dates_is_always_visible()
    {
        var bar = AnnouncementBar.Create("سلام", null, null, null, null, null, null, null, null, 1);

        Assert.True(bar.IsVisibleAt(DateTime.UtcNow));
        Assert.Equal(36, bar.HeightPx);
        Assert.Equal(string.Empty, bar.MessageEn);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(500)]
    public void Announcement_height_is_range_checked(int height)
    {
        Assert.Throws<ArgumentException>(() =>
            AnnouncementBar.Create("سلام", null, null, null, null, height, null, null, null, 1));
    }

    [Fact]
    public void Announcement_rejects_end_before_start_and_bad_colors()
    {
        var now = DateTime.UtcNow;
        Assert.Throws<ArgumentException>(() =>
            AnnouncementBar.Create("سلام", null, null, null, null, null, now, now.AddHours(-1), null, 1));
        Assert.Throws<ArgumentException>(() =>
            AnnouncementBar.Create("سلام", null, null, "blue", null, null, null, null, null, 1));
        Assert.Throws<ArgumentException>(() =>
            AnnouncementBar.Create(" ", null, null, null, null, null, null, null, null, 1));
    }
}
