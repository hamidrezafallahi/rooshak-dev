using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class HeroLinksHomeSlidesAboutFonts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BodyTextSize",
                table: "ThemeSettings",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "medium");

            migrationBuilder.AddColumn<int>(
                name: "BodyTextSizePx",
                table: "ThemeSettings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeadingTextSize",
                table: "ThemeSettings",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "medium");

            migrationBuilder.AddColumn<int>(
                name: "HeadingTextSizePx",
                table: "ThemeSettings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondUrlTextEn",
                table: "Slides",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SecondUrlTextFa",
                table: "Slides",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "AboutSections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SectionKey = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    EyebrowFa = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EyebrowEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TitleFa = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TitleEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BodyFa = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    BodyEn = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    LinkTextFa = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LinkTextEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LinkUrl = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    DeletedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AboutSections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomFonts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NameFa = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Category = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FileUrl = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    DeletedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomFonts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HomeSlides",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ImageUrl = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    MobileImageUrl = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    LinkUrl = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    TitleFa = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    TitleEn = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    DeletedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomeSlides", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "AboutSections",
                columns: new[] { "Id", "BodyEn", "BodyFa", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "DisplayOrder", "EyebrowEn", "EyebrowFa", "IsActive", "IsDeleted", "LinkTextEn", "LinkTextFa", "LinkUrl", "SectionKey", "TitleEn", "TitleFa", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, "Rooshak is an online store for crystal and decorative tableware for hosting, gifts and the wedding trousseau.", "روشاک فروشگاه آنلاین ظروف کریستال و دکوری برای پذیرایی، هدیه و جهیزیه است.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, null, 1, "About us", "درباره ما", true, false, "", "", "", "hero", "About Rooshak", "درباره روشاک", null, null },
                    { 2, "We choose each piece with care, describe every product clearly and work to make buying fragile tableware online simple and safe.", "ظروف را با دقت انتخاب می‌کنیم، مشخصات هر محصول را روشن می‌نویسیم و تلاش می‌کنیم خرید آنلاین ظروف شکستنی ساده و مطمئن باشد.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, null, 2, "", "", true, false, "", "", "", "intro", "What we do", "ما چه می‌کنیم", null, null },
                    { 3, "Crystal is fragile, so every order is packed with protective packaging before it ships.", "ظروف کریستال شکستنی‌اند؛ هر سفارش با بسته‌بندی محافظ آماده و ارسال می‌شود.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, null, 3, "", "", true, false, "", "", "", "packaging", "Packaging and delivery", "بسته‌بندی و ارسال", null, null },
                    { 4, "Get in touch if you need help choosing a serving set, a gift or pieces for a trousseau.", "برای انتخاب ست پذیرایی، هدیه یا جهیزیه می‌توانید با ما در تماس باشید.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, null, 4, "", "", true, false, "Contact us", "تماس با ما", "cooperation", "support", "Help before you buy", "مشاوره پیش از خرید", null, null },
                    { 5, "", "", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, null, 5, "", "", true, false, "", "", "", "quote", "", "", null, null },
                    { 6, "", "", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, null, 6, "", "", true, false, "", "", "", "fact-1", "", "", null, null },
                    { 7, "", "", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, null, 7, "", "", true, false, "", "", "", "fact-2", "", "", null, null },
                    { 8, "", "", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, null, 8, "", "", true, false, "", "", "", "fact-3", "", "", null, null },
                    { 9, "", "", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, null, 9, "", "", true, false, "Contact us", "تماس با ما", "cooperation", "cta", "Questions? Get in touch.", "سؤالی دارید؟ با ما در تماس باشید.", null, null }
                });

            migrationBuilder.UpdateData(
                table: "EntityConfigs",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "ColumnsJson", "EnglishDisplayName", "FormFieldsJson", "PersianDisplayName" },
                values: new object[] { "[{\"Header\":\"\\u0634\\u0646\\u0627\\u0633\\u0647\",\"Accessor\":\"id\",\"Type\":\"number\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u0639\\u06A9\\u0633\",\"Accessor\":\"bannerUrl\",\"Type\":\"image\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u0639\\u0646\\u0648\\u0627\\u0646 \\u0628\\u0646\\u0631\",\"Accessor\":\"bannerTitle\",\"Type\":\"text\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u062A\\u0648\\u0636\\u06CC\\u062D \\u0628\\u0646\\u0631\",\"Accessor\":\"bannerDescription\",\"Type\":\"text\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u0622\\u062F\\u0631\\u0633 \\u0627\\u0648\\u0644\",\"Accessor\":\"firstUrl\",\"Type\":\"text\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u0622\\u062F\\u0631\\u0633 \\u062F\\u0648\\u0645\",\"Accessor\":\"secondUrl\",\"Type\":\"text\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u0645\\u062A\\u0646 \\u0644\\u06CC\\u0646\\u06A9 \\u062F\\u0648\\u0645\",\"Accessor\":\"secondUrlTextFa\",\"Type\":\"text\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u0646\\u0645\\u0627\\u06CC\\u0634 \\u0628\\u0646\\u0631\",\"Accessor\":\"isHero\",\"Type\":\"bool\",\"Sortable\":false,\"Filterable\":false,\"Options\":null}]", "home hero banner", "[{\"Name\":\"bannerUrl\",\"Caption\":\"\\u067E\\u0648\\u0633\\u062A\\u0631 / \\u0639\\u06A9\\u0633 \\u062F\\u0633\\u06A9\\u062A\\u0627\\u067E\",\"Type\":\"file\",\"PlaceHolder\":\"\\u0627\\u0646\\u062A\\u062E\\u0627\\u0628 \\u0628\\u0646\\u0631\",\"Help\":\"\\u0639\\u06A9\\u0633 \\u0646\\u0633\\u062E\\u0647\\u200C\\u06CC \\u062F\\u0633\\u06A9\\u062A\\u0627\\u067E\\u061B \\u067E\\u0648\\u0633\\u062A\\u0631 \\u0648\\u06CC\\u062F\\u06CC\\u0648 \\u0648 \\u062C\\u0627\\u06CC\\u06AF\\u0632\\u06CC\\u0646 \\u0622\\u0646 \\u0647\\u0645 \\u0647\\u0633\\u062A\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":[{\"Rule\":\"required\",\"Condition\":\"true\",\"Message\":\"\\u0627\\u0646\\u062A\\u062E\\u0627\\u0628 \\u0639\\u06A9\\u0633 \\u0627\\u0644\\u0632\\u0627\\u0645\\u06CC \\u0627\\u0633\\u062A\"}]},{\"Name\":\"bannerTitle\",\"Caption\":\"\\u0639\\u0646\\u0648\\u0627\\u0646 \\u0628\\u0646\\u0631 (\\u0627\\u062E\\u062A\\u06CC\\u0627\\u0631\\u06CC)\",\"Type\":\"text\",\"PlaceHolder\":\"\\u0639\\u0646\\u0648\\u0627\\u0646 \\u0628\\u0646\\u0631\",\"Help\":\"\\u0627\\u06AF\\u0631 \\u062E\\u0627\\u0644\\u06CC \\u0628\\u0645\\u0627\\u0646\\u062F\\u060C \\u0639\\u0646\\u0648\\u0627\\u0646\\u06CC \\u0631\\u0648\\u06CC \\u0628\\u0646\\u0631 \\u0646\\u0645\\u0627\\u06CC\\u0634 \\u062F\\u0627\\u062F\\u0647 \\u0646\\u0645\\u06CC\\u200C\\u0634\\u0648\\u062F.\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":[]},{\"Name\":\"bannerDescription\",\"Caption\":\"\\u062A\\u0648\\u0636\\u06CC\\u062D \\u0628\\u0646\\u0631 (\\u0627\\u062E\\u062A\\u06CC\\u0627\\u0631\\u06CC)\",\"Type\":\"text\",\"PlaceHolder\":\"\\u062A\\u0648\\u0636\\u06CC\\u062D\\u0627\\u062A \\u0628\\u0646\\u0631\",\"Help\":\"\\u0627\\u06AF\\u0631 \\u062E\\u0627\\u0644\\u06CC \\u0628\\u0645\\u0627\\u0646\\u062F\\u060C \\u062A\\u0648\\u0636\\u06CC\\u062D\\u06CC \\u0631\\u0648\\u06CC \\u0628\\u0646\\u0631 \\u0646\\u0645\\u0627\\u06CC\\u0634 \\u062F\\u0627\\u062F\\u0647 \\u0646\\u0645\\u06CC\\u200C\\u0634\\u0648\\u062F.\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":[]},{\"Name\":\"firstUrl\",\"Caption\":\"\\u0622\\u062F\\u0631\\u0633 \\u0627\\u0648\\u0644\",\"Type\":\"text\",\"PlaceHolder\":\"\\u0622\\u062F\\u0631\\u0633 \\u0627\\u0648\\u0644\",\"Help\":\"\\u0628\\u0627 \\u06A9\\u0644\\u06CC\\u06A9 \\u0628\\u0631 \\u0631\\u0648\\u06CC \\u0639\\u06A9\\u0633 \\u06CC\\u0627 \\u0648\\u06CC\\u062F\\u06CC\\u0648\\u06CC \\u0628\\u0646\\u0631 \\u0628\\u0647 \\u0686\\u0647 \\u0622\\u062F\\u0631\\u0633\\u06CC \\u0628\\u0631\\u0648\\u062F\\u061F\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":[{\"Rule\":\"required\",\"Condition\":\"true\",\"Message\":\"\\u0622\\u062F\\u0631\\u0633   \\u0627\\u0644\\u0632\\u0627\\u0645\\u06CC \\u0627\\u0633\\u062A\"}]},{\"Name\":\"secondUrl\",\"Caption\":\"\\u0622\\u062F\\u0631\\u0633 \\u062F\\u0648\\u0645 (\\u0627\\u062E\\u062A\\u06CC\\u0627\\u0631\\u06CC)\",\"Type\":\"text\",\"PlaceHolder\":\"\\u0622\\u062F\\u0631\\u0633 \\u0635\\u0641\\u062D\\u0647 \\u062F\\u0648\\u0645\",\"Help\":\"\\u0627\\u06AF\\u0631 \\u067E\\u0631 \\u0634\\u0648\\u062F\\u060C \\u06CC\\u06A9 \\u0644\\u06CC\\u0646\\u06A9 \\u0645\\u062A\\u0646\\u06CC \\u0631\\u0648\\u06CC \\u0628\\u0646\\u0631 \\u0646\\u0645\\u0627\\u06CC\\u0634 \\u062F\\u0627\\u062F\\u0647 \\u0645\\u06CC\\u200C\\u0634\\u0648\\u062F\\u061B \\u0645\\u062A\\u0646 \\u0641\\u0627\\u0631\\u0633\\u06CC \\u0648 \\u0627\\u0646\\u06AF\\u0644\\u06CC\\u0633\\u06CC \\u0622\\u0646 \\u0631\\u0627 \\u067E\\u0627\\u06CC\\u06CC\\u0646\\u200C\\u062A\\u0631 \\u0648\\u0627\\u0631\\u062F \\u06A9\\u0646\\u06CC\\u062F.\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":[]},{\"Name\":\"secondUrlTextFa\",\"Caption\":\"\\u0645\\u062A\\u0646 \\u0644\\u06CC\\u0646\\u06A9 \\u062F\\u0648\\u0645 (\\u0641\\u0627\\u0631\\u0633\\u06CC)\",\"Type\":\"text\",\"PlaceHolder\":\"\\u0645\\u062B\\u0644\\u0627\\u064B: \\u0645\\u0634\\u0627\\u0647\\u062F\\u0647 \\u062A\\u062E\\u0641\\u06CC\\u0641\\u200C\\u0647\\u0627\",\"Help\":\"\\u0628\\u0627 \\u0648\\u062C\\u0648\\u062F \\u0622\\u062F\\u0631\\u0633 \\u062F\\u0648\\u0645 \\u0627\\u0644\\u0632\\u0627\\u0645\\u06CC \\u0627\\u0633\\u062A\\u061B \\u062F\\u0631 \\u0646\\u0633\\u062E\\u0647\\u200C\\u06CC \\u0641\\u0627\\u0631\\u0633\\u06CC \\u0633\\u0627\\u06CC\\u062A \\u0646\\u0645\\u0627\\u06CC\\u0634 \\u062F\\u0627\\u062F\\u0647 \\u0645\\u06CC\\u200C\\u0634\\u0648\\u062F.\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":[]},{\"Name\":\"secondUrlTextEn\",\"Caption\":\"\\u0645\\u062A\\u0646 \\u0644\\u06CC\\u0646\\u06A9 \\u062F\\u0648\\u0645 (\\u0627\\u0646\\u06AF\\u0644\\u06CC\\u0633\\u06CC)\",\"Type\":\"text\",\"PlaceHolder\":\"e.g. See the offers\",\"Help\":\"\\u0628\\u0627 \\u0648\\u062C\\u0648\\u062F \\u0622\\u062F\\u0631\\u0633 \\u062F\\u0648\\u0645 \\u0627\\u0644\\u0632\\u0627\\u0645\\u06CC \\u0627\\u0633\\u062A\\u061B \\u062F\\u0631 \\u0646\\u0633\\u062E\\u0647\\u200C\\u06CC \\u0627\\u0646\\u06AF\\u0644\\u06CC\\u0633\\u06CC \\u0633\\u0627\\u06CC\\u062A \\u0646\\u0645\\u0627\\u06CC\\u0634 \\u062F\\u0627\\u062F\\u0647 \\u0645\\u06CC\\u200C\\u0634\\u0648\\u062F.\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":[]},{\"Name\":\"videoUrl\",\"Caption\":\"\\u0648\\u06CC\\u062F\\u06CC\\u0648\\u06CC \\u062F\\u0633\\u06A9\\u062A\\u0627\\u067E (\\u0627\\u062E\\u062A\\u06CC\\u0627\\u0631\\u06CC)\",\"Type\":\"video\",\"PlaceHolder\":\"\\u0627\\u0646\\u062A\\u062E\\u0627\\u0628 \\u0648\\u06CC\\u062F\\u06CC\\u0648 (mp4 \\u06CC\\u0627 webm)\",\"Help\":\"\\u0648\\u06CC\\u062F\\u06CC\\u0648\\u06CC \\u06A9\\u0648\\u062A\\u0627\\u0647 \\u0628\\u06CC\\u200C\\u0635\\u062F\\u0627\\u060C \\u062D\\u062F\\u0627\\u06A9\\u062B\\u0631 \\u06F3\\u06F0 \\u0645\\u06AF\\u0627\\u0628\\u0627\\u06CC\\u062A\\u060C \\u062A\\u0631\\u062C\\u06CC\\u062D\\u0627\\u064B \\u06F1\\u06F6:\\u06F9 \\u0648 \\u062D\\u062F\\u0648\\u062F \\u06F1\\u06F5 \\u062B\\u0627\\u0646\\u06CC\\u0647. \\u0639\\u06A9\\u0633 \\u0628\\u0646\\u0631 \\u0628\\u0647\\u200C\\u0639\\u0646\\u0648\\u0627\\u0646 \\u067E\\u0648\\u0633\\u062A\\u0631 \\u0648 \\u062C\\u0627\\u06CC\\u06AF\\u0632\\u06CC\\u0646 \\u0646\\u0645\\u0627\\u06CC\\u0634 \\u062F\\u0627\\u062F\\u0647 \\u0645\\u06CC\\u200C\\u0634\\u0648\\u062F.\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":[]},{\"Name\":\"mobileBannerUrl\",\"Caption\":\"\\u067E\\u0648\\u0633\\u062A\\u0631 \\u0645\\u0648\\u0628\\u0627\\u06CC\\u0644 (\\u0627\\u062E\\u062A\\u06CC\\u0627\\u0631\\u06CC)\",\"Type\":\"file\",\"PlaceHolder\":\"\\u0627\\u0646\\u062A\\u062E\\u0627\\u0628 \\u067E\\u0648\\u0633\\u062A\\u0631 \\u0645\\u0648\\u0628\\u0627\\u06CC\\u0644\",\"Help\":\"\\u0639\\u06A9\\u0633 \\u0639\\u0645\\u0648\\u062F\\u06CC \\u0645\\u062E\\u0635\\u0648\\u0635 \\u0635\\u0641\\u062D\\u0647\\u200C\\u0647\\u0627\\u06CC \\u06A9\\u0648\\u0686\\u06A9\\u061B \\u0627\\u06AF\\u0631 \\u062E\\u0627\\u0644\\u06CC \\u0628\\u0627\\u0634\\u062F \\u0627\\u0632 \\u0639\\u06A9\\u0633 \\u062F\\u0633\\u06A9\\u062A\\u0627\\u067E \\u0627\\u0633\\u062A\\u0641\\u0627\\u062F\\u0647 \\u0645\\u06CC\\u200C\\u0634\\u0648\\u062F\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":[]},{\"Name\":\"mobileVideoUrl\",\"Caption\":\"\\u0648\\u06CC\\u062F\\u06CC\\u0648\\u06CC \\u0645\\u0648\\u0628\\u0627\\u06CC\\u0644 (\\u0627\\u062E\\u062A\\u06CC\\u0627\\u0631\\u06CC)\",\"Type\":\"video\",\"PlaceHolder\":\"\\u0627\\u0646\\u062A\\u062E\\u0627\\u0628 \\u0648\\u06CC\\u062F\\u06CC\\u0648 (mp4 \\u06CC\\u0627 webm)\",\"Help\":\"\\u0648\\u06CC\\u062F\\u06CC\\u0648\\u06CC \\u0633\\u0628\\u06A9 \\u0648 \\u0639\\u0645\\u0648\\u062F\\u06CC \\u0628\\u0631\\u0627\\u06CC \\u0645\\u0648\\u0628\\u0627\\u06CC\\u0644\\u060C \\u062D\\u062F\\u0627\\u06A9\\u062B\\u0631 \\u06F3\\u06F0 \\u0645\\u06AF\\u0627\\u0628\\u0627\\u06CC\\u062A\\u061B \\u0627\\u06AF\\u0631 \\u062E\\u0627\\u0644\\u06CC \\u0628\\u0627\\u0634\\u062F \\u0627\\u0632 \\u0648\\u06CC\\u062F\\u06CC\\u0648\\u06CC \\u062F\\u0633\\u06A9\\u062A\\u0627\\u067E \\u0627\\u0633\\u062A\\u0641\\u0627\\u062F\\u0647 \\u0645\\u06CC\\u200C\\u0634\\u0648\\u062F\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":[]},{\"Name\":\"removeMobileBanner\",\"Caption\":\"\\u062D\\u0630\\u0641 \\u067E\\u0648\\u0633\\u062A\\u0631 \\u0645\\u0648\\u0628\\u0627\\u06CC\\u0644 \\u0641\\u0639\\u0644\\u06CC\",\"Type\":\"checkbox\",\"PlaceHolder\":\"\",\"Help\":\"\\u062F\\u0631 \\u0648\\u06CC\\u0631\\u0627\\u06CC\\u0634: \\u0628\\u0631\\u0627\\u06CC \\u062D\\u0630\\u0641 \\u067E\\u0648\\u0633\\u062A\\u0631 \\u0645\\u0648\\u0628\\u0627\\u06CC\\u0644 \\u062B\\u0628\\u062A\\u200C\\u0634\\u062F\\u0647 \\u0641\\u0639\\u0627\\u0644 \\u06A9\\u0646\\u06CC\\u062F.\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":[]},{\"Name\":\"removeMobileVideo\",\"Caption\":\"\\u062D\\u0630\\u0641 \\u0648\\u06CC\\u062F\\u06CC\\u0648\\u06CC \\u0645\\u0648\\u0628\\u0627\\u06CC\\u0644 \\u0641\\u0639\\u0644\\u06CC\",\"Type\":\"checkbox\",\"PlaceHolder\":\"\",\"Help\":\"\\u062F\\u0631 \\u0648\\u06CC\\u0631\\u0627\\u06CC\\u0634: \\u0628\\u0631\\u0627\\u06CC \\u062D\\u0630\\u0641 \\u0648\\u06CC\\u062F\\u06CC\\u0648\\u06CC \\u0645\\u0648\\u0628\\u0627\\u06CC\\u0644 \\u062B\\u0628\\u062A\\u200C\\u0634\\u062F\\u0647 \\u0641\\u0639\\u0627\\u0644 \\u06A9\\u0646\\u06CC\\u062F.\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":[]},{\"Name\":\"removeVideo\",\"Caption\":\"\\u062D\\u0630\\u0641 \\u0648\\u06CC\\u062F\\u06CC\\u0648 \\u0641\\u0639\\u0644\\u06CC\",\"Type\":\"checkbox\",\"PlaceHolder\":\"\",\"Help\":\"\\u062F\\u0631 \\u0648\\u06CC\\u0631\\u0627\\u06CC\\u0634: \\u0628\\u0631\\u0627\\u06CC \\u062D\\u0630\\u0641 \\u0648\\u06CC\\u062F\\u06CC\\u0648\\u06CC \\u062B\\u0628\\u062A\\u200C\\u0634\\u062F\\u0647 \\u0641\\u0639\\u0627\\u0644 \\u06A9\\u0646\\u06CC\\u062F.\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":[]}]", "بنر هیرو صفحه اصلی" });

            migrationBuilder.InsertData(
                table: "EntityConfigs",
                columns: new[] { "Id", "ActionsJson", "ColumnsJson", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "EndPoint", "EnglishDisplayName", "EntityIconBase64", "EntityName", "FormFieldsJson", "IsActive", "PersianDisplayName", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 34, "[\"active\",\"edit\",\"delete\",\"new\"]", "[{\"Header\":\"\\u0634\\u0646\\u0627\\u0633\\u0647\",\"Accessor\":\"id\",\"Type\":\"number\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u0639\\u06A9\\u0633\",\"Accessor\":\"imageUrl\",\"Type\":\"image\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u0639\\u0646\\u0648\\u0627\\u0646\",\"Accessor\":\"titleFa\",\"Type\":\"text\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u0644\\u06CC\\u0646\\u06A9\",\"Accessor\":\"linkUrl\",\"Type\":\"text\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u062A\\u0631\\u062A\\u06CC\\u0628\",\"Accessor\":\"displayOrder\",\"Type\":\"number\",\"Sortable\":false,\"Filterable\":false,\"Options\":null}]", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, null, null, "homeSlides", "Home slider", "<svg xmlns=\"http://www.w3.org/2000/svg\" fill=\"none\" viewBox=\"0 0 24 24\" stroke-width=\"1.5\" stroke=\"currentColor\" class=\"size-6\"><path stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"m2.25 15.75 5.159-5.159a2.25 2.25 0 0 1 3.182 0l5.159 5.159m-1.5-1.5 1.409-1.409a2.25 2.25 0 0 1 3.182 0l2.909 2.909m-18 3.75h16.5a1.5 1.5 0 0 0 1.5-1.5V6a1.5 1.5 0 0 0-1.5-1.5H3.75A1.5 1.5 0 0 0 2.25 6v12a1.5 1.5 0 0 0 1.5 1.5Zm10.5-11.25h.008v.008h-.008V8.25Zm.375 0a.375.375 0 1 1-.75 0 .375.375 0 0 1 .75 0Z\" /></svg>", "homeSlides", "[{\"Name\":\"imageUrl\",\"Caption\":\"\\u0639\\u06A9\\u0633 \\u0627\\u0633\\u0644\\u0627\\u06CC\\u062F\",\"Type\":\"file\",\"PlaceHolder\":\"\\u0627\\u0646\\u062A\\u062E\\u0627\\u0628 \\u0639\\u06A9\\u0633\",\"Help\":\"\\u0639\\u06A9\\u0633 \\u0627\\u0641\\u0642\\u06CC\\u060C \\u062A\\u0631\\u062C\\u06CC\\u062D\\u0627\\u064B \\u06F1\\u06F6:\\u06F7\\u061B \\u0641\\u0642\\u0637 \\u062F\\u0631 \\u0627\\u0633\\u0644\\u0627\\u06CC\\u062F\\u0631 \\u0645\\u06CC\\u0627\\u0646\\u06CC \\u0635\\u0641\\u062D\\u0647\\u200C\\u06CC \\u0627\\u0635\\u0644\\u06CC \\u0646\\u0645\\u0627\\u06CC\\u0634 \\u062F\\u0627\\u062F\\u0647 \\u0645\\u06CC\\u200C\\u0634\\u0648\\u062F (\\u0646\\u0647 \\u062F\\u0631 \\u0628\\u0646\\u0631 \\u0647\\u06CC\\u0631\\u0648).\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":[{\"Rule\":\"required\",\"Condition\":\"true\",\"Message\":\"\\u0627\\u0646\\u062A\\u062E\\u0627\\u0628 \\u0639\\u06A9\\u0633 \\u0627\\u0644\\u0632\\u0627\\u0645\\u06CC \\u0627\\u0633\\u062A\"}]},{\"Name\":\"mobileImageUrl\",\"Caption\":\"\\u0639\\u06A9\\u0633 \\u0645\\u0648\\u0628\\u0627\\u06CC\\u0644 (\\u0627\\u062E\\u062A\\u06CC\\u0627\\u0631\\u06CC)\",\"Type\":\"file\",\"PlaceHolder\":\"\\u0627\\u0646\\u062A\\u062E\\u0627\\u0628 \\u0639\\u06A9\\u0633 \\u0645\\u0648\\u0628\\u0627\\u06CC\\u0644\",\"Help\":\"\\u0627\\u06AF\\u0631 \\u062E\\u0627\\u0644\\u06CC \\u0628\\u0627\\u0634\\u062F \\u0647\\u0645\\u0627\\u0646 \\u0639\\u06A9\\u0633 \\u0627\\u0635\\u0644\\u06CC \\u0627\\u0633\\u062A\\u0641\\u0627\\u062F\\u0647 \\u0645\\u06CC\\u200C\\u0634\\u0648\\u062F.\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":null},{\"Name\":\"removeMobileImage\",\"Caption\":\"\\u062D\\u0630\\u0641 \\u0639\\u06A9\\u0633 \\u0645\\u0648\\u0628\\u0627\\u06CC\\u0644 \\u0641\\u0639\\u0644\\u06CC\",\"Type\":\"checkbox\",\"PlaceHolder\":null,\"Help\":\"\\u062F\\u0631 \\u0648\\u06CC\\u0631\\u0627\\u06CC\\u0634: \\u0628\\u0631\\u0627\\u06CC \\u062D\\u0630\\u0641 \\u0639\\u06A9\\u0633 \\u0645\\u0648\\u0628\\u0627\\u06CC\\u0644 \\u062B\\u0628\\u062A\\u200C\\u0634\\u062F\\u0647 \\u0641\\u0639\\u0627\\u0644 \\u06A9\\u0646\\u06CC\\u062F.\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":null},{\"Name\":\"linkUrl\",\"Caption\":\"\\u0622\\u062F\\u0631\\u0633 \\u0644\\u06CC\\u0646\\u06A9 (\\u0627\\u062E\\u062A\\u06CC\\u0627\\u0631\\u06CC)\",\"Type\":\"text\",\"PlaceHolder\":\"\\u0645\\u062B\\u0644\\u0627\\u064B products\",\"Help\":\"\\u0628\\u0627 \\u06A9\\u0644\\u06CC\\u06A9 \\u0631\\u0648\\u06CC \\u0627\\u0633\\u0644\\u0627\\u06CC\\u062F \\u0628\\u0647 \\u0627\\u06CC\\u0646 \\u0635\\u0641\\u062D\\u0647 \\u0645\\u06CC\\u200C\\u0631\\u0648\\u062F\\u061B \\u062E\\u0627\\u0644\\u06CC = \\u0628\\u062F\\u0648\\u0646 \\u0644\\u06CC\\u0646\\u06A9.\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":null},{\"Name\":\"titleFa\",\"Caption\":\"\\u0639\\u0646\\u0648\\u0627\\u0646 \\u0641\\u0627\\u0631\\u0633\\u06CC (\\u0627\\u062E\\u062A\\u06CC\\u0627\\u0631\\u06CC)\",\"Type\":\"text\",\"PlaceHolder\":null,\"Help\":\"\\u0645\\u062A\\u0646 \\u062C\\u0627\\u06CC\\u06AF\\u0632\\u06CC\\u0646 \\u0639\\u06A9\\u0633 \\u0628\\u0631\\u0627\\u06CC \\u0645\\u0648\\u062A\\u0648\\u0631\\u0647\\u0627\\u06CC \\u062C\\u0633\\u062A\\u062C\\u0648 \\u0648 \\u0635\\u0641\\u062D\\u0647\\u200C\\u062E\\u0648\\u0627\\u0646.\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":null},{\"Name\":\"titleEn\",\"Caption\":\"\\u0639\\u0646\\u0648\\u0627\\u0646 \\u0627\\u0646\\u06AF\\u0644\\u06CC\\u0633\\u06CC (\\u0627\\u062E\\u062A\\u06CC\\u0627\\u0631\\u06CC)\",\"Type\":\"text\",\"PlaceHolder\":null,\"Help\":null,\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":null},{\"Name\":\"displayOrder\",\"Caption\":\"\\u062A\\u0631\\u062A\\u06CC\\u0628 \\u0646\\u0645\\u0627\\u06CC\\u0634\",\"Type\":\"number\",\"PlaceHolder\":\"0\",\"Help\":\"\\u0639\\u062F\\u062F \\u06A9\\u0648\\u0686\\u06A9\\u200C\\u062A\\u0631 \\u0632\\u0648\\u062F\\u062A\\u0631 \\u0646\\u0645\\u0627\\u06CC\\u0634 \\u062F\\u0627\\u062F\\u0647 \\u0645\\u06CC\\u200C\\u0634\\u0648\\u062F.\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":null}]", true, "اسلایدر صفحه اصلی", null, null },
                    { 35, "[\"active\",\"edit\"]", "[{\"Header\":\"\\u0628\\u062E\\u0634\",\"Accessor\":\"sectionKey\",\"Type\":\"text\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u0628\\u0631\\u0686\\u0633\\u0628\",\"Accessor\":\"eyebrowFa\",\"Type\":\"text\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u062A\\u06CC\\u062A\\u0631\",\"Accessor\":\"titleFa\",\"Type\":\"text\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u0645\\u062A\\u0646\",\"Accessor\":\"bodyFa\",\"Type\":\"text\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u062A\\u0631\\u062A\\u06CC\\u0628\",\"Accessor\":\"displayOrder\",\"Type\":\"number\",\"Sortable\":false,\"Filterable\":false,\"Options\":null}]", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, null, null, "aboutSections", "About us", "<svg xmlns=\"http://www.w3.org/2000/svg\" fill=\"none\" viewBox=\"0 0 24 24\" stroke-width=\"1.5\" stroke=\"currentColor\" class=\"size-6\"><path stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"M12 6.042A8.967 8.967 0 0 0 6 3.75c-1.052 0-2.062.18-3 .512v14.25A8.987 8.987 0 0 1 6 18c2.305 0 4.408.867 6 2.292m0-14.25a8.966 8.966 0 0 1 6-2.292c1.052 0 2.062.18 3 .512v14.25A8.987 8.987 0 0 0 18 18a8.967 8.967 0 0 0-6 2.292m0-14.25v14.25\" /></svg>", "aboutSections", "[{\"Name\":\"eyebrowFa\",\"Caption\":\"\\u0628\\u0631\\u0686\\u0633\\u0628 \\u0628\\u0627\\u0644\\u0627\\u06CC \\u062A\\u06CC\\u062A\\u0631 (\\u0641\\u0627\\u0631\\u0633\\u06CC)\",\"Type\":\"text\",\"PlaceHolder\":null,\"Help\":\"\\u062E\\u0627\\u0644\\u06CC = \\u0646\\u0645\\u0627\\u06CC\\u0634 \\u062F\\u0627\\u062F\\u0647 \\u0646\\u0645\\u06CC\\u200C\\u0634\\u0648\\u062F.\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":null},{\"Name\":\"eyebrowEn\",\"Caption\":\"\\u0628\\u0631\\u0686\\u0633\\u0628 \\u0628\\u0627\\u0644\\u0627\\u06CC \\u062A\\u06CC\\u062A\\u0631 (\\u0627\\u0646\\u06AF\\u0644\\u06CC\\u0633\\u06CC)\",\"Type\":\"text\",\"PlaceHolder\":null,\"Help\":null,\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":null},{\"Name\":\"titleFa\",\"Caption\":\"\\u062A\\u06CC\\u062A\\u0631 (\\u0641\\u0627\\u0631\\u0633\\u06CC)\",\"Type\":\"text\",\"PlaceHolder\":null,\"Help\":\"\\u062F\\u0631 \\u0628\\u062E\\u0634\\u200C\\u0647\\u0627\\u06CC \\u00ABfact\\u00BB \\u0639\\u062F\\u062F \\u06CC\\u0627 \\u0645\\u0642\\u062F\\u0627\\u0631 \\u0628\\u0632\\u0631\\u06AF\\u061B \\u062F\\u0631 \\u00ABquote\\u00BB \\u0646\\u0627\\u0645 \\u06AF\\u0648\\u06CC\\u0646\\u062F\\u0647.\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":null},{\"Name\":\"titleEn\",\"Caption\":\"\\u062A\\u06CC\\u062A\\u0631 (\\u0627\\u0646\\u06AF\\u0644\\u06CC\\u0633\\u06CC)\",\"Type\":\"text\",\"PlaceHolder\":null,\"Help\":null,\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":null},{\"Name\":\"bodyFa\",\"Caption\":\"\\u0645\\u062A\\u0646 (\\u0641\\u0627\\u0631\\u0633\\u06CC)\",\"Type\":\"textarea\",\"PlaceHolder\":null,\"Help\":\"\\u062F\\u0631 \\u0628\\u062E\\u0634 \\u00ABquote\\u00BB \\u0645\\u062A\\u0646 \\u0646\\u0642\\u0644\\u200C\\u0642\\u0648\\u0644\\u061B \\u062F\\u0631 \\u00ABfact\\u00BB \\u062A\\u0648\\u0636\\u06CC\\u062D \\u0632\\u06CC\\u0631 \\u0639\\u062F\\u062F.\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":null},{\"Name\":\"bodyEn\",\"Caption\":\"\\u0645\\u062A\\u0646 (\\u0627\\u0646\\u06AF\\u0644\\u06CC\\u0633\\u06CC)\",\"Type\":\"textarea\",\"PlaceHolder\":null,\"Help\":null,\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":null},{\"Name\":\"linkTextFa\",\"Caption\":\"\\u0645\\u062A\\u0646 \\u0644\\u06CC\\u0646\\u06A9 (\\u0641\\u0627\\u0631\\u0633\\u06CC)\",\"Type\":\"text\",\"PlaceHolder\":null,\"Help\":\"\\u0627\\u062E\\u062A\\u06CC\\u0627\\u0631\\u06CC\\u061B \\u0641\\u0642\\u0637 \\u0628\\u062E\\u0634\\u200C\\u0647\\u0627\\u06CC\\u06CC \\u06A9\\u0647 \\u0644\\u06CC\\u0646\\u06A9 \\u062F\\u0627\\u0631\\u0646\\u062F.\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":null},{\"Name\":\"linkTextEn\",\"Caption\":\"\\u0645\\u062A\\u0646 \\u0644\\u06CC\\u0646\\u06A9 (\\u0627\\u0646\\u06AF\\u0644\\u06CC\\u0633\\u06CC)\",\"Type\":\"text\",\"PlaceHolder\":null,\"Help\":null,\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":null},{\"Name\":\"linkUrl\",\"Caption\":\"\\u0622\\u062F\\u0631\\u0633 \\u0644\\u06CC\\u0646\\u06A9\",\"Type\":\"text\",\"PlaceHolder\":\"\\u0645\\u062B\\u0644\\u0627\\u064B cooperation\",\"Help\":null,\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":null},{\"Name\":\"displayOrder\",\"Caption\":\"\\u062A\\u0631\\u062A\\u06CC\\u0628\",\"Type\":\"number\",\"PlaceHolder\":null,\"Help\":null,\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":null}]", true, "درباره ما", null, null },
                    { 36, "[\"active\",\"edit\",\"delete\"]", "[{\"Header\":\"\\u0634\\u0646\\u0627\\u0633\\u0647\",\"Accessor\":\"id\",\"Type\":\"number\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u0646\\u0627\\u0645 \\u0641\\u0627\\u0631\\u0633\\u06CC\",\"Accessor\":\"nameFa\",\"Type\":\"text\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u0646\\u0627\\u0645 \\u0627\\u0646\\u06AF\\u0644\\u06CC\\u0633\\u06CC\",\"Accessor\":\"nameEn\",\"Type\":\"text\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u062F\\u0633\\u062A\\u0647\",\"Accessor\":\"category\",\"Type\":\"text\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u06A9\\u0644\\u06CC\\u062F\",\"Accessor\":\"key\",\"Type\":\"text\",\"Sortable\":false,\"Filterable\":false,\"Options\":null}]", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, null, null, "customFonts", "Custom fonts", "<svg xmlns=\"http://www.w3.org/2000/svg\" fill=\"none\" viewBox=\"0 0 24 24\" stroke-width=\"1.5\" stroke=\"currentColor\" class=\"size-6\"><path stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"M3 19.5 8.25 4.5h1.5L15 19.5M5.1 14.25h7.8M15.75 19.5l2.625-7.5h.75L21.75 19.5m-5.25-2.25h4.5\" /></svg>", "customFonts", "[{\"Name\":\"nameFa\",\"Caption\":\"\\u0646\\u0627\\u0645 \\u0641\\u0627\\u0631\\u0633\\u06CC\",\"Type\":\"text\",\"PlaceHolder\":null,\"Help\":null,\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":null},{\"Name\":\"nameEn\",\"Caption\":\"\\u0646\\u0627\\u0645 \\u0627\\u0646\\u06AF\\u0644\\u06CC\\u0633\\u06CC\",\"Type\":\"text\",\"PlaceHolder\":null,\"Help\":null,\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":null},{\"Name\":\"category\",\"Caption\":\"\\u062F\\u0633\\u062A\\u0647 (general / formal / fancy)\",\"Type\":\"text\",\"PlaceHolder\":null,\"Help\":\"general = \\u0639\\u0645\\u0648\\u0645\\u06CC\\u060C formal = \\u0631\\u0633\\u0645\\u06CC\\u060C fancy = \\u0641\\u0627\\u0646\\u062A\\u0632\\u06CC\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":null}]", true, "فونت‌های آپلودی", null, null }
                });

            migrationBuilder.UpdateData(
                table: "ThemeSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BodyTextSize", "BodyTextSizePx", "HeadingTextSize", "HeadingTextSizePx" },
                values: new object[] { "medium", null, "medium", null });

            migrationBuilder.CreateIndex(
                name: "IX_AboutSections_SectionKey",
                table: "AboutSections",
                column: "SectionKey",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_HomeSlides_IsActive_DisplayOrder",
                table: "HomeSlides",
                columns: new[] { "IsActive", "DisplayOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AboutSections");

            migrationBuilder.DropTable(
                name: "CustomFonts");

            migrationBuilder.DropTable(
                name: "HomeSlides");

            migrationBuilder.DeleteData(
                table: "EntityConfigs",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "EntityConfigs",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "EntityConfigs",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DropColumn(
                name: "BodyTextSize",
                table: "ThemeSettings");

            migrationBuilder.DropColumn(
                name: "BodyTextSizePx",
                table: "ThemeSettings");

            migrationBuilder.DropColumn(
                name: "HeadingTextSize",
                table: "ThemeSettings");

            migrationBuilder.DropColumn(
                name: "HeadingTextSizePx",
                table: "ThemeSettings");

            migrationBuilder.DropColumn(
                name: "SecondUrlTextEn",
                table: "Slides");

            migrationBuilder.DropColumn(
                name: "SecondUrlTextFa",
                table: "Slides");

            migrationBuilder.UpdateData(
                table: "EntityConfigs",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "ColumnsJson", "EnglishDisplayName", "FormFieldsJson", "PersianDisplayName" },
                values: new object[] { "[{\"Header\":\"\\u0634\\u0646\\u0627\\u0633\\u0647\",\"Accessor\":\"id\",\"Type\":\"number\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u0639\\u06A9\\u0633\",\"Accessor\":\"banner\",\"Type\":\"image\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u0639\\u0646\\u0648\\u0627\\u0646 \\u0628\\u0646\\u0631\",\"Accessor\":\"bannerTitle\",\"Type\":\"text\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u062A\\u0648\\u0636\\u06CC\\u062D \\u0628\\u0646\\u0631\",\"Accessor\":\"bannerDescription\",\"Type\":\"text\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u0622\\u062F\\u0631\\u0633 \\u0627\\u0648\\u0644\",\"Accessor\":\"firstUrl\",\"Type\":\"text\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u0622\\u062F\\u0631\\u0633 \\u062F\\u0648\\u0645\",\"Accessor\":\"secondUrl\",\"Type\":\"text\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u0648\\u06CC\\u062F\\u06CC\\u0648 \\u062F\\u0633\\u06A9\\u062A\\u0627\\u067E\",\"Accessor\":\"videoUrl\",\"Type\":\"text\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u0648\\u06CC\\u062F\\u06CC\\u0648 \\u0645\\u0648\\u0628\\u0627\\u06CC\\u0644\",\"Accessor\":\"mobileVideoUrl\",\"Type\":\"text\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u0646\\u0645\\u0627\\u06CC\\u0634 \\u0628\\u0646\\u0631\",\"Accessor\":\"isHero\",\"Type\":\"bool\",\"Sortable\":false,\"Filterable\":false,\"Options\":null}]", "landing slide", "[{\"Name\":\"bannerUrl\",\"Caption\":\"\\u067E\\u0648\\u0633\\u062A\\u0631 / \\u0639\\u06A9\\u0633 \\u062F\\u0633\\u06A9\\u062A\\u0627\\u067E\",\"Type\":\"file\",\"PlaceHolder\":\"\\u0627\\u0646\\u062A\\u062E\\u0627\\u0628 \\u0628\\u0646\\u0631\",\"Help\":\"\\u0639\\u06A9\\u0633 \\u0646\\u0633\\u062E\\u0647\\u200C\\u06CC \\u062F\\u0633\\u06A9\\u062A\\u0627\\u067E\\u061B \\u067E\\u0648\\u0633\\u062A\\u0631 \\u0648\\u06CC\\u062F\\u06CC\\u0648 \\u0648 \\u062C\\u0627\\u06CC\\u06AF\\u0632\\u06CC\\u0646 \\u0622\\u0646 \\u0647\\u0645 \\u0647\\u0633\\u062A\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":[{\"Rule\":\"required\",\"Condition\":\"true\",\"Message\":\"\\u0627\\u0646\\u062A\\u062E\\u0627\\u0628 \\u0639\\u06A9\\u0633 \\u0627\\u0644\\u0632\\u0627\\u0645\\u06CC \\u0627\\u0633\\u062A\"}]},{\"Name\":\"bannerTitle\",\"Caption\":\"\\u0639\\u0646\\u0648\\u0627\\u0646 \\u0628\\u0646\\u0631\",\"Type\":\"text\",\"PlaceHolder\":\"\\u0639\\u0646\\u0648\\u0627\\u0646 \\u0628\\u0646\\u0631\",\"Help\":\"\\u062F\\u0631 \\u0628\\u0646\\u0631 \\u0686\\u0647 \\u0639\\u0646\\u0648\\u0627\\u0646\\u06CC \\u0646\\u0645\\u0627\\u06CC\\u0634 \\u062F\\u0627\\u062F\\u0647 \\u0634\\u0648\\u062F \\u061F \\u061F\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":[{\"Rule\":\"required\",\"Condition\":\"true\",\"Message\":\"\\u0639\\u0646\\u0648\\u0627\\u0646 \\u0628\\u0646\\u0631 \\u0627\\u0644\\u0632\\u0627\\u0645\\u06CC \\u0627\\u0633\\u062A\"}]},{\"Name\":\"bannerDescription\",\"Caption\":\"\\u062A\\u0648\\u0636\\u06CC\\u062D \\u0628\\u0646\\u0631\",\"Type\":\"text\",\"PlaceHolder\":\"\\u062A\\u0648\\u0636\\u06CC\\u062D\\u0627\\u062A \\u0628\\u0646\\u0631\",\"Help\":\"\\u062F\\u0631 \\u0628\\u0646\\u0631 \\u0686\\u0647 \\u062A\\u0648\\u0636\\u06CC\\u062D\\u0627\\u062A\\u06CC \\u0646\\u0645\\u0627\\u06CC\\u0634 \\u062F\\u0627\\u062F\\u0647 \\u0634\\u0648\\u062F \\u061F\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":[{\"Rule\":\"required\",\"Condition\":\"true\",\"Message\":\"\\u062A\\u0648\\u0636\\u06CC\\u062D\\u0627\\u062A \\u0628\\u0646\\u0631 \\u0627\\u0644\\u0632\\u0627\\u0645\\u06CC \\u0627\\u0633\\u062A\"}]},{\"Name\":\"firstUrl\",\"Caption\":\"\\u0622\\u062F\\u0631\\u0633 \\u0627\\u0648\\u0644\",\"Type\":\"text\",\"PlaceHolder\":\"\\u0622\\u062F\\u0631\\u0633 \\u0627\\u0648\\u0644\",\"Help\":\"\\u0628\\u0627 \\u06A9\\u0644\\u06CC\\u06A9 \\u0628\\u0631 \\u0631\\u0648\\u06CC \\u0639\\u06A9\\u0633 \\u0628\\u0647 \\u0686\\u0647 \\u0622\\u062F\\u0631\\u0633\\u06CC \\u0628\\u0631\\u0648\\u062F \\u061F\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":[{\"Rule\":\"required\",\"Condition\":\"true\",\"Message\":\"\\u0622\\u062F\\u0631\\u0633   \\u0627\\u0644\\u0632\\u0627\\u0645\\u06CC \\u0627\\u0633\\u062A\"}]},{\"Name\":\"secondUrl\",\"Caption\":\"\\u0622\\u062F\\u0631\\u0633 \\u062F\\u0648\\u0645\",\"Type\":\"text\",\"PlaceHolder\":\"\\u0622\\u062F\\u0631\\u0633 \\u0635\\u0641\\u062D\\u0647 \\u062F\\u0648\\u0645\",\"Help\":\"\\u0628\\u0627 \\u06A9\\u0644\\u06CC\\u06A9 \\u0628\\u0631 \\u0631\\u0648\\u06CC \\u062F\\u06A9\\u0645\\u0647 \\u062F\\u0648\\u0645 \\u0628\\u0647 \\u0686\\u0647 \\u0622\\u062F\\u0631\\u0633\\u06CC \\u0628\\u0631\\u0648\\u062F \\u061F (\\u0627\\u062E\\u062A\\u06CC\\u0627\\u0631\\u06CC)\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":[]},{\"Name\":\"videoUrl\",\"Caption\":\"\\u0648\\u06CC\\u062F\\u06CC\\u0648\\u06CC \\u062F\\u0633\\u06A9\\u062A\\u0627\\u067E (\\u0627\\u062E\\u062A\\u06CC\\u0627\\u0631\\u06CC)\",\"Type\":\"video\",\"PlaceHolder\":\"\\u0627\\u0646\\u062A\\u062E\\u0627\\u0628 \\u0648\\u06CC\\u062F\\u06CC\\u0648 (mp4 \\u06CC\\u0627 webm)\",\"Help\":\"\\u0648\\u06CC\\u062F\\u06CC\\u0648\\u06CC \\u06A9\\u0648\\u062A\\u0627\\u0647 \\u0628\\u06CC\\u200C\\u0635\\u062F\\u0627\\u060C \\u062D\\u062F\\u0627\\u06A9\\u062B\\u0631 \\u06F3\\u06F0 \\u0645\\u06AF\\u0627\\u0628\\u0627\\u06CC\\u062A\\u060C \\u062A\\u0631\\u062C\\u06CC\\u062D\\u0627\\u064B \\u06F1\\u06F6:\\u06F9 \\u0648 \\u062D\\u062F\\u0648\\u062F \\u06F1\\u06F5 \\u062B\\u0627\\u0646\\u06CC\\u0647. \\u0639\\u06A9\\u0633 \\u0628\\u0646\\u0631 \\u0628\\u0647\\u200C\\u0639\\u0646\\u0648\\u0627\\u0646 \\u067E\\u0648\\u0633\\u062A\\u0631 \\u0648 \\u062C\\u0627\\u06CC\\u06AF\\u0632\\u06CC\\u0646 \\u0646\\u0645\\u0627\\u06CC\\u0634 \\u062F\\u0627\\u062F\\u0647 \\u0645\\u06CC\\u200C\\u0634\\u0648\\u062F.\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":[]},{\"Name\":\"mobileBannerUrl\",\"Caption\":\"\\u067E\\u0648\\u0633\\u062A\\u0631 \\u0645\\u0648\\u0628\\u0627\\u06CC\\u0644 (\\u0627\\u062E\\u062A\\u06CC\\u0627\\u0631\\u06CC)\",\"Type\":\"file\",\"PlaceHolder\":\"\\u0627\\u0646\\u062A\\u062E\\u0627\\u0628 \\u067E\\u0648\\u0633\\u062A\\u0631 \\u0645\\u0648\\u0628\\u0627\\u06CC\\u0644\",\"Help\":\"\\u0639\\u06A9\\u0633 \\u0639\\u0645\\u0648\\u062F\\u06CC \\u0645\\u062E\\u0635\\u0648\\u0635 \\u0635\\u0641\\u062D\\u0647\\u200C\\u0647\\u0627\\u06CC \\u06A9\\u0648\\u0686\\u06A9\\u061B \\u0627\\u06AF\\u0631 \\u062E\\u0627\\u0644\\u06CC \\u0628\\u0627\\u0634\\u062F \\u0627\\u0632 \\u0639\\u06A9\\u0633 \\u062F\\u0633\\u06A9\\u062A\\u0627\\u067E \\u0627\\u0633\\u062A\\u0641\\u0627\\u062F\\u0647 \\u0645\\u06CC\\u200C\\u0634\\u0648\\u062F\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":[]},{\"Name\":\"mobileVideoUrl\",\"Caption\":\"\\u0648\\u06CC\\u062F\\u06CC\\u0648\\u06CC \\u0645\\u0648\\u0628\\u0627\\u06CC\\u0644 (\\u0627\\u062E\\u062A\\u06CC\\u0627\\u0631\\u06CC)\",\"Type\":\"video\",\"PlaceHolder\":\"\\u0627\\u0646\\u062A\\u062E\\u0627\\u0628 \\u0648\\u06CC\\u062F\\u06CC\\u0648 (mp4 \\u06CC\\u0627 webm)\",\"Help\":\"\\u0648\\u06CC\\u062F\\u06CC\\u0648\\u06CC \\u0633\\u0628\\u06A9 \\u0648 \\u0639\\u0645\\u0648\\u062F\\u06CC \\u0628\\u0631\\u0627\\u06CC \\u0645\\u0648\\u0628\\u0627\\u06CC\\u0644\\u060C \\u062D\\u062F\\u0627\\u06A9\\u062B\\u0631 \\u06F3\\u06F0 \\u0645\\u06AF\\u0627\\u0628\\u0627\\u06CC\\u062A\\u061B \\u0627\\u06AF\\u0631 \\u062E\\u0627\\u0644\\u06CC \\u0628\\u0627\\u0634\\u062F \\u0627\\u0632 \\u0648\\u06CC\\u062F\\u06CC\\u0648\\u06CC \\u062F\\u0633\\u06A9\\u062A\\u0627\\u067E \\u0627\\u0633\\u062A\\u0641\\u0627\\u062F\\u0647 \\u0645\\u06CC\\u200C\\u0634\\u0648\\u062F\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":[]},{\"Name\":\"removeMobileBanner\",\"Caption\":\"\\u062D\\u0630\\u0641 \\u067E\\u0648\\u0633\\u062A\\u0631 \\u0645\\u0648\\u0628\\u0627\\u06CC\\u0644 \\u0641\\u0639\\u0644\\u06CC\",\"Type\":\"checkbox\",\"PlaceHolder\":\"\",\"Help\":\"\\u062F\\u0631 \\u0648\\u06CC\\u0631\\u0627\\u06CC\\u0634: \\u0628\\u0631\\u0627\\u06CC \\u062D\\u0630\\u0641 \\u067E\\u0648\\u0633\\u062A\\u0631 \\u0645\\u0648\\u0628\\u0627\\u06CC\\u0644 \\u062B\\u0628\\u062A\\u200C\\u0634\\u062F\\u0647 \\u0641\\u0639\\u0627\\u0644 \\u06A9\\u0646\\u06CC\\u062F.\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":[]},{\"Name\":\"removeMobileVideo\",\"Caption\":\"\\u062D\\u0630\\u0641 \\u0648\\u06CC\\u062F\\u06CC\\u0648\\u06CC \\u0645\\u0648\\u0628\\u0627\\u06CC\\u0644 \\u0641\\u0639\\u0644\\u06CC\",\"Type\":\"checkbox\",\"PlaceHolder\":\"\",\"Help\":\"\\u062F\\u0631 \\u0648\\u06CC\\u0631\\u0627\\u06CC\\u0634: \\u0628\\u0631\\u0627\\u06CC \\u062D\\u0630\\u0641 \\u0648\\u06CC\\u062F\\u06CC\\u0648\\u06CC \\u0645\\u0648\\u0628\\u0627\\u06CC\\u0644 \\u062B\\u0628\\u062A\\u200C\\u0634\\u062F\\u0647 \\u0641\\u0639\\u0627\\u0644 \\u06A9\\u0646\\u06CC\\u062F.\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":[]},{\"Name\":\"removeVideo\",\"Caption\":\"\\u062D\\u0630\\u0641 \\u0648\\u06CC\\u062F\\u06CC\\u0648 \\u0641\\u0639\\u0644\\u06CC\",\"Type\":\"checkbox\",\"PlaceHolder\":\"\",\"Help\":\"\\u062F\\u0631 \\u0648\\u06CC\\u0631\\u0627\\u06CC\\u0634: \\u0628\\u0631\\u0627\\u06CC \\u062D\\u0630\\u0641 \\u0648\\u06CC\\u062F\\u06CC\\u0648\\u06CC \\u062B\\u0628\\u062A\\u200C\\u0634\\u062F\\u0647 \\u0641\\u0639\\u0627\\u0644 \\u06A9\\u0646\\u06CC\\u062F.\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":[]}]", "اسلاید صفحه اصلی" });
        }
    }
}
