using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MomVibe.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBundles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bundles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SellerId = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsSold = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bundles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bundles_AspNetUsers_SellerId",
                        column: x => x.SellerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BundleItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BundleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BundleItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BundleItems_Bundles_BundleId",
                        column: x => x.BundleId,
                        principalTable: "Bundles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BundleItems_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bundles_SellerId",
                table: "Bundles",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_BundleItems_BundleId",
                table: "BundleItems",
                column: "BundleId");

            migrationBuilder.CreateIndex(
                name: "IX_BundleItems_ItemId",
                table: "BundleItems",
                column: "ItemId");

            // Make PurchaseRequests.ItemId nullable and add BundleId FK
            migrationBuilder.AlterColumn<Guid>(
                name: "ItemId",
                table: "PurchaseRequests",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "BundleId",
                table: "PurchaseRequests",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseRequests_Bundles_BundleId",
                table: "PurchaseRequests",
                column: "BundleId",
                principalTable: "Bundles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequests_BundleId",
                table: "PurchaseRequests",
                column: "BundleId");

            // Make Payments.ItemId nullable and add BundleId FK
            migrationBuilder.AlterColumn<Guid>(
                name: "ItemId",
                table: "Payments",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "BundleId",
                table: "Payments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Bundles_BundleId",
                table: "Payments",
                column: "BundleId",
                principalTable: "Bundles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BundleId",
                table: "Payments",
                column: "BundleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Payments_Bundles_BundleId", table: "Payments");
            migrationBuilder.DropIndex(name: "IX_Payments_BundleId", table: "Payments");
            migrationBuilder.DropColumn(name: "BundleId", table: "Payments");
            migrationBuilder.AlterColumn<Guid>(
                name: "ItemId",
                table: "Payments",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.DropForeignKey(name: "FK_PurchaseRequests_Bundles_BundleId", table: "PurchaseRequests");
            migrationBuilder.DropIndex(name: "IX_PurchaseRequests_BundleId", table: "PurchaseRequests");
            migrationBuilder.DropColumn(name: "BundleId", table: "PurchaseRequests");
            migrationBuilder.AlterColumn<Guid>(
                name: "ItemId",
                table: "PurchaseRequests",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.DropTable(name: "BundleItems");
            migrationBuilder.DropTable(name: "Bundles");
        }
    }
}
