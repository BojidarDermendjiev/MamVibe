using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MomVibe.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEBillNumberToPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "Wallets",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                comment: "ISO 4217 currency code (e.g. EUR, BGN).",
                oldClrType: typeof(string),
                oldType: "character varying(3)",
                oldMaxLength: 3,
                oldComment: "ISO 4217 currency code (e.g. BGN, EUR).");

            migrationBuilder.AddColumn<string>(
                name: "EBillNumber",
                table: "Payments",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true,
                comment: "Human-readable e-bill number assigned when a purchase is completed (e.g. MV-2026-A1B2C3D4).");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EBillNumber",
                table: "Payments");

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "Wallets",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                comment: "ISO 4217 currency code (e.g. BGN, EUR).",
                oldClrType: typeof(string),
                oldType: "character varying(3)",
                oldMaxLength: 3,
                oldComment: "ISO 4217 currency code (e.g. EUR, BGN).");
        }
    }
}
