using Application.Common.Interfaces;
using Application.DataTransfer.Excel;
using ClosedXML.Excel;
using OnlineShop.Infrastructure.Services.Excel;
using Xunit;

namespace Application.Tests;

public class ExcelValueTests
{
    [Theory]
    [InlineData("۱۲", 12)]
    [InlineData("12 - ظرف کریستال", 12)]
    [InlineData(" ٤٥ ", 45)]
    public void ToInt_accepts_persian_digits_and_dropdown_values(string input, int expected) =>
        Assert.Equal(expected, ExcelValue.ToInt(input, "Id"));

    [Fact]
    public void ToInt_rejects_text() =>
        Assert.Throws<ExcelRowException>(() => ExcelValue.ToInt("abc", "Id"));

    [Theory]
    [InlineData("بله", true)]
    [InlineData("خیر", false)]
    [InlineData("TRUE", true)]
    [InlineData("0", false)]
    public void ToBool_accepts_persian_and_english(string input, bool expected) =>
        Assert.Equal(expected, ExcelValue.ToBool(input, "IsActive"));

    [Fact]
    public void ToDecimal_handles_persian_separators() =>
        Assert.Equal(1250000.5m, ExcelValue.ToDecimal("۱٬۲۵۰٬۰۰۰٫۵", "Price"));

    [Fact]
    public void Jalali_dates_round_trip()
    {
        var date = ExcelValue.ToDate("1404/07/26", "StartDate");
        Assert.Equal(new DateTime(2025, 10, 18), date);
        Assert.Equal("1404/07/26", ExcelValue.ToJalali(date!.Value, includeTime: false));
        Assert.Equal(new DateTime(2025, 10, 18, 14, 30, 0), ExcelValue.ToDate("۱۴۰۴/۰۷/۲۶ ۱۴:۳۰", "StartDate"));
        Assert.Equal(new DateTime(2025, 10, 18), ExcelValue.ToDate("2025-10-18", "StartDate"));
    }

    [Fact]
    public void Too_long_text_is_written_as_marker_and_read_back_as_blank()
    {
        var cell = ExcelValue.ToCell(new string('x', ExcelValue.MaxCellLength + 1), ExcelValueType.Text);
        Assert.Equal(ExcelValue.TooLongMarker, cell);
        Assert.Null(new ExcelRow(3, new Dictionary<string, object?> { ["Description"] = cell }).GetString("Description"));
    }
}

public class ClosedXmlWorkbookServiceTests
{
    private static readonly ExcelSheetSchema Products = new ExcelSheetBuilder("products", "محصولات")
        .Id().Ref()
        .Text("Name", "نام", required: true)
        .Lookup("CategoryId", "دسته", "categories", required: true)
        .Decimal("Price", "قیمت")
        .Boolean("IsActive", "فعال")
        .ReadOnly("MainImage", "تصویر")
        .Build();

    private static readonly ExcelLookup Categories = new()
    {
        Key = "categories",
        Caption = "دسته‌ها",
        Items = [new ExcelLookupItem(3, "گلدان"), new ExcelLookupItem(4, "میوه‌خوری")],
    };

    private readonly ClosedXmlWorkbookService _service = new();

    [Fact]
    public void Export_then_import_round_trips_values()
    {
        var bytes = _service.Write(new ExcelWorkbookModel
        {
            Title = "test",
            Sheets =
            [
                new ExcelSheetData
                {
                    Schema = Products,
                    Rows =
                    [
                        new ExcelRow(0, new Dictionary<string, object?>
                        {
                            ["Id"] = 7, ["Name"] = "گلدان آبی", ["CategoryId"] = 3,
                            ["Price"] = 1500000m, ["IsActive"] = true, ["MainImage"] = "/a.webp",
                        }),
                    ],
                },
            ],
            Lookups = [Categories],
        });

        var sheet = Assert.Single(_service.Read(new MemoryStream(bytes), [Products]));
        var row = Assert.Single(sheet.Rows);

        Assert.Equal(3, row.RowNumber);
        Assert.Equal(7, row.Id);
        Assert.Equal("گلدان آبی", row.GetString("Name"));
        Assert.Equal("3 - گلدان", row.GetString("CategoryId"));
        Assert.Equal(3, row.GetInt("CategoryId"));
        Assert.Equal(1500000m, row.GetDecimal("Price"));
        Assert.True(row.GetBool("IsActive"));
        Assert.False(row.Has("MainImage")); // read-only columns are never imported
    }

