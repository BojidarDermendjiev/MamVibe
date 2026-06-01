using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MomVibe.Infrastructure.Migrations
{
    /// <inheritdoc />
    /// <summary>
    /// One-shot data migration to switch the platform from BGN to EUR.
    /// Divides every "live" price column by the fixed BGN/EUR peg (1.95583) and rounds
    /// to 2 decimal places. Historical financial records (Payment.Amount, Shipment.*)
    /// are intentionally left untouched — they record what was actually charged in BGN
    /// at the time and re-pricing them would falsify history.
    ///
    /// Affected columns: Items.Price, Bundles.Price, Offers.OfferedPrice, Offers.CounterPrice,
    /// SavedSearches.MaxPrice.
    ///
    /// IRREVERSIBLE in practice: the Down() multiplies back by the peg, but rounding error
    /// can drift on the second decimal. Treat as forward-only.
    /// </summary>
    public partial class ConvertPricesBgnToEur : Migration
    {
        private const string PegRate = "1.95583";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"UPDATE ""Items"" SET ""Price"" = ROUND(""Price"" / {PegRate}, 2) WHERE ""Price"" IS NOT NULL;");
            migrationBuilder.Sql($@"UPDATE ""Bundles"" SET ""Price"" = ROUND(""Price"" / {PegRate}, 2);");
            migrationBuilder.Sql($@"UPDATE ""Offers"" SET ""OfferedPrice"" = ROUND(""OfferedPrice"" / {PegRate}, 2);");
            migrationBuilder.Sql($@"UPDATE ""Offers"" SET ""CounterPrice"" = ROUND(""CounterPrice"" / {PegRate}, 2) WHERE ""CounterPrice"" IS NOT NULL;");
            migrationBuilder.Sql($@"UPDATE ""SavedSearches"" SET ""MaxPrice"" = ROUND(""MaxPrice"" / {PegRate}, 2) WHERE ""MaxPrice"" IS NOT NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"UPDATE ""Items"" SET ""Price"" = ROUND(""Price"" * {PegRate}, 2) WHERE ""Price"" IS NOT NULL;");
            migrationBuilder.Sql($@"UPDATE ""Bundles"" SET ""Price"" = ROUND(""Price"" * {PegRate}, 2);");
            migrationBuilder.Sql($@"UPDATE ""Offers"" SET ""OfferedPrice"" = ROUND(""OfferedPrice"" * {PegRate}, 2);");
            migrationBuilder.Sql($@"UPDATE ""Offers"" SET ""CounterPrice"" = ROUND(""CounterPrice"" * {PegRate}, 2) WHERE ""CounterPrice"" IS NOT NULL;");
            migrationBuilder.Sql($@"UPDATE ""SavedSearches"" SET ""MaxPrice"" = ROUND(""MaxPrice"" * {PegRate}, 2) WHERE ""MaxPrice"" IS NOT NULL;");
        }
    }
}
