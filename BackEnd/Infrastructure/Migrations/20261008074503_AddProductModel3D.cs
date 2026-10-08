using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductModel3D : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductModels3D",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    ModelUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ModelSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    UsdzUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UsdzSizeBytes = table.Column<long>(type: "bigint", nullable: true),
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
                    table.PrimaryKey("PK_ProductModels3D", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductModels3D_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductScanSources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    FileUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Kind = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_ProductScanSources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductScanSources_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductModels3D_ProductId",
                table: "ProductModels3D",
                column: "ProductId",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_ProductScanSources_ProductId",
                table: "ProductScanSources",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductModels3D");

            migrationBuilder.DropTable(
                name: "ProductScanSources");
        }
    }
}
