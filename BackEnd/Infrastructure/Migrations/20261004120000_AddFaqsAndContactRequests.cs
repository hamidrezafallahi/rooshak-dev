using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFaqsAndContactRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContactRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PreferredContactTime = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsReviewed = table.Column<bool>(type: "boolean", nullable: false),
                    AdminNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_ContactRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Faqs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Question = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Answer = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
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
                    table.PrimaryKey("PK_Faqs", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "EntityConfigs",
                columns: new[] { "Id", "ActionsJson", "ColumnsJson", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "EndPoint", "EnglishDisplayName", "EntityIconBase64", "EntityName", "FormFieldsJson", "IsActive", "PersianDisplayName", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 30, "[\"active\",\"edit\",\"delete\",\"new\"]", "[{\"Header\":\"\\u0634\\u0646\\u0627\\u0633\\u0647\",\"Accessor\":\"id\",\"Type\":\"number\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u0633\\u0648\\u0627\\u0644\",\"Accessor\":\"question\",\"Type\":\"text\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u067E\\u0627\\u0633\\u062E\",\"Accessor\":\"answer\",\"Type\":\"text\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u062A\\u0631\\u062A\\u06CC\\u0628 \\u0646\\u0645\\u0627\\u06CC\\u0634\",\"Accessor\":\"displayOrder\",\"Type\":\"number\",\"Sortable\":false,\"Filterable\":false,\"Options\":null}]", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, null, null, "faqs", "FAQs", "<svg xmlns=\"http://www.w3.org/2000/svg\" fill=\"none\" viewBox=\"0 0 24 24\" stroke-width=\"1.5\" stroke=\"currentColor\" class=\"size-6\"><path stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"M9.879 7.519c1.171-1.025 3.071-1.025 4.242 0 1.172 1.025 1.172 2.687 0 3.712-.203.179-.43.326-.67.442-.745.361-1.45.999-1.45 1.827v.75M21 12a9 9 0 1 1-18 0 9 9 0 0 1 18 0Zm-9 5.25h.008v.008H12v-.008Z\" /></svg>", "faqs", "[{\"Name\":\"question\",\"Caption\":\"\\u0633\\u0648\\u0627\\u0644\",\"Type\":\"text\",\"PlaceHolder\":\"\\u0645\\u062B\\u0644\\u0627: \\u0627\\u0631\\u0633\\u0627\\u0644 \\u0633\\u0641\\u0627\\u0631\\u0634 \\u0686\\u0642\\u062F\\u0631 \\u0632\\u0645\\u0627\\u0646 \\u0645\\u06CC \\u0628\\u0631\\u062F\\u061F\",\"Help\":\"\\u0645\\u062A\\u0646 \\u0633\\u0648\\u0627\\u0644 \\u0647\\u0645\\u0627\\u0646 \\u0637\\u0648\\u0631 \\u06A9\\u0647 \\u062F\\u0631 \\u0633\\u0627\\u06CC\\u062A \\u0646\\u0645\\u0627\\u06CC\\u0634 \\u062F\\u0627\\u062F\\u0647 \\u0645\\u06CC \\u0634\\u0648\\u062F\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":[{\"Rule\":\"required\",\"Condition\":\"true\",\"Message\":\"\\u0645\\u062A\\u0646 \\u0633\\u0648\\u0627\\u0644 \\u0627\\u0644\\u0632\\u0627\\u0645\\u06CC \\u0627\\u0633\\u062A\"}]},{\"Name\":\"displayOrder\",\"Caption\":\"\\u062A\\u0631\\u062A\\u06CC\\u0628 \\u0646\\u0645\\u0627\\u06CC\\u0634\",\"Type\":\"number\",\"PlaceHolder\":\"\\u0645\\u062B\\u0644\\u0627: 1\",\"Help\":\"\\u0639\\u062F\\u062F \\u06A9\\u0648\\u0686\\u06A9 \\u062A\\u0631 \\u0628\\u0627\\u0644\\u0627\\u062A\\u0631 \\u0646\\u0645\\u0627\\u06CC\\u0634 \\u062F\\u0627\\u062F\\u0647 \\u0645\\u06CC \\u0634\\u0648\\u062F\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":null},{\"Name\":\"answer\",\"Caption\":\"\\u067E\\u0627\\u0633\\u062E\",\"Type\":\"textarea\",\"PlaceHolder\":\"\\u067E\\u0627\\u0633\\u062E \\u06A9\\u0627\\u0645\\u0644 \\u0633\\u0648\\u0627\\u0644\",\"Help\":null,\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":[{\"Rule\":\"required\",\"Condition\":\"true\",\"Message\":\"\\u0645\\u062A\\u0646 \\u067E\\u0627\\u0633\\u062E \\u0627\\u0644\\u0632\\u0627\\u0645\\u06CC \\u0627\\u0633\\u062A\"}]}]", true, "سوالات متداول", null, null },
                    { 31, "[\"edit\",\"delete\",\"new\"]", "[{\"Header\":\"\\u0634\\u0646\\u0627\\u0633\\u0647\",\"Accessor\":\"id\",\"Type\":\"number\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u0646\\u0627\\u0645\",\"Accessor\":\"firstName\",\"Type\":\"text\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u0646\\u0627\\u0645 \\u062E\\u0627\\u0646\\u0648\\u0627\\u062F\\u06AF\\u06CC\",\"Accessor\":\"lastName\",\"Type\":\"text\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u0634\\u0645\\u0627\\u0631\\u0647 \\u062A\\u0645\\u0627\\u0633\",\"Accessor\":\"phoneNumber\",\"Type\":\"text\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u0622\\u062F\\u0631\\u0633\",\"Accessor\":\"address\",\"Type\":\"text\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u0632\\u0645\\u0627\\u0646 \\u0645\\u0646\\u0627\\u0633\\u0628 \\u062A\\u0645\\u0627\\u0633\",\"Accessor\":\"preferredContactTime\",\"Type\":\"text\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u062A\\u0627\\u0631\\u06CC\\u062E \\u062B\\u0628\\u062A\",\"Accessor\":\"createdAt\",\"Type\":\"date\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u0628\\u0631\\u0631\\u0633\\u06CC \\u0634\\u062F\\u0647\",\"Accessor\":\"isReviewed\",\"Type\":\"bool\",\"Sortable\":false,\"Filterable\":false,\"Options\":null}]", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, null, null, "contactRequests", "Contact requests", "<svg xmlns=\"http://www.w3.org/2000/svg\" fill=\"none\" viewBox=\"0 0 24 24\" stroke-width=\"1.5\" stroke=\"currentColor\" class=\"size-6\"><path stroke-linecap=\"round\" stroke-linejoin=\"round\" d=\"M21.75 6.75v10.5a2.25 2.25 0 0 1-2.25 2.25h-15a2.25 2.25 0 0 1-2.25-2.25V6.75m19.5 0A2.25 2.25 0 0 0 19.5 4.5h-15a2.25 2.25 0 0 0-2.25 2.25m19.5 0v.243a2.25 2.25 0 0 1-1.07 1.916l-7.5 4.615a2.25 2.25 0 0 1-2.36 0L3.32 8.91a2.25 2.25 0 0 1-1.07-1.916V6.75\" /></svg>", "contactRequests", "[{\"Name\":\"firstName\",\"Caption\":\"\\u0646\\u0627\\u0645\",\"Type\":\"text\",\"PlaceHolder\":null,\"Help\":null,\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":[{\"Rule\":\"required\",\"Condition\":\"true\",\"Message\":\"\\u0646\\u0627\\u0645 \\u0627\\u0644\\u0632\\u0627\\u0645\\u06CC \\u0627\\u0633\\u062A\"}]},{\"Name\":\"lastName\",\"Caption\":\"\\u0646\\u0627\\u0645 \\u062E\\u0627\\u0646\\u0648\\u0627\\u062F\\u06AF\\u06CC\",\"Type\":\"text\",\"PlaceHolder\":null,\"Help\":null,\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":[{\"Rule\":\"required\",\"Condition\":\"true\",\"Message\":\"\\u0646\\u0627\\u0645 \\u062E\\u0627\\u0646\\u0648\\u0627\\u062F\\u06AF\\u06CC \\u0627\\u0644\\u0632\\u0627\\u0645\\u06CC \\u0627\\u0633\\u062A\"}]},{\"Name\":\"phoneNumber\",\"Caption\":\"\\u0634\\u0645\\u0627\\u0631\\u0647 \\u062A\\u0645\\u0627\\u0633\",\"Type\":\"text\",\"PlaceHolder\":\"09123456789\",\"Help\":null,\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":[{\"Rule\":\"required\",\"Condition\":\"true\",\"Message\":\"\\u0634\\u0645\\u0627\\u0631\\u0647 \\u062A\\u0645\\u0627\\u0633 \\u0627\\u0644\\u0632\\u0627\\u0645\\u06CC \\u0627\\u0633\\u062A\"}]},{\"Name\":\"email\",\"Caption\":\"\\u0627\\u06CC\\u0645\\u06CC\\u0644\",\"Type\":\"text\",\"PlaceHolder\":\"name@example.com\",\"Help\":null,\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":null},{\"Name\":\"preferredContactTime\",\"Caption\":\"\\u0632\\u0645\\u0627\\u0646 \\u0645\\u0646\\u0627\\u0633\\u0628 \\u062A\\u0645\\u0627\\u0633\",\"Type\":\"text\",\"PlaceHolder\":\"\\u0645\\u062B\\u0644\\u0627: \\u0634\\u0646\\u0628\\u0647 \\u062A\\u0627 \\u0686\\u0647\\u0627\\u0631\\u0634\\u0646\\u0628\\u0647 \\u06F9 \\u062A\\u0627 \\u06F1\\u06F3\",\"Help\":null,\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":null},{\"Name\":\"isReviewed\",\"Caption\":\"\\u0628\\u0631\\u0631\\u0633\\u06CC \\u0634\\u062F\\u0647\",\"Type\":\"checkbox\",\"PlaceHolder\":null,\"Help\":\"\\u0628\\u0639\\u062F \\u0627\\u0632 \\u062A\\u0645\\u0627\\u0633 \\u06CC\\u0627 \\u067E\\u06CC\\u06AF\\u06CC\\u0631\\u06CC \\u062F\\u0631\\u062E\\u0648\\u0627\\u0633\\u062A\\u060C \\u0627\\u06CC\\u0646 \\u06AF\\u0632\\u06CC\\u0646\\u0647 \\u0631\\u0627 \\u0641\\u0639\\u0627\\u0644 \\u06A9\\u0646\\u06CC\\u062F\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":null},{\"Name\":\"address\",\"Caption\":\"\\u0622\\u062F\\u0631\\u0633\",\"Type\":\"textarea\",\"PlaceHolder\":null,\"Help\":null,\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":null},{\"Name\":\"message\",\"Caption\":\"\\u0645\\u062A\\u0646 \\u067E\\u06CC\\u0627\\u0645\",\"Type\":\"textarea\",\"PlaceHolder\":null,\"Help\":null,\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":null},{\"Name\":\"adminNote\",\"Caption\":\"\\u06CC\\u0627\\u062F\\u062F\\u0627\\u0634\\u062A \\u0627\\u062F\\u0645\\u06CC\\u0646\",\"Type\":\"textarea\",\"PlaceHolder\":\"\\u0646\\u062A\\u06CC\\u062C\\u0647 \\u062A\\u0645\\u0627\\u0633 \\u06CC\\u0627 \\u067E\\u06CC\\u06AF\\u06CC\\u0631\\u06CC\",\"Help\":null,\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":null}]", true, "همکاری با ما", null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "EntityConfigs",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "EntityConfigs",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DropTable(
                name: "ContactRequests");

            migrationBuilder.DropTable(
                name: "Faqs");
        }
    }
}
