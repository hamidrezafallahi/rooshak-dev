using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Common;
using OnlineShop.Domain.Entities;
using OnlineShop.Infrastructure.Persistence;
using Xunit;

namespace Application.Tests;

public class FieldLengthValidationTests
{
    // The length check runs before EF opens a connection, so no database is needed.
    private static AppDbContext Context() => new(
        new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=unused;Database=unused")
            .Options);

    [Fact]
    public async Task Save_reports_too_long_fields_by_form_field_name()
    {
        await using var db = Context();
        db.Categories.Add(Category.Create(
            persianName: "نام",
            persianDesc: new string('x', 250),
            englishName: "Name",
            englishDesc: "ok",
            isShowInLanding: false,
            isActive: true,
            parentCategoryId: null,
            currentUserId: 1));

        var ex = await Assert.ThrowsAsync<FieldLengthExceededException>(() => db.SaveChangesAsync());

        var violation = Assert.Single(ex.Violations);
        Assert.Equal("categoryPersianDesc", violation.Field);
        Assert.Equal(200, violation.MaxLength);
        Assert.Equal(250, violation.ActualLength);
    }
}