    [Fact]
    public void Template_has_guide_lookup_sheet_and_rtl_headers()
    {
        var bytes = _service.Write(new ExcelWorkbookModel
        {
            Title = "قالب",
            Sheets = [new ExcelSheetData { Schema = Products }],
            Lookups = [Categories],
        });

        using var workbook = new XLWorkbook(new MemoryStream(bytes));
        Assert.Equal(["راهنما", "محصولات", "لیست دسته‌ها"], workbook.Worksheets.Select(w => w.Name));

        var data = workbook.Worksheet("محصولات");
        Assert.True(data.RightToLeft);
        Assert.Equal("نام *", data.Cell(1, 3).GetString());
        Assert.Equal("Name", data.Cell(2, 3).GetString());
        Assert.Equal("4 - میوه‌خوری", workbook.Worksheet("لیست دسته‌ها").Cell(3, 1).GetString());
    }

    [Fact]
    public void Import_matches_columns_by_key_in_any_order_and_ignores_unknown_columns()
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("products");
        ws.Cell(1, 1).Value = "قیمت"; ws.Cell(2, 1).Value = "Price";
        ws.Cell(1, 2).Value = "یادداشت من"; ws.Cell(2, 2).Value = "MyNote";
        ws.Cell(1, 3).Value = "شناسه"; ws.Cell(2, 3).Value = "Id";
        ws.Cell(3, 1).Value = "۲۵۰۰"; ws.Cell(3, 2).Value = "x"; ws.Cell(3, 3).Value = 9;
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        var row = Assert.Single(Assert.Single(_service.Read(new MemoryStream(stream.ToArray()), [Products])).Rows);
        Assert.Equal(9, row.Id);
        Assert.Equal(2500m, row.GetDecimal("Price"));
        Assert.False(row.Values.ContainsKey("MyNote"));
    }

    [Fact]
    public void Import_falls_back_to_caption_headers()
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("محصولات");
        ws.Cell(1, 1).Value = "نام *";
        ws.Cell(2, 1).Value = "میز";
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        var row = Assert.Single(Assert.Single(_service.Read(new MemoryStream(stream.ToArray()), [Products])).Rows);
        Assert.Equal("میز", row.GetString("Name"));
    }

    [Fact]
    public void Unreadable_file_or_missing_sheet_raises_format_error()
    {
        Assert.Throws<ExcelFormatException>(() => _service.Read(new MemoryStream([1, 2, 3]), [Products]));

        using var workbook = new XLWorkbook();
        workbook.Worksheets.Add("something");
        workbook.Worksheets.Add("else");
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        Assert.Throws<ExcelFormatException>(() => _service.Read(new MemoryStream(stream.ToArray()), [Products]));
    }
}

public class ExcelImportContextTests
{
    private static readonly ExcelSheetSchema Parents = new ExcelSheetBuilder("parents", "والد")
        .Id().Ref().Text("Name", "نام", required: true).Build();

    private static readonly ExcelSheetSchema Children = new ExcelSheetBuilder("children", "فرزند")
        .Id().Lookup("ParentId", "والد", "parents", required: true, referencesSheet: "parents").Build();

    [Fact]
    public async Task Children_resolve_refs_of_new_parent_rows_and_failed_rows_are_rolled_back()
    {
        var session = new FakeSession();
        var result = new ExcelImportResult { Entity = "parents" };
        var context = new ExcelImportContext(session,
        [
            Sheet(Parents, Row(3, ("Ref", "p1"), ("Name", "a")), Row(4, ("Ref", "p2"))),
            Sheet(Children, Row(3, ("ParentId", "p1")), Row(4, ("ParentId", "p2")), Row(5, ("ParentId", "12 - x")), Row(6, ("ParentId", "zz"))),
        ], result, CancellationToken.None);

        var nextId = 100;
        await context.ProcessSheetAsync("parents", _ => Task.FromResult<(ExcelRowOutcome, int?)>((ExcelRowOutcome.Created, ++nextId)));

        var resolved = new List<int>();
        await context.ProcessSheetAsync("children", row =>
        {
            resolved.Add(context.RequireId(Children, row, "ParentId"));
            return Task.FromResult<(ExcelRowOutcome, int?)>((ExcelRowOutcome.Created, 1));
        });

        Assert.Equal([101, 12], resolved);
        Assert.Equal(1, result.Sheets[0].Created);   // row 4 lacked the required Name
        Assert.Equal(2, result.Sheets[1].Created);
        Assert.Equal(2, result.Sheets[1].Failed);
        Assert.Collection(result.Errors,
            e => Assert.Equal(("والد", 4), (e.Sheet, e.Row)),
            e => Assert.Equal(("فرزند", 4), (e.Sheet, e.Row)),   // its parent row failed
            e => Assert.Equal(("فرزند", 6), (e.Sheet, e.Row)));  // neither an Id nor a Ref
        Assert.Equal(3, session.RolledBack);
    }

