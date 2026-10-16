using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SubscriptionCommerce.Modules.Pricing.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Create_Database : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "pricing");

            migrationBuilder.CreateTable(
                name: "Coupons",
                schema: "pricing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nchar(3)", fixedLength: true, maxLength: 3, nullable: true),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coupons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PriceListDiscounts",
                schema: "pricing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nchar(3)", fixedLength: true, maxLength: 3, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceListDiscounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Promotions",
                schema: "pricing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nchar(3)", fixedLength: true, maxLength: 3, nullable: true),
                    StartsAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndsAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Promotions", x => x.Id);
                });

            migrationBuilder.InsertData(
                schema: "pricing",
                table: "Coupons",
                columns: new[] { "Id", "Code", "Currency", "ExpiresAtUtc", "IsActive", "Name", "Type", "Value" },
                values: new object[] { 1, "WELCOME10", "EUR", null, true, "Welcome coupon", "FixedAmount", 10m });

            migrationBuilder.InsertData(
                schema: "pricing",
                table: "PriceListDiscounts",
                columns: new[] { "Id", "Currency", "CustomerId", "IsActive", "Name", "Type", "Value" },
                values: new object[] { 1, null, 1, true, "Gold price list", "Percentage", 5m });

            migrationBuilder.InsertData(
                schema: "pricing",
                table: "Promotions",
                columns: new[] { "Id", "Currency", "EndsAtUtc", "IsActive", "Name", "StartsAtUtc", "Type", "Value" },
                values: new object[] { 1, null, new DateTime(2031, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Autumn promotion", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Percentage", 10m });

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_Code",
                schema: "pricing",
                table: "Coupons",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PriceListDiscounts_CustomerId",
                schema: "pricing",
                table: "PriceListDiscounts",
                column: "CustomerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Coupons",
                schema: "pricing");

            migrationBuilder.DropTable(
                name: "PriceListDiscounts",
                schema: "pricing");

            migrationBuilder.DropTable(
                name: "Promotions",
                schema: "pricing");
        }
    }
}
