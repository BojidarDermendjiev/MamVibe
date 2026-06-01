using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MomVibe.Infrastructure.Migrations
{
    /// <inheritdoc />
    /// <summary>
    /// Adds the <c>OutboxMessages</c> table backing the transactional outbox. Each row holds a
    /// serialized payload destined for an external system (n8n, push notifications, third-party
    /// email) so delivery survives process crashes between business state commit and external
    /// call success.
    ///
    /// The composite index on (ProcessedAt, NextAttemptAt) supports the queue query
    /// <c>WHERE ProcessedAt IS NULL AND NextAttemptAt &lt;= now</c> efficiently.
    /// </summary>
    public partial class AddOutboxMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                columns: table => new
                {
                    Id            = table.Column<Guid>(type: "uuid", nullable: false),
                    MessageType   = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Payload       = table.Column<string>(type: "text", nullable: false),
                    ProcessedAt   = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AttemptCount  = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    NextAttemptAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastError     = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt     = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt     = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedAt_NextAttemptAt",
                table: "OutboxMessages",
                columns: ["ProcessedAt", "NextAttemptAt"]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "OutboxMessages");
        }
    }
}
