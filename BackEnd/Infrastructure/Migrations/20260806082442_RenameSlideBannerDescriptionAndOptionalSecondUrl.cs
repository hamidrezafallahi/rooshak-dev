using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameSlideBannerDescriptionAndOptionalSecondUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotent: prod may already have BannerDescription while this migration is still pending.
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM information_schema.columns
                        WHERE table_schema = 'public'
                          AND table_name = 'Slides'
                          AND column_name = 'BannerDescrioption'
                    ) THEN
                        ALTER TABLE "Slides" RENAME COLUMN "BannerDescrioption" TO "BannerDescription";
                    END IF;
                END $$;
                """);

            migrationBuilder.UpdateData(
                table: "EntityConfigs",
                keyColumn: "Id",
                keyValue: 20,
                column: "FormFieldsJson",
                value: "[{\"Name\":\"bannerUrl\",\"Caption\":\"\\u0627\\u0646\\u062A\\u062E\\u0627\\u0628 \\u0628\\u0646\\u0631\",\"Type\":\"file\",\"PlaceHolder\":\"\\u0627\\u0646\\u062A\\u062E\\u0627\\u0628 \\u0628\\u0646\\u0631\",\"Help\":\"\\u0639\\u06A9\\u0633\\u06CC \\u06A9\\u0647 \\u0627\\u0646\\u062A\\u0638\\u0627\\u0631 \\u0645\\u06CC \\u0631\\u0648\\u062F \\u062F\\u0631 \\u0635\\u0641\\u062D\\u0647 \\u0627\\u0635\\u0644\\u06CC \\u062F\\u06CC\\u062F\\u0647 \\u0634\\u0648\\u062F\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":[{\"Rule\":\"required\",\"Condition\":\"true\",\"Message\":\"\\u0627\\u0646\\u062A\\u062E\\u0627\\u0628 \\u0639\\u06A9\\u0633 \\u0627\\u0644\\u0632\\u0627\\u0645\\u06CC \\u0627\\u0633\\u062A\"}]},{\"Name\":\"bannerTitle\",\"Caption\":\"\\u0639\\u0646\\u0648\\u0627\\u0646 \\u0628\\u0646\\u0631\",\"Type\":\"text\",\"PlaceHolder\":\"\\u0639\\u0646\\u0648\\u0627\\u0646 \\u0628\\u0646\\u0631\",\"Help\":\"\\u062F\\u0631 \\u0628\\u0646\\u0631 \\u0686\\u0647 \\u0639\\u0646\\u0648\\u0627\\u0646\\u06CC \\u0646\\u0645\\u0627\\u06CC\\u0634 \\u062F\\u0627\\u062F\\u0647 \\u0634\\u0648\\u062F \\u061F \\u061F\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":[{\"Rule\":\"required\",\"Condition\":\"true\",\"Message\":\"\\u0639\\u0646\\u0648\\u0627\\u0646 \\u0628\\u0646\\u0631 \\u0627\\u0644\\u0632\\u0627\\u0645\\u06CC \\u0627\\u0633\\u062A\"}]},{\"Name\":\"bannerDescription\",\"Caption\":\"\\u062A\\u0648\\u0636\\u06CC\\u062D \\u0628\\u0646\\u0631\",\"Type\":\"text\",\"PlaceHolder\":\"\\u062A\\u0648\\u0636\\u06CC\\u062D\\u0627\\u062A \\u0628\\u0646\\u0631\",\"Help\":\"\\u062F\\u0631 \\u0628\\u0646\\u0631 \\u0686\\u0647 \\u062A\\u0648\\u0636\\u06CC\\u062D\\u0627\\u062A\\u06CC \\u0646\\u0645\\u0627\\u06CC\\u0634 \\u062F\\u0627\\u062F\\u0647 \\u0634\\u0648\\u062F \\u061F\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":[{\"Rule\":\"required\",\"Condition\":\"true\",\"Message\":\"\\u062A\\u0648\\u0636\\u06CC\\u062D\\u0627\\u062A \\u0628\\u0646\\u0631 \\u0627\\u0644\\u0632\\u0627\\u0645\\u06CC \\u0627\\u0633\\u062A\"}]},{\"Name\":\"firstUrl\",\"Caption\":\"\\u0622\\u062F\\u0631\\u0633 \\u0627\\u0648\\u0644\",\"Type\":\"text\",\"PlaceHolder\":\"\\u0622\\u062F\\u0631\\u0633 \\u0627\\u0648\\u0644\",\"Help\":\"\\u0628\\u0627 \\u06A9\\u0644\\u06CC\\u06A9 \\u0628\\u0631 \\u0631\\u0648\\u06CC \\u0639\\u06A9\\u0633 \\u0628\\u0647 \\u0686\\u0647 \\u0622\\u062F\\u0631\\u0633\\u06CC \\u0628\\u0631\\u0648\\u062F \\u061F\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":[{\"Rule\":\"required\",\"Condition\":\"true\",\"Message\":\"\\u0622\\u062F\\u0631\\u0633   \\u0627\\u0644\\u0632\\u0627\\u0645\\u06CC \\u0627\\u0633\\u062A\"}]},{\"Name\":\"secondUrl\",\"Caption\":\"\\u0622\\u062F\\u0631\\u0633 \\u062F\\u0648\\u0645\",\"Type\":\"text\",\"PlaceHolder\":\"\\u0622\\u062F\\u0631\\u0633 \\u0635\\u0641\\u062D\\u0647 \\u062F\\u0648\\u0645\",\"Help\":\"\\u0628\\u0627 \\u06A9\\u0644\\u06CC\\u06A9 \\u0628\\u0631 \\u0631\\u0648\\u06CC \\u062F\\u06A9\\u0645\\u0647 \\u062F\\u0648\\u0645 \\u0628\\u0647 \\u0686\\u0647 \\u0622\\u062F\\u0631\\u0633\\u06CC \\u0628\\u0631\\u0648\\u062F \\u061F (\\u0627\\u062E\\u062A\\u06CC\\u0627\\u0631\\u06CC)\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":[]}]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BannerDescription",
                table: "Slides",
                newName: "BannerDescrioption");

            migrationBuilder.UpdateData(
                table: "EntityConfigs",
                keyColumn: "Id",
                keyValue: 20,
                column: "FormFieldsJson",
                value: "[{\"Name\":\"bannerUrl\",\"Caption\":\"\\u0627\\u0646\\u062A\\u062E\\u0627\\u0628 \\u0628\\u0646\\u0631\",\"Type\":\"file\",\"PlaceHolder\":\"\\u0627\\u0646\\u062A\\u062E\\u0627\\u0628 \\u0628\\u0646\\u0631\",\"Help\":\"\\u0639\\u06A9\\u0633\\u06CC \\u06A9\\u0647 \\u0627\\u0646\\u062A\\u0638\\u0627\\u0631 \\u0645\\u06CC \\u0631\\u0648\\u062F \\u062F\\u0631 \\u0635\\u0641\\u062D\\u0647 \\u0627\\u0635\\u0644\\u06CC \\u062F\\u06CC\\u062F\\u0647 \\u0634\\u0648\\u062F\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":[{\"Rule\":\"required\",\"Condition\":\"true\",\"Message\":\"\\u0627\\u0646\\u062A\\u062E\\u0627\\u0628 \\u0639\\u06A9\\u0633 \\u0627\\u0644\\u0632\\u0627\\u0645\\u06CC \\u0627\\u0633\\u062A\"}]},{\"Name\":\"bannerTitle\",\"Caption\":\"\\u0639\\u0646\\u0648\\u0627\\u0646 \\u0628\\u0646\\u0631\",\"Type\":\"text\",\"PlaceHolder\":\"\\u0639\\u0646\\u0648\\u0627\\u0646 \\u0628\\u0646\\u0631\",\"Help\":\"\\u062F\\u0631 \\u0628\\u0646\\u0631 \\u0686\\u0647 \\u0639\\u0646\\u0648\\u0627\\u0646\\u06CC \\u0646\\u0645\\u0627\\u06CC\\u0634 \\u062F\\u0627\\u062F\\u0647 \\u0634\\u0648\\u062F \\u061F \\u061F\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":[{\"Rule\":\"required\",\"Condition\":\"true\",\"Message\":\"\\u0639\\u0646\\u0648\\u0627\\u0646 \\u0628\\u0646\\u0631 \\u0627\\u0644\\u0632\\u0627\\u0645\\u06CC \\u0627\\u0633\\u062A\"}]},{\"Name\":\"bannerDescription\",\"Caption\":\"\\u062A\\u0648\\u0636\\u06CC\\u062D \\u0628\\u0646\\u0631\",\"Type\":\"text\",\"PlaceHolder\":\"\\u062A\\u0648\\u0636\\u06CC\\u062D\\u0627\\u062A \\u0628\\u0646\\u0631\",\"Help\":\"\\u062F\\u0631 \\u0628\\u0646\\u0631 \\u0686\\u0647 \\u062A\\u0648\\u0636\\u06CC\\u062D\\u0627\\u062A\\u06CC \\u0646\\u0645\\u0627\\u06CC\\u0634 \\u062F\\u0627\\u062F\\u0647 \\u0634\\u0648\\u062F \\u061F\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":[{\"Rule\":\"required\",\"Condition\":\"true\",\"Message\":\"\\u062A\\u0648\\u0636\\u06CC\\u062D\\u0627\\u062A \\u0628\\u0646\\u0631 \\u0627\\u0644\\u0632\\u0627\\u0645\\u06CC \\u0627\\u0633\\u062A\"}]},{\"Name\":\"firstUrl\",\"Caption\":\"\\u0622\\u062F\\u0631\\u0633 \\u0627\\u0648\\u0644\",\"Type\":\"text\",\"PlaceHolder\":\"\\u0622\\u062F\\u0631\\u0633 \\u0627\\u0648\\u0644\",\"Help\":\"\\u0628\\u0627 \\u06A9\\u0644\\u06CC\\u06A9 \\u0628\\u0631 \\u0631\\u0648\\u06CC \\u0639\\u06A9\\u0633 \\u0628\\u0647 \\u0686\\u0647 \\u0622\\u062F\\u0631\\u0633\\u06CC \\u0628\\u0631\\u0648\\u062F \\u061F\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":[{\"Rule\":\"required\",\"Condition\":\"true\",\"Message\":\"\\u0622\\u062F\\u0631\\u0633   \\u0627\\u0644\\u0632\\u0627\\u0645\\u06CC \\u0627\\u0633\\u062A\"}]},{\"Name\":\"secondUrl\",\"Caption\":\"\\u0622\\u062F\\u0631\\u0633 \\u062F\\u0648\\u0645\",\"Type\":\"text\",\"PlaceHolder\":\"\\u0622\\u062F\\u0631\\u0633 \\u0635\\u0641\\u062D\\u0647 \\u062F\\u0648\\u0645\",\"Help\":\"\\u0628\\u0627 \\u06A9\\u0644\\u06CC\\u06A9 \\u0628\\u0631 \\u0631\\u0648\\u06CC \\u0639\\u06A9\\u0633 \\u0628\\u0647 \\u0686\\u0647 \\u0622\\u062F\\u0631\\u0633\\u06CC \\u0628\\u0631\\u0648\\u062F \\u061F\",\"Order\":0,\"FetchConfig\":null,\"Options\":null,\"Rules\":[{\"Rule\":\"required\",\"Condition\":\"true\",\"Message\":\"\\u0622\\u062F\\u0631\\u0633 \\u0635\\u0641\\u062D\\u0647 \\u062F\\u0648\\u0645 \\u0627\\u0644\\u0632\\u0627\\u0645\\u06CC \\u0627\\u0633\\u062A\"}]}]");
        }
    }
}
