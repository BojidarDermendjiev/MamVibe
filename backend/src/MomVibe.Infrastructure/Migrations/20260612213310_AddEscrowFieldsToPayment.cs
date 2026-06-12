using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MomVibe.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEscrowFieldsToPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "HeldUntil",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PlatformFeeAmount",
                table: "Payments",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReleaseScheduledAt",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SellerNetAmount",
                table: "Payments",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "StripePaymentIntentId",
                table: "Payments",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripeTransferId",
                table: "Payments",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Status_HeldUntil",
                table: "Payments",
                columns: new[] { "Status", "HeldUntil" });

            migrationBuilder.CreateIndex(
                name: "UX_Payments_StripePaymentIntentId",
                table: "Payments",
                column: "StripePaymentIntentId",
                unique: true,
                filter: "\"StripePaymentIntentId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_Status_HeldUntil",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "UX_Payments_StripePaymentIntentId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "HeldUntil",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PlatformFeeAmount",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ReleaseScheduledAt",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "SellerNetAmount",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "StripePaymentIntentId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "StripeTransferId",
                table: "Payments");
        }
    }
}
