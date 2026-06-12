using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MomVibe.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStripeConnectToApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "BusinessProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "StripeConnectAccountId",
                table: "AspNetUsers",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StripeConnectStatus",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StripeConnectStatusUpdatedAt",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessProfiles_Category",
                table: "BusinessProfiles",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "UX_AspNetUsers_StripeConnectAccountId",
                table: "AspNetUsers",
                column: "StripeConnectAccountId",
                unique: true,
                filter: "\"StripeConnectAccountId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BusinessProfiles_Category",
                table: "BusinessProfiles");

            migrationBuilder.DropIndex(
                name: "UX_AspNetUsers_StripeConnectAccountId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "BusinessProfiles");

            migrationBuilder.DropColumn(
                name: "StripeConnectAccountId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "StripeConnectStatus",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "StripeConnectStatusUpdatedAt",
                table: "AspNetUsers");
        }
    }
}
