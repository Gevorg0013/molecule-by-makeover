using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoleculeByMakeover.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalesTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrderNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ItemCount = table.Column<int>(type: "integer", nullable: false),
                    SubTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    DiscountTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    ShippingTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    GrandTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    CouponCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PaymentProvider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PaymentReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesTransactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesTransactionEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SalesTransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    Succeeded = table.Column<bool>(type: "boolean", nullable: false),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Data = table.Column<string>(type: "jsonb", nullable: true),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesTransactionEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesTransactionEvents_SalesTransactions_SalesTransactionId",
                        column: x => x.SalesTransactionId,
                        principalTable: "SalesTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesTransactionEvents_EventType_OccurredAt",
                table: "SalesTransactionEvents",
                columns: new[] { "EventType", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesTransactionEvents_SalesTransactionId_OccurredAt",
                table: "SalesTransactionEvents",
                columns: new[] { "SalesTransactionId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesTransactions_OrderId",
                table: "SalesTransactions",
                column: "OrderId",
                unique: true,
                filter: "\"OrderId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SalesTransactions_OrderNumber",
                table: "SalesTransactions",
                column: "OrderNumber");

            migrationBuilder.CreateIndex(
                name: "IX_SalesTransactions_PaymentReference",
                table: "SalesTransactions",
                column: "PaymentReference");

            migrationBuilder.CreateIndex(
                name: "IX_SalesTransactions_Status_StartedAt",
                table: "SalesTransactions",
                columns: new[] { "Status", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesTransactions_UserId_StartedAt",
                table: "SalesTransactions",
                columns: new[] { "UserId", "StartedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesTransactionEvents");

            migrationBuilder.DropTable(
                name: "SalesTransactions");
        }
    }
}
