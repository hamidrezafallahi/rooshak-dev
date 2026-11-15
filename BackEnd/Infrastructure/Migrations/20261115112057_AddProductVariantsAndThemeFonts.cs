using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductVariantsAndThemeFonts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FontFamily",
                table: "ThemeSettings",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "iransans");

            migrationBuilder.AddColumn<string>(
                name: "HeadingFontFamily",
                table: "ThemeSettings",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "iransans");

            migrationBuilder.AddColumn<string>(
                name: "VariantName",
                table: "ProductOffers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VariantValue",
                table: "ProductOffers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "EntityConfigs",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "ColumnsJson", "FormFieldsJson" },
                values: new object[] { "[{\"Header\":\"\\u0634\\u0646\\u0627\\u0633\\u0647\",\"Accessor\":\"id\",\"Type\":\"number\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u0646\\u0627\\u0645 \\u0645\\u062D\\u0635\\u0648\\u0644\",\"Accessor\":\"productName\",\"Type\":\"text\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u0646\\u0627\\u0645 \\u062A\\u0627\\u0645\\u06CC\\u0646 \\u06A9\\u0646\\u0646\\u062F\\u0647\",\"Accessor\":\"supplierName\",\"Type\":\"text\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u062A\\u0635\\u0648\\u06CC\\u0631 \\u062A\\u0627\\u0645\\u06CC\\u0646 \\u06A9\\u0646\\u0646\\u062F\\u0647\",\"Accessor\":\"supplierImage\",\"Type\":\"image\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u062A\\u0646\\u0648\\u0639\",\"Accessor\":\"variantValue\",\"Type\":\"text\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u0642\\u06CC\\u0645\\u062A \\u067E\\u0627\\u06CC\\u0647\",\"Accessor\":\"basePrice\",\"Type\":\"price\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u0642\\u06CC\\u0645\\u062A \\u0646\\u0647\\u0627\\u06CC\\u06CC\",\"Accessor\":\"finalPrice\",\"Type\":\"price\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u0645\\u0648\\u062C\\u0648\\u062F\\u06CC\",\"Accessor\":\"inventory\",\"Type\":\"number\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u062A\\u0627\\u0631\\u06CC\\u062E \\u0627\\u06CC\\u062C\\u0627\\u062F\",\"Accessor\":\"createdAt\",\"Type\":\"date\",\"Sortable\":false,\"Filterable\":false,\"Options\":null}]", "[{\"Name\":\"productId\",\"Caption\":\"\\u0627\\u0646\\u062A\\u062E\\u0627\\u0628 \\u0645\\u062D\\u0635\\u0648\\u0644\",\"Type\":\"dynamicSelect\",\"PlaceHolder\":\" \\u0645\\u062D\\u0635\\u0648\\u0644 \\u0631\\u0627 \\u0627\\u0646\\u062A\\u062E\\u0627\\u0628 \\u06A9\\u0646\\u06CC\\u062F\",\"Help\":\"\\u0645\\u062B\\u0644\\u0627\\u064B: \\u062A\\u0644\\u0648\\u06CC\\u0632\\u06CC\\u0648\\u0646 \\u0633\\u0627\\u0645\\u0633\\u0648\\u0646\\u06AF 55 \\u0627\\u06CC\\u0646\\u0686\",\"Order\":0,\"FetchConfig\":{\"api\":\"Products/selectOption\",\"fetchFilters\":[]},\"Options\":null,\"Rules\":[{\"Rule\":\"required\",\"Condition\":\"true\",\"Message\":\" \\u0645\\u062D\\u0635\\u0648\\u0644 \\u0627\\u0644\\u0632\\u0627\\u0645\\u06CC \\u0627\\u0633\\u062A\"}]},{\"Name\":\"variantName\",\"Caption\":\"\\u0639\\u0646\\u0648\\u0627\\u0646 \\u062A\\u0646\\u0648\\u0639\",\"Type\":\"text\",\"PlaceHolder\":\"\\u0645\\u062B\\u0644\\u0627\\u064B: \\u0633\\u0627\\u06CC\\u0632\",\"Help\":\"\\u0627\\u062E\\u062A\\u06CC\\u0627\\u0631\\u06CC. \\u0627\\u06AF\\u0631 \\u0645\\u062D\\u0635\\u0648\\u0644 \\u0686\\u0646\\u062F \\u0645\\u062F\\u0644 \\u0628\\u0627 \\u0642\\u06CC\\u0645\\u062A \\u0645\\u062A\\u0641\\u0627\\u0648\\u062A \\u062F\\u0627\\u0631\\u062F (\\u0645\\u062B\\u0644 \\u0633\\u0627\\u06CC\\u0632 \\u06CC\\u0627 \\u0631\\u0646\\u06AF\\u200C\\u0647\\u0627\\u06CC \\u0645\\u062E\\u062A\\u0644\\u0641)\\u060C \\u0628\\u0631\\u0627\\u06CC \\u0647\\u0631 \\u0645\\u062F\\u0644 \\u06CC\\u06A9 \\u0633\\u0641\\u0627\\u0631\\u0634 \\u0641\\u0631\\u0648\\u0634 \\u062C\\u062F\\u0627 \\u0628\\u0633\\u0627\\u0632\\u06CC\\u062F.\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":[{\"Rule\":\"maxLength\",\"Condition\":\"50\",\"Message\":\"\\u062D\\u062F\\u0627\\u06A9\\u062B\\u0631 \\u06F5\\u06F0 \\u06A9\\u0627\\u0631\\u0627\\u06A9\\u062A\\u0631\"}]},{\"Name\":\"variantValue\",\"Caption\":\"\\u0645\\u0642\\u062F\\u0627\\u0631 \\u062A\\u0646\\u0648\\u0639\",\"Type\":\"text\",\"PlaceHolder\":\"\\u0645\\u062B\\u0644\\u0627\\u064B: \\u06F2\\u06F4 \\u0633\\u0627\\u0646\\u062A\\u06CC\\u200C\\u0645\\u062A\\u0631\",\"Help\":\"\\u0631\\u0648\\u06CC \\u06A9\\u0627\\u0631\\u062A \\u0645\\u062D\\u0635\\u0648\\u0644 \\u0628\\u0647\\u200C\\u0635\\u0648\\u0631\\u062A \\u062F\\u06A9\\u0645\\u0647\\u0654 \\u0642\\u0627\\u0628\\u0644 \\u0627\\u0646\\u062A\\u062E\\u0627\\u0628 \\u0646\\u0645\\u0627\\u06CC\\u0634 \\u062F\\u0627\\u062F\\u0647 \\u0645\\u06CC\\u200C\\u0634\\u0648\\u062F \\u0648 \\u0642\\u06CC\\u0645\\u062A \\u0647\\u0645\\u06CC\\u0646 \\u0633\\u0641\\u0627\\u0631\\u0634 \\u0641\\u0631\\u0648\\u0634 \\u0631\\u0627 \\u0646\\u0634\\u0627\\u0646 \\u0645\\u06CC\\u200C\\u062F\\u0647\\u062F.\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":[{\"Rule\":\"maxLength\",\"Condition\":\"50\",\"Message\":\"\\u062D\\u062F\\u0627\\u06A9\\u062B\\u0631 \\u06F5\\u06F0 \\u06A9\\u0627\\u0631\\u0627\\u06A9\\u062A\\u0631\"}]},{\"Name\":\"basePrice\",\"Caption\":\"\\u0642\\u06CC\\u0645\\u062A \\u067E\\u0627\\u06CC\\u0647\",\"Type\":\"price\",\"PlaceHolder\":\"\\u0642\\u06CC\\u0645\\u062A \\u067E\\u0627\\u06CC\\u0647 \\u0645\\u062D\\u0635\\u0648\\u0644 \\u0631\\u0627 \\u0648\\u0627\\u0631\\u062F \\u06A9\\u0646\\u06CC\\u062F\",\"Help\":\"\\u0642\\u06CC\\u0645\\u062A \\u0645\\u062D\\u0635\\u0648\\u0644 \\u0631\\u0627 \\u0628\\u0646\\u0648\\u06CC\\u0633\\u06CC\\u062F\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":[{\"Rule\":\"required\",\"Condition\":\"true\",\"Message\":\"\\u0642\\u06CC\\u0645\\u062A \\u067E\\u0627\\u06CC\\u0647 \\u0627\\u0644\\u0632\\u0627\\u0645\\u06CC \\u0627\\u0633\\u062A\"}]},{\"Name\":\"inventory\",\"Caption\":\"\\u0645\\u0648\\u062C\\u0648\\u062F\\u06CC\",\"Type\":\"number\",\"PlaceHolder\":\"\\u062A\\u0639\\u062F\\u0627\\u062F \\u0645\\u0648\\u062C\\u0648\\u062F\\u06CC \\u0631\\u0627 \\u0648\\u0627\\u0631\\u062F \\u06A9\\u0646\\u06CC\\u062F\",\"Help\":\"\\u062A\\u0639\\u062F\\u0627\\u062F \\u0645\\u062D\\u0635\\u0648\\u0644 \\u062F\\u0631 \\u0627\\u0646\\u0628\\u0627\\u0631\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":[{\"Rule\":\"required\",\"Condition\":\"true\",\"Message\":\"\\u0645\\u0648\\u062C\\u0648\\u062F\\u06CC \\u0627\\u0644\\u0632\\u0627\\u0645\\u06CC \\u0627\\u0633\\u062A\"},{\"Rule\":\"min\",\"Condition\":\"0\",\"Message\":\"\\u0645\\u0648\\u062C\\u0648\\u062F\\u06CC \\u0646\\u0645\\u06CC\\u200C\\u062A\\u0648\\u0627\\u0646\\u062F \\u0645\\u0646\\u0641\\u06CC \\u0628\\u0627\\u0634\\u062F\"}]}]" });

            migrationBuilder.UpdateData(
                table: "ThemeSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "FontFamily", "HeadingFontFamily" },
                values: new object[] { "iransans", "iransans" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FontFamily",
                table: "ThemeSettings");

            migrationBuilder.DropColumn(
                name: "HeadingFontFamily",
                table: "ThemeSettings");

            migrationBuilder.DropColumn(
                name: "VariantName",
                table: "ProductOffers");

            migrationBuilder.DropColumn(
                name: "VariantValue",
                table: "ProductOffers");

            migrationBuilder.UpdateData(
                table: "EntityConfigs",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "ColumnsJson", "FormFieldsJson" },
                values: new object[] { "[{\"Header\":\"\\u0634\\u0646\\u0627\\u0633\\u0647\",\"Accessor\":\"id\",\"Type\":\"number\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u0646\\u0627\\u0645 \\u0645\\u062D\\u0635\\u0648\\u0644\",\"Accessor\":\"productName\",\"Type\":\"text\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u0646\\u0627\\u0645 \\u062A\\u0627\\u0645\\u06CC\\u0646 \\u06A9\\u0646\\u0646\\u062F\\u0647\",\"Accessor\":\"supplierName\",\"Type\":\"text\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u062A\\u0635\\u0648\\u06CC\\u0631 \\u062A\\u0627\\u0645\\u06CC\\u0646 \\u06A9\\u0646\\u0646\\u062F\\u0647\",\"Accessor\":\"supplierImage\",\"Type\":\"image\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u0642\\u06CC\\u0645\\u062A \\u067E\\u0627\\u06CC\\u0647\",\"Accessor\":\"basePrice\",\"Type\":\"price\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u0642\\u06CC\\u0645\\u062A \\u0646\\u0647\\u0627\\u06CC\\u06CC\",\"Accessor\":\"finalPrice\",\"Type\":\"price\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u0645\\u0648\\u062C\\u0648\\u062F\\u06CC\",\"Accessor\":\"inventory\",\"Type\":\"number\",\"Sortable\":false,\"Filterable\":false,\"Options\":null},{\"Header\":\"\\u062A\\u0627\\u0631\\u06CC\\u062E \\u0627\\u06CC\\u062C\\u0627\\u062F\",\"Accessor\":\"createdAt\",\"Type\":\"date\",\"Sortable\":false,\"Filterable\":false,\"Options\":null}]", "[{\"Name\":\"productId\",\"Caption\":\"\\u0627\\u0646\\u062A\\u062E\\u0627\\u0628 \\u0645\\u062D\\u0635\\u0648\\u0644\",\"Type\":\"dynamicSelect\",\"PlaceHolder\":\" \\u0645\\u062D\\u0635\\u0648\\u0644 \\u0631\\u0627 \\u0627\\u0646\\u062A\\u062E\\u0627\\u0628 \\u06A9\\u0646\\u06CC\\u062F\",\"Help\":\"\\u0645\\u062B\\u0644\\u0627\\u064B: \\u062A\\u0644\\u0648\\u06CC\\u0632\\u06CC\\u0648\\u0646 \\u0633\\u0627\\u0645\\u0633\\u0648\\u0646\\u06AF 55 \\u0627\\u06CC\\u0646\\u0686\",\"Order\":0,\"FetchConfig\":{\"api\":\"Products/selectOption\",\"fetchFilters\":[]},\"Options\":null,\"Rules\":[{\"Rule\":\"required\",\"Condition\":\"true\",\"Message\":\" \\u0645\\u062D\\u0635\\u0648\\u0644 \\u0627\\u0644\\u0632\\u0627\\u0645\\u06CC \\u0627\\u0633\\u062A\"}]},{\"Name\":\"basePrice\",\"Caption\":\"\\u0642\\u06CC\\u0645\\u062A \\u067E\\u0627\\u06CC\\u0647\",\"Type\":\"price\",\"PlaceHolder\":\"\\u0642\\u06CC\\u0645\\u062A \\u067E\\u0627\\u06CC\\u0647 \\u0645\\u062D\\u0635\\u0648\\u0644 \\u0631\\u0627 \\u0648\\u0627\\u0631\\u062F \\u06A9\\u0646\\u06CC\\u062F\",\"Help\":\"\\u0642\\u06CC\\u0645\\u062A \\u0645\\u062D\\u0635\\u0648\\u0644 \\u0631\\u0627 \\u0628\\u0646\\u0648\\u06CC\\u0633\\u06CC\\u062F\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":[{\"Rule\":\"required\",\"Condition\":\"true\",\"Message\":\"\\u0642\\u06CC\\u0645\\u062A \\u067E\\u0627\\u06CC\\u0647 \\u0627\\u0644\\u0632\\u0627\\u0645\\u06CC \\u0627\\u0633\\u062A\"}]},{\"Name\":\"inventory\",\"Caption\":\"\\u0645\\u0648\\u062C\\u0648\\u062F\\u06CC\",\"Type\":\"number\",\"PlaceHolder\":\"\\u062A\\u0639\\u062F\\u0627\\u062F \\u0645\\u0648\\u062C\\u0648\\u062F\\u06CC \\u0631\\u0627 \\u0648\\u0627\\u0631\\u062F \\u06A9\\u0646\\u06CC\\u062F\",\"Help\":\"\\u062A\\u0639\\u062F\\u0627\\u062F \\u0645\\u062D\\u0635\\u0648\\u0644 \\u062F\\u0631 \\u0627\\u0646\\u0628\\u0627\\u0631\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":[{\"Rule\":\"required\",\"Condition\":\"true\",\"Message\":\"\\u0645\\u0648\\u062C\\u0648\\u062F\\u06CC \\u0627\\u0644\\u0632\\u0627\\u0645\\u06CC \\u0627\\u0633\\u062A\"},{\"Rule\":\"min\",\"Condition\":\"0\",\"Message\":\"\\u0645\\u0648\\u062C\\u0648\\u062F\\u06CC \\u0646\\u0645\\u06CC\\u200C\\u062A\\u0648\\u0627\\u0646\\u062F \\u0645\\u0646\\u0641\\u06CC \\u0628\\u0627\\u0634\\u062F\"}]}]" });
        }
    }
}
