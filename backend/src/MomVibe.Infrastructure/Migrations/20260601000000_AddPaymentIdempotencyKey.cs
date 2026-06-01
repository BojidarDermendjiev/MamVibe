using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MomVibe.Infrastructure.Migrations
{
    /// <inheritdoc />
    /// <summary>
    /// Adds <c>Payment.IdempotencyKey</c> (nullable string, max 255) with a unique index.
    /// The column captures the client-supplied <c>Idempotency-Key</c> request header so that
    /// duplicate payment-creation requests caused by double-taps or retries are deduped both
    /// at the application layer and (as a safety net for races) at the database layer.
    /// </summary>
    public partial class AddPaymentIdempotencyKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdempotencyKey",
                table: "Payments",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                comment: "Client-supplied idempotency key used to dedupe duplicate payment-creation requests.");

            // Partial unique index — only enforces uniqueness for rows where IdempotencyKey IS NOT NULL.
            // Existing rows have NULL keys and remain unaffected.
            migrationBuilder.Sql(@"CREATE UNIQUE INDEX ""IX_Payments_IdempotencyKey"" ON ""Payments"" (""IdempotencyKey"") WHERE ""IdempotencyKey"" IS NOT NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_Payments_IdempotencyKey"";");

            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                table: "Payments");
        }
    }
}
