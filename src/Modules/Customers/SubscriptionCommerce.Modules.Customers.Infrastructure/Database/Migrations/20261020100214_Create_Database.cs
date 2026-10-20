using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SubscriptionCommerce.Modules.Customers.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Create_Database : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "customers");

            migrationBuilder.CreateTable(
                name: "Tenants",
                schema: "customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                schema: "customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                    table.UniqueConstraint("AK_Customers_Id_TenantId", x => new { x.Id, x.TenantId });
                    table.ForeignKey(
                        name: "FK_Customers_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "customers",
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CustomFieldDefinitions",
                schema: "customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AllowMultipleSelection = table.Column<bool>(type: "bit", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NormalizedName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, computedColumnSql: "UPPER([Name] COLLATE Latin1_General_100_BIN2)", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomFieldDefinitions", x => x.Id);
                    table.UniqueConstraint("AK_CustomFieldDefinitions_Id_TenantId", x => new { x.Id, x.TenantId });
                    table.CheckConstraint("CK_CustomFieldDefinitions_Name_Canonical", "DATALENGTH([Name]) > 0 AND UNICODE(LEFT([Name], 1)) NOT IN (9, 10, 11, 12, 13, 32, 133, 160, 5760, 8192, 8193, 8194, 8195, 8196, 8197, 8198, 8199, 8200, 8201, 8202, 8232, 8233, 8239, 8287, 12288) AND UNICODE(RIGHT([Name], 1)) NOT IN (9, 10, 11, 12, 13, 32, 133, 160, 5760, 8192, 8193, 8194, 8195, 8196, 8197, 8198, 8199, 8200, 8201, 8202, 8232, 8233, 8239, 8287, 12288)");
                    table.ForeignKey(
                        name: "FK_CustomFieldDefinitions_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "customers",
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CustomerFieldValues",
                schema: "customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    FieldDefinitionId = table.Column<int>(type: "int", nullable: false),
                    TextValue = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerFieldValues", x => x.Id);
                    table.UniqueConstraint("AK_CustomerFieldValues_Id_FieldDefinitionId", x => new { x.Id, x.FieldDefinitionId });
                    table.ForeignKey(
                        name: "FK_CustomerFieldValues_CustomFieldDefinitions_FieldDefinitionId_TenantId",
                        columns: x => new { x.FieldDefinitionId, x.TenantId },
                        principalSchema: "customers",
                        principalTable: "CustomFieldDefinitions",
                        principalColumns: new[] { "Id", "TenantId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomerFieldValues_Customers_CustomerId_TenantId",
                        columns: x => new { x.CustomerId, x.TenantId },
                        principalSchema: "customers",
                        principalTable: "Customers",
                        principalColumns: new[] { "Id", "TenantId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CustomFieldOptions",
                schema: "customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FieldDefinitionId = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    NormalizedLabel = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, computedColumnSql: "UPPER([Label] COLLATE Latin1_General_100_BIN2)", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomFieldOptions", x => x.Id);
                    table.UniqueConstraint("AK_CustomFieldOptions_Id_FieldDefinitionId", x => new { x.Id, x.FieldDefinitionId });
                    table.CheckConstraint("CK_CustomFieldOptions_Label_Canonical", "DATALENGTH([Label]) > 0 AND UNICODE(LEFT([Label], 1)) NOT IN (9, 10, 11, 12, 13, 32, 133, 160, 5760, 8192, 8193, 8194, 8195, 8196, 8197, 8198, 8199, 8200, 8201, 8202, 8232, 8233, 8239, 8287, 12288) AND UNICODE(RIGHT([Label], 1)) NOT IN (9, 10, 11, 12, 13, 32, 133, 160, 5760, 8192, 8193, 8194, 8195, 8196, 8197, 8198, 8199, 8200, 8201, 8202, 8232, 8233, 8239, 8287, 12288)");
                    table.ForeignKey(
                        name: "FK_CustomFieldOptions_CustomFieldDefinitions_FieldDefinitionId",
                        column: x => x.FieldDefinitionId,
                        principalSchema: "customers",
                        principalTable: "CustomFieldDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CustomerFieldValueHistory",
                schema: "customers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerFieldValueId = table.Column<int>(type: "int", nullable: false),
                    OldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChangedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerFieldValueHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerFieldValueHistory_CustomerFieldValues_CustomerFieldValueId",
                        column: x => x.CustomerFieldValueId,
                        principalSchema: "customers",
                        principalTable: "CustomerFieldValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CustomerFieldValueOptions",
                schema: "customers",
                columns: table => new
                {
                    CustomerFieldValueId = table.Column<int>(type: "int", nullable: false),
                    OptionId = table.Column<int>(type: "int", nullable: false),
                    FieldDefinitionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerFieldValueOptions", x => new { x.CustomerFieldValueId, x.OptionId });
                    table.ForeignKey(
                        name: "FK_CustomerFieldValueOptions_CustomFieldOptions_OptionId_FieldDefinitionId",
                        columns: x => new { x.OptionId, x.FieldDefinitionId },
                        principalSchema: "customers",
                        principalTable: "CustomFieldOptions",
                        principalColumns: new[] { "Id", "FieldDefinitionId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomerFieldValueOptions_CustomerFieldValues_CustomerFieldValueId_FieldDefinitionId",
                        columns: x => new { x.CustomerFieldValueId, x.FieldDefinitionId },
                        principalSchema: "customers",
                        principalTable: "CustomerFieldValues",
                        principalColumns: new[] { "Id", "FieldDefinitionId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerFieldValueHistory_CustomerFieldValueId",
                schema: "customers",
                table: "CustomerFieldValueHistory",
                column: "CustomerFieldValueId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerFieldValueOptions_CustomerFieldValueId_FieldDefinitionId",
                schema: "customers",
                table: "CustomerFieldValueOptions",
                columns: new[] { "CustomerFieldValueId", "FieldDefinitionId" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerFieldValueOptions_OptionId_FieldDefinitionId",
                schema: "customers",
                table: "CustomerFieldValueOptions",
                columns: new[] { "OptionId", "FieldDefinitionId" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerFieldValues_CustomerId_FieldDefinitionId",
                schema: "customers",
                table: "CustomerFieldValues",
                columns: new[] { "CustomerId", "FieldDefinitionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerFieldValues_CustomerId_TenantId",
                schema: "customers",
                table: "CustomerFieldValues",
                columns: new[] { "CustomerId", "TenantId" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerFieldValues_FieldDefinitionId_TenantId",
                schema: "customers",
                table: "CustomerFieldValues",
                columns: new[] { "FieldDefinitionId", "TenantId" });

            migrationBuilder.CreateIndex(
                name: "IX_Customers_TenantId",
                schema: "customers",
                table: "Customers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomFieldDefinitions_TenantId_NormalizedName",
                schema: "customers",
                table: "CustomFieldDefinitions",
                columns: new[] { "TenantId", "NormalizedName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomFieldOptions_FieldDefinitionId_NormalizedLabel",
                schema: "customers",
                table: "CustomFieldOptions",
                columns: new[] { "FieldDefinitionId", "NormalizedLabel" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerFieldValueHistory",
                schema: "customers");

            migrationBuilder.DropTable(
                name: "CustomerFieldValueOptions",
                schema: "customers");

            migrationBuilder.DropTable(
                name: "CustomFieldOptions",
                schema: "customers");

            migrationBuilder.DropTable(
                name: "CustomerFieldValues",
                schema: "customers");

            migrationBuilder.DropTable(
                name: "CustomFieldDefinitions",
                schema: "customers");

            migrationBuilder.DropTable(
                name: "Customers",
                schema: "customers");

            migrationBuilder.DropTable(
                name: "Tenants",
                schema: "customers");
        }
    }
}
