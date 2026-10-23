using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MoleculeByMakeover.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDatabaseLogging : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Level = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    MessageTemplate = table.Column<string>(type: "text", nullable: false),
                    Exception = table.Column<string>(type: "text", nullable: true),
                    SourceContext = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    RequestPath = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    UserId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Properties = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuthenticationLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Succeeded = table.Column<bool>(type: "boolean", nullable: false),
                    FailureReason = table.Column<int>(type: "integer", nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthenticationLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppLogs_Level_Timestamp",
                table: "AppLogs",
                columns: new[] { "Level", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_AppLogs_Timestamp",
                table: "AppLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AppLogs_UserId_Timestamp",
                table: "AppLogs",
                columns: new[] { "UserId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticationLogs_Email_OccurredAt",
                table: "AuthenticationLogs",
                columns: new[] { "Email", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticationLogs_IpAddress_OccurredAt",
                table: "AuthenticationLogs",
                columns: new[] { "IpAddress", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticationLogs_OccurredAt",
                table: "AuthenticationLogs",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticationLogs_UserId_OccurredAt",
                table: "AuthenticationLogs",
                columns: new[] { "UserId", "OccurredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppLogs");

            migrationBuilder.DropTable(
                name: "AuthenticationLogs");
        }
    }
}
