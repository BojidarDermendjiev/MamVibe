using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MomVibe.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIbanAndReceiptUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReceiptUrl",
                table: "Payments",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true,
                comment: "URL to the digital receipt from Take a NAP.");

            migrationBuilder.AddColumn<string>(
                name: "Iban",
                table: "AspNetUsers",
                type: "nvarchar(34)",
                maxLength: 34,
                nullable: true,
                comment: "IBAN for receiving card payments.");

            migrationBuilder.CreateTable(
                name: "Shipments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "Foreign key referencing the associated payment."),
                    CourierProvider = table.Column<int>(type: "int", nullable: false, comment: "Courier provider used for this shipment (Econt, Speedy)."),
                    DeliveryType = table.Column<int>(type: "int", nullable: false, comment: "Delivery type (Office, Address, Locker)."),
                    Status = table.Column<int>(type: "int", nullable: false, comment: "Current shipment lifecycle status."),
                    TrackingNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, comment: "Courier tracking number for package lookup."),
                    WaybillId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, comment: "Courier waybill identifier for API operations."),
                    RecipientName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, comment: "Full name of the shipment recipient."),
                    RecipientPhone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, comment: "Phone number of the shipment recipient."),
                    DeliveryAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, comment: "Street address for address-based delivery."),
                    City = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true, comment: "City name for delivery destination."),
                    OfficeId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, comment: "Courier office or locker identifier."),
                    OfficeName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true, comment: "Courier office or locker display name."),
                    ShippingPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, comment: "Shipping price charged for this shipment."),
                    IsCod = table.Column<bool>(type: "bit", nullable: false, comment: "Whether cash on delivery is enabled."),
                    CodAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, comment: "Cash on delivery amount to collect from recipient."),
                    IsInsured = table.Column<bool>(type: "bit", nullable: false, comment: "Whether the shipment has additional insurance."),
                    InsuredAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, comment: "Declared value for shipment insurance."),
                    Weight = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: false, comment: "Package weight in kilograms."),
                    LabelUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true, comment: "URL or path to the generated shipping label PDF."),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shipments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shipments_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_CourierProvider",
                table: "Shipments",
                column: "CourierProvider");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_CreatedAt",
                table: "Shipments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_PaymentId",
                table: "Shipments",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_Status",
                table: "Shipments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_TrackingNumber",
                table: "Shipments",
                column: "TrackingNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Shipments");

            migrationBuilder.DropColumn(
                name: "ReceiptUrl",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Iban",
                table: "AspNetUsers");
        }
    }
}
