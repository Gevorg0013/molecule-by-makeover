using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MoleculeByMakeover.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDataChangeAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataChangeLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    TableName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Operation = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    RowId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    OldData = table.Column<string>(type: "jsonb", nullable: true),
                    NewData = table.Column<string>(type: "jsonb", nullable: true),
                    ChangedColumns = table.Column<string>(type: "text", nullable: true),
                    AppUserId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    DbUser = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TransactionId = table.Column<long>(type: "bigint", nullable: false),
                    ChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "clock_timestamp()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataChangeLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DataChangeLogs_AppUserId_ChangedAt",
                table: "DataChangeLogs",
                columns: new[] { "AppUserId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_DataChangeLogs_ChangedAt",
                table: "DataChangeLogs",
                column: "ChangedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DataChangeLogs_TableName_RowId_ChangedAt",
                table: "DataChangeLogs",
                columns: new[] { "TableName", "RowId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_DataChangeLogs_TransactionId",
                table: "DataChangeLogs",
                column: "TransactionId");

            migrationBuilder.Sql(LogDataChangeFunction);
            foreach (var (table, excludedColumns) in AuditedTables)
            {
                var args = string.Join(", ", excludedColumns.Select(c => $"'{c}'"));
                migrationBuilder.Sql($"""
                    CREATE TRIGGER audit_data_change
                    AFTER INSERT OR UPDATE OR DELETE ON "{table}"
                    FOR EACH ROW EXECUTE FUNCTION log_data_change({args});
                    """);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var (table, _) in AuditedTables)
                migrationBuilder.Sql($"""DROP TRIGGER IF EXISTS audit_data_change ON "{table}";""");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS log_data_change();");

            migrationBuilder.DropTable(
                name: "DataChangeLogs");
        }

        // Tables whose changes are audited, with the columns whose VALUES must never be copied into
        // the log (a change to them is still recorded, by column name only). Left out on purpose:
        // token tables (secrets), carts/wishlists (high-volume, not business records), translation
        // tables (rewritten on every save), newsletter subscribers, and the log tables themselves.
        private static readonly (string Table, string[] ExcludedColumns)[] AuditedTables =
        [
            ("Orders", []),
            ("OrderItems", []),
            ("Products", []),
            ("ProductImages", []),
            ("Categories", []),
            ("Coupons", []),
            ("Users", ["PasswordHash"]),
            ("UserRoles", []),
            ("Roles", []),
            ("Reviews", []),
            ("Banners", []),
            ("Pages", []),
            ("BlogPosts", []),
            ("GalleryImages", []),
            ("Languages", []),
            ("Settings", ["Value"]),
            ("ProcessedPaymentEvents", []),
        ];

        // Generic AFTER ROW trigger. Arguments = column names to redact. UPDATEs that change nothing
        // but UpdatedAt are skipped. app.user_id is set per save by AuditUserInterceptor; it is
        // empty (stored as NULL) for anonymous requests, background jobs and changes made outside the app.
        private const string LogDataChangeFunction = """
            CREATE OR REPLACE FUNCTION log_data_change() RETURNS trigger
            LANGUAGE plpgsql AS $$
            DECLARE
                excluded text[] := COALESCE(TG_ARGV::text[], '{}');
                full_old jsonb;
                full_new jsonb;
                changed text[];
                row_key jsonb;
            BEGIN
                IF TG_OP <> 'INSERT' THEN full_old := to_jsonb(OLD); END IF;
                IF TG_OP <> 'DELETE' THEN full_new := to_jsonb(NEW); END IF;

                IF TG_OP = 'UPDATE' THEN
                    SELECT array_agg(n.key ORDER BY n.key) INTO changed
                    FROM jsonb_each(full_new) n
                    WHERE n.key <> 'UpdatedAt' AND (full_old -> n.key) IS DISTINCT FROM n.value;
                    IF changed IS NULL THEN RETURN NULL; END IF;
                END IF;

                row_key := COALESCE(full_new, full_old);
                INSERT INTO "DataChangeLogs"
                    ("TableName", "Operation", "RowId", "OldData", "NewData", "ChangedColumns",
                     "AppUserId", "DbUser", "TransactionId", "ChangedAt")
                VALUES (
                    TG_TABLE_NAME,
                    TG_OP,
                    COALESCE(row_key ->> 'Id', row_key ->> 'Key', (row_key ->> 'UserId') || '/' || (row_key ->> 'RoleId')),
                    full_old - excluded,
                    full_new - excluded,
                    array_to_string(changed, ','),
                    NULLIF(current_setting('app.user_id', true), ''),
                    current_user,
                    txid_current(),
                    clock_timestamp());
                RETURN NULL;
            END
            $$;
            """;
    }
}