    [Fact]
    public async Task Required_columns_are_enforced_only_for_new_rows()
    {
        var result = new ExcelImportResult { Entity = "parents" };
        var context = new ExcelImportContext(new FakeSession(), [Sheet(Parents, Row(3, ("Id", 5d)))], result, CancellationToken.None);

        await context.ProcessSheetAsync("parents", _ => Task.FromResult<(ExcelRowOutcome, int?)>((ExcelRowOutcome.Updated, 5)));

        Assert.Equal(1, result.Sheets[0].Updated);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ParentsFirst_orders_self_referencing_rows()
    {
        var child = Row(3, ("Ref", "c"), ("ParentId", "b"));
        var middle = Row(4, ("Ref", "b"), ("ParentId", "a"));
        var root = Row(5, ("Ref", "a"));
        var existing = Row(6, ("ParentId", "8"));

        var ordered = ExcelImportContext.ParentsFirst([child, middle, root, existing], "ParentId").ToList();

        Assert.Equal([root, middle, child, existing], ordered);
    }

    private static ExcelSheetData Sheet(ExcelSheetSchema schema, params ExcelRow[] rows) => new() { Schema = schema, Rows = rows.ToList() };

    private static ExcelRow Row(int number, params (string Key, object? Value)[] values) =>
        new(number, values.ToDictionary(v => v.Key, v => v.Value));

    private sealed class FakeSession : IBulkOperationSession
    {
        public int RolledBack { get; private set; }

        public Task<IAppTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task CreateSavepointAsync(string name, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task ReleaseSavepointAsync(string name, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task RollbackToSavepointAsync(string name, CancellationToken cancellationToken = default)
        {
            RolledBack++;
            return Task.CompletedTask;
        }
    }
}

public class ExcelProfileDefinitionTests
{
    public static IEnumerable<object[]> ProfileTypes() =>
        typeof(IExcelEntityProfile).Assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IExcelEntityProfile).IsAssignableFrom(t))
            .Select(t => new object[] { t });

    private static IExcelEntityProfile Create(Type type)
    {
        // Sheet definitions never touch repositories, so nulls are enough here.
        var ctor = type.GetConstructors().Single();
        var args = ctor.GetParameters().Select(p => p.HasDefaultValue ? p.DefaultValue : null).ToArray();
        return (IExcelEntityProfile)ctor.Invoke(args);
    }

    [Theory]
    [MemberData(nameof(ProfileTypes))]
    public void Profile_defines_consistent_sheets(Type type)
    {
        var profile = Create(type);

        Assert.NotEmpty(profile.Sheets);
        Assert.Equal(profile.Sheets.Count, profile.Sheets.Select(s => s.Key).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.All(profile.Sheets, s => Assert.Equal(ExcelSheetSchema.IdColumn, s.Columns[0].Key));

        var sheetKeys = profile.Sheets.Select(s => s.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
        Assert.All(profile.Sheets.SelectMany(s => s.Columns).Where(c => c.ReferencesSheet is not null),
            c => Assert.Contains(c.ReferencesSheet!, sheetKeys));
    }

    [Fact]
    public void Entity_keys_are_unique() =>
        Assert.Equal(ProfileTypes().Count(),
            ProfileTypes().Select(t => Create((Type)t[0]).EntityKey).Distinct(StringComparer.OrdinalIgnoreCase).Count());
}
