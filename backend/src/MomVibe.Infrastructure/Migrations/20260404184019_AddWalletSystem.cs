using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MomVibe.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWalletSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Wallets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false, comment: "Foreign key referencing the wallet owner (FK to AspNetUsers.Id)."),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, comment: "ISO 4217 currency code (e.g. BGN, EUR)."),
                    Status = table.Column<int>(type: "integer", nullable: false, comment: "Operational state of the wallet (Active, Frozen, Suspended, Closed)."),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wallets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Wallets_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WalletTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WalletId = table.Column<Guid>(type: "uuid", nullable: false, comment: "Foreign key referencing the wallet this transaction belongs to."),
                    Type = table.Column<int>(type: "integer", nullable: false, comment: "Direction of money movement: Credit (money in) or Debit (money out)."),
                    Kind = table.Column<int>(type: "integer", nullable: false, comment: "Business reason for the transaction (TopUp, Transfer, ItemPayment, Withdrawal, Refund, Fee)."),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, comment: "Absolute monetary amount of this transaction in the wallet currency."),
                    BalanceAfter = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, comment: "Wallet balance snapshot immediately after this transaction was applied."),
                    Status = table.Column<int>(type: "integer", nullable: false, comment: "Settlement state of this transaction."),
                    Reference = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "External reference identifier (e.g. Stripe PaymentIntent ID, transfer ID)."),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "Human-readable description shown in transaction history."),
                    RelatedTransactionId = table.Column<Guid>(type: "uuid", nullable: true, comment: "ID of the counterpart transaction in a double-entry transfer (the other leg)."),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: true, comment: "FK to the marketplace Payment record when kind is ItemPayment."),
                    ReceiptUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true, comment: "URL to the TakeANap fiscal receipt generated for this transaction."),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WalletTransactions_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WalletTransactions_WalletTransactions_RelatedTransactionId",
                        column: x => x.RelatedTransactionId,
                        principalTable: "WalletTransactions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WalletTransactions_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "Wallets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WalletTransfers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderWalletId = table.Column<Guid>(type: "uuid", nullable: false, comment: "FK to the wallet that initiates the transfer (debit side)."),
                    ReceiverWalletId = table.Column<Guid>(type: "uuid", nullable: false, comment: "FK to the wallet that receives the funds (credit side)."),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, comment: "Amount transferred between wallets."),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, comment: "ISO 4217 currency code of the transferred amount."),
                    Status = table.Column<int>(type: "integer", nullable: false, comment: "Overall status of the transfer operation."),
                    Note = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true, comment: "Optional message from the sender shown in the receiver's transaction history."),
                    InitiatedByIp = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true, comment: "IP address of the client that initiated the transfer, stored for fraud monitoring."),
                    SenderTransactionId = table.Column<Guid>(type: "uuid", nullable: true, comment: "FK to the debit WalletTransaction created for the sender."),
                    ReceiverTransactionId = table.Column<Guid>(type: "uuid", nullable: true, comment: "FK to the credit WalletTransaction created for the receiver."),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletTransfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WalletTransfers_WalletTransactions_ReceiverTransactionId",
                        column: x => x.ReceiverTransactionId,
                        principalTable: "WalletTransactions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WalletTransfers_WalletTransactions_SenderTransactionId",
                        column: x => x.SenderTransactionId,
                        principalTable: "WalletTransactions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WalletTransfers_Wallets_ReceiverWalletId",
                        column: x => x.ReceiverWalletId,
                        principalTable: "Wallets",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WalletTransfers_Wallets_SenderWalletId",
                        column: x => x.SenderWalletId,
                        principalTable: "Wallets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_Status",
                table: "Wallets",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_UserId",
                table: "Wallets",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_CreatedAt",
                table: "WalletTransactions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_Kind",
                table: "WalletTransactions",
                column: "Kind");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_PaymentId",
                table: "WalletTransactions",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_RelatedTransactionId",
                table: "WalletTransactions",
                column: "RelatedTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_Status",
                table: "WalletTransactions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_WalletId",
                table: "WalletTransactions",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_WalletId_CreatedAt",
                table: "WalletTransactions",
                columns: new[] { "WalletId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransfers_CreatedAt",
                table: "WalletTransfers",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransfers_ReceiverTransactionId",
                table: "WalletTransfers",
                column: "ReceiverTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransfers_ReceiverWalletId",
                table: "WalletTransfers",
                column: "ReceiverWalletId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransfers_SenderTransactionId",
                table: "WalletTransfers",
                column: "SenderTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransfers_SenderWalletId",
                table: "WalletTransfers",
                column: "SenderWalletId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransfers_Status",
                table: "WalletTransfers",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WalletTransfers");

            migrationBuilder.DropTable(
                name: "WalletTransactions");

            migrationBuilder.DropTable(
                name: "Wallets");
        }
    }
}
