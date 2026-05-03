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
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "Description", "Name", "Slug", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("a7b8c9d0-e1f2-3456-0123-567890123456"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Kids shoes and footwear", "Shoes", "shoes", null },
                    { new Guid("b8c9d0e1-f2a3-4567-1234-678901234567"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bottles, breast pumps, and feeding accessories", "Feeding", "feeding", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
