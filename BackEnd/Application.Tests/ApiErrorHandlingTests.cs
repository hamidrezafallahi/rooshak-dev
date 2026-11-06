using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Api.Errors;
using Application.Common;
using Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using OnlineShop.Domain.Common;
using OnlineShop.Domain.Entities;
using OnlineShop.Infrastructure.Persistence;
using Xunit;

namespace Application.Tests;

/// <summary>What the client reads when something fails, from the database up to the HTTP response.</summary>
public class ApiErrorHandlingTests
{
    private static IModel Model { get; } = new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql("Host=unused;Database=unused")
                .Options)
        .Model;

    private static PostgresException Postgres(
        string sqlState,
        string message = "database said no",
        string? table = null,
        string? column = null,
        string? constraint = null) =>
        new(message, "ERROR", "ERROR", sqlState, tableName: table, columnName: column, constraintName: constraint);

    private static string ConstraintOf<T>(string property)
    {
        var entity = Model.FindEntityType(typeof(T))!;
        return entity.GetIndexes().First(i => i.Properties.Any(p => p.Name == property)).GetDatabaseName()!;
    }

    [Fact]
    public void Duplicate_value_names_the_record_and_the_field()
    {
        var error = DatabaseErrorTranslator.Translate(
            new DbUpdateException("save failed", Postgres(
                PostgresErrorCodes.UniqueViolation,
                table: Model.FindEntityType(typeof(Product))!.GetTableName(),
                constraint: ConstraintOf<Product>(nameof(Product.Slug)))),
            Model);

        var conflict = Assert.IsType<ConflictException>(error);
        Assert.Equal(ErrorCodes.DuplicateValue, conflict.Code);
        Assert.Contains("محصول", conflict.Message);
        Assert.Contains(FieldLabels.Field("Slug"), conflict.Message);
        Assert.Equal("Slug", Assert.Single(conflict.FieldErrors!).Field);
    }

    [Fact]
    public void Deleting_a_record_in_use_names_what_depends_on_it()
    {
        var entity = Model.FindEntityType(typeof(Product))!;
        var foreignKey = entity.GetForeignKeys().First(fk => fk.PrincipalEntityType.ClrType == typeof(Category));

        var error = DatabaseErrorTranslator.Translate(
            new DbUpdateException("save failed", Postgres(
                PostgresErrorCodes.ForeignKeyViolation,
                message: "update or delete on table \"Categories\" violates foreign key constraint",
                table: entity.GetTableName(),
                constraint: foreignKey.GetConstraintName())),
            Model);

        var conflict = Assert.IsType<ConflictException>(error);
        Assert.Equal(ErrorCodes.ReferenceInUse, conflict.Code);
        Assert.Contains("دسته‌بندی", conflict.Message);
        Assert.Contains("محصول", conflict.Message);
    }

    [Fact]
    public void Missing_reference_names_the_field_to_choose_again()
    {
        var entity = Model.FindEntityType(typeof(Product))!;
        var foreignKey = entity.GetForeignKeys().First(fk => fk.PrincipalEntityType.ClrType == typeof(Category));

        var error = DatabaseErrorTranslator.Translate(
            new DbUpdateException("save failed", Postgres(
                PostgresErrorCodes.ForeignKeyViolation,
                message: "insert or update on table \"Products\" violates foreign key constraint",
                table: entity.GetTableName(),
                constraint: foreignKey.GetConstraintName())),
            Model);

        Assert.Equal(ErrorCodes.ReferenceMissing, error!.Code);
        Assert.Contains("دسته‌بندی", error.Message);
    }

    [Fact]
    public void Missing_required_value_names_the_column_by_its_label()
    {
        var error = DatabaseErrorTranslator.Translate(
            new DbUpdateException("save failed", Postgres(
                PostgresErrorCodes.NotNullViolation, table: "Brands", column: "Name")),
            Model);

        Assert.Equal(ErrorCodes.RequiredValueMissing, error!.Code);
        Assert.Contains("نام", error.Message);
        Assert.Contains("برند", error.Message);
        Assert.DoesNotContain("Brands", error.Message);
    }

    [Theory]
    [InlineData(PostgresErrorCodes.DeadlockDetected, ErrorCodes.DatabaseBusy)]
    [InlineData(PostgresErrorCodes.TooManyConnections, ErrorCodes.DatabaseBusy)]
    [InlineData(PostgresErrorCodes.QueryCanceled, ErrorCodes.DatabaseTimeout)]
    [InlineData(PostgresErrorCodes.DiskFull, ErrorCodes.DatabaseResources)]
    [InlineData(PostgresErrorCodes.UndefinedTable, ErrorCodes.DatabaseSchemaMismatch)]
    [InlineData(PostgresErrorCodes.UndefinedColumn, ErrorCodes.DatabaseSchemaMismatch)]
    [InlineData(PostgresErrorCodes.InvalidPassword, ErrorCodes.DatabaseUnavailable)]
    [InlineData(PostgresErrorCodes.AdminShutdown, ErrorCodes.DatabaseUnavailable)]
    [InlineData(PostgresErrorCodes.NumericValueOutOfRange, ErrorCodes.InvalidData)]
    [InlineData(PostgresErrorCodes.StringDataRightTruncation, ErrorCodes.FieldTooLong)]
    public void Every_database_failure_has_a_reason(string sqlState, string expectedCode)
    {
        var error = DatabaseErrorTranslator.Translate(Postgres(sqlState, message: "raw english from postgres"), Model);

        Assert.Equal(expectedCode, error!.Code);
        Assert.True(ValidationMessages.HasPersianText(error.Message));
        Assert.DoesNotContain("raw english", error.Message);
        Assert.Contains("raw english", error.TechnicalDetail);
    }

    [Fact]
    public void Database_being_down_says_so_without_naming_the_host()
    {
        var error = DatabaseErrorTranslator.Translate(
            new NpgsqlException("Failed to connect to 10.0.0.5:5432", new System.Net.Sockets.SocketException(10061)),
            Model);

        Assert.IsType<DependencyFailureException>(error);
        Assert.Equal(ErrorCodes.DatabaseUnavailable, error!.Code);
        Assert.DoesNotContain("10.0.0.5", error.Message);
    }

    [Fact]
    public void Concurrent_edit_is_explained()
    {
        var error = DatabaseErrorTranslator.Translate(new DbUpdateConcurrencyException("0 rows affected"), Model);

        Assert.Equal(ErrorCodes.ConcurrencyConflict, error!.Code);
    }

    [Fact]
    public void Exceptions_from_elsewhere_are_not_taken_for_database_errors()
    {
        Assert.Null(DatabaseErrorTranslator.Translate(new NullReferenceException(), Model));
    }

    [Theory]
    [InlineData("name", "The Name field is required.", "«نام» الزامی است.")]
    [InlineData("$.price", "The JSON value could not be converted to System.Decimal. Path: $.price | LineNumber: 0 | BytePositionInLine: 12.", "مقدار «قیمت» باید عدد باشد.")]
    [InlineData("id", "The value 'abc' is not valid.", "مقدار «abc» برای «شناسه» معتبر نیست.")]
    [InlineData("title", "The field Title must be a string with a maximum length of '20'.", "«عنوان» حداکثر 20 نویسه می‌تواند باشد.")]
    [InlineData("", "A non-empty request body is required.", "اطلاعات فرم به سرور نرسید؛ بدنهٔ درخواست خالی است.")]
    [InlineData("name", "پیام فارسی خود برنامه", "پیام فارسی خود برنامه")]
    public void Framework_validation_messages_are_rewritten_per_field(string key, string message, string expected)
    {
        Assert.Equal(expected, ValidationMessages.Translate(key, message));
    }

    [Fact]
    public void Field_labels_accept_every_form_the_framework_reports()
    {
        Assert.Equal("نام فارسی", FieldLabels.Field("persianName"));
        Assert.Equal("نام فارسی", FieldLabels.Field("PersianName"));
        Assert.Equal("قیمت", FieldLabels.Field("$.items[0].price"));
        Assert.Equal("قیمت", FieldLabels.Field("command.Price"));
    }

    // ── The whole pipeline ─────────────────────────────────────────────────

    private static async Task<(HttpStatusCode Status, JsonElement Body)> SendAsync(
        RequestDelegate endpoint,
        string environment = "Production",
        string method = "GET")
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = environment });
        builder.WebHost.UseTestServer();
        await using var app = builder.Build();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.Run(endpoint);
        await app.StartAsync();

        var response = await app.GetTestClient().SendAsync(new HttpRequestMessage(new HttpMethod(method), "/api/test"));
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return (response.StatusCode, body);
    }

    [Fact]
    public async Task Unexpected_exception_hides_its_text_from_visitors()
    {
        var (status, body) = await SendAsync(_ => throw new InvalidOperationException("secret internals"));

        Assert.Equal(HttpStatusCode.InternalServerError, status);
        Assert.False(body.GetProperty("isSuccess").GetBoolean());
        Assert.Equal(ErrorCodes.InternalError, body.GetProperty("errorCode").GetString());
        Assert.True(ValidationMessages.HasPersianText(body.GetProperty("error").GetString()));
        Assert.DoesNotContain("secret internals", body.GetRawText());
        Assert.False(string.IsNullOrEmpty(body.GetProperty("traceId").GetString()));
    }

    [Fact]
    public async Task Library_failures_are_not_blamed_on_the_user()
    {
        var (status, body) = await SendAsync(
            _ => throw new ArgumentOutOfRangeException("keyBytes", "IDX10720: the key is too short"));

        Assert.Equal(HttpStatusCode.InternalServerError, status);
        Assert.Equal(ErrorCodes.InternalError, body.GetProperty("errorCode").GetString());
        Assert.DoesNotContain("keyBytes", body.GetRawText());
    }

    [Fact]
    public void An_empty_body_is_reported_once()
    {
        var state = new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary();
        state.AddModelError(string.Empty, "A non-empty request body is required.");
        state.AddModelError("command", "The command field is required.");

        var body = ValidationMessages.FromModelState(state, new DefaultHttpContext());

        Assert.Equal("اطلاعات فرم به سرور نرسید؛ بدنهٔ درخواست خالی است.", Assert.Single(body.FieldErrors!).Message);
    }

    [Fact]
    public async Task Developers_get_the_technical_cause_as_detail()
    {
        var (_, body) = await SendAsync(_ => throw new InvalidOperationException("secret internals"), "Development");

        Assert.Contains("secret internals", body.GetProperty("detail").GetString());
    }

    [Theory]
    [InlineData(StatusCodes.Status401Unauthorized, ErrorCodes.Unauthorized)]
    [InlineData(StatusCodes.Status403Forbidden, ErrorCodes.Forbidden)]
    [InlineData(StatusCodes.Status404NotFound, ErrorCodes.NotFound)]
    [InlineData(StatusCodes.Status405MethodNotAllowed, ErrorCodes.MethodNotAllowed)]
    [InlineData(StatusCodes.Status413PayloadTooLarge, ErrorCodes.PayloadTooLarge)]
    [InlineData(StatusCodes.Status415UnsupportedMediaType, ErrorCodes.UnsupportedMediaType)]
    [InlineData(StatusCodes.Status429TooManyRequests, ErrorCodes.RateLimited)]
    [InlineData(StatusCodes.Status503ServiceUnavailable, ErrorCodes.ServiceUnavailable)]
    public async Task Empty_error_responses_get_a_reason(int statusCode, string expectedCode)
    {
        var (status, body) = await SendAsync(context =>
        {
            context.Response.StatusCode = statusCode;
            return Task.CompletedTask;
        });

        Assert.Equal(statusCode, (int)status);
        Assert.Equal(expectedCode, body.GetProperty("errorCode").GetString());
        Assert.True(ValidationMessages.HasPersianText(body.GetProperty("error").GetString()));
    }

    [Fact]
    public async Task Responses_that_already_explain_themselves_stay_untouched()
    {
        var (status, body) = await SendAsync(async context =>
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { isSuccess = false, error = "دلیل خود کنترلر" });
        });

        Assert.Equal(HttpStatusCode.BadRequest, status);
        Assert.Equal("دلیل خود کنترلر", body.GetProperty("error").GetString());
    }

    [Fact]
    public async Task Domain_exception_keeps_its_message_and_status()
    {
        var (status, body) = await SendAsync(_ => throw new NotFoundException("سفارش موردنظر پیدا نشد."));

        Assert.Equal(HttpStatusCode.NotFound, status);
        Assert.Equal("سفارش موردنظر پیدا نشد.", body.GetProperty("error").GetString());
    }

    [Fact]
    public async Task Field_length_errors_list_each_field_with_its_label()
    {
        var (status, body) = await SendAsync(_ => throw new FieldLengthExceededException(
            [new FieldLengthViolation("categoryPersianDesc", 200, 250, "توضیح فارسی دسته‌بندی")]));

        Assert.Equal(HttpStatusCode.BadRequest, status);
        var field = body.GetProperty("fieldErrors")[0];
        Assert.Equal("categoryPersianDesc", field.GetProperty("field").GetString());
        Assert.Contains("توضیح فارسی دسته‌بندی", field.GetProperty("message").GetString());
    }

    [Fact]
    public void Service_result_tells_sign_in_failures_apart()
    {
        Assert.True(ServiceResult<object>.Unauthorized().IsUnauthorized);
        Assert.False(ServiceResult<object>.Failed("دلیل دیگر").IsUnauthorized);
        Assert.False(ServiceResult<object>.Ok(new object()).IsUnauthorized);
    }
}
