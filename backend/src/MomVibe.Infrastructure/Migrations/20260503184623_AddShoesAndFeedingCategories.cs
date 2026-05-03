using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MomVibe.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShoesAndFeedingCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserRatings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RaterId = table.Column<string>(type: "text", nullable: false),
                    RatedUserId = table.Column<string>(type: "text", nullable: false),
                    PurchaseRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRatings_AspNetUsers_RatedUserId",
                        column: x => x.RatedUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRatings_AspNetUsers_RaterId",
                        column: x => x.RaterId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRatings_PurchaseRequests_PurchaseRequestId",
                        column: x => x.PurchaseRequestId,
                        principalTable: "PurchaseRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "Description", "Name", "Slug", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("a7b8c9d0-e1f2-3456-0123-567890123456"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Kids shoes and footwear", "Shoes", "shoes", null },
                    { new Guid("b8c9d0e1-f2a3-4567-1234-678901234567"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bottles, breast pumps, and feeding accessories", "Feeding", "feeding", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserRatings_PurchaseRequestId",
                table: "UserRatings",
                column: "PurchaseRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRatings_RatedUserId",
                table: "UserRatings",
                column: "RatedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRatings_RaterId",
                table: "UserRatings",
                column: "RaterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserRatings");

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("a7b8c9d0-e1f2-3456-0123-567890123456"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("b8c9d0e1-f2a3-4567-1234-678901234567"));
        }
    }
}
