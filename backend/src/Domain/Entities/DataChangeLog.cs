namespace MoleculeByMakeover.Domain.Entities;

// Database-level audit trail. Rows are written by the PostgreSQL trigger function log_data_change()
// (see the AddDataChangeAudit migration), not by application code, so every INSERT / UPDATE / DELETE
// on an audited table is captured - including changes made outside the app (psql, pgAdmin, scripts).
public class DataChangeLog
{
    public long Id { get; set; }

    public string TableName { get; set; } = default!;

    /// <summary>INSERT, UPDATE or DELETE.</summary>
    public string Operation { get; set; } = default!;

    /// <summary>Primary key of the changed row (Id, Key, or UserId/RoleId for UserRoles).</summary>
    public string? RowId { get; set; }

    /// <summary>Row before the change (null for INSERT), as JSON. Sensitive columns are left out.</summary>
    public string? OldData { get; set; }

    /// <summary>Row after the change (null for DELETE), as JSON. Sensitive columns are left out.</summary>
    public string? NewData { get; set; }

    /// <summary>For UPDATE: comma-separated names of the columns that changed (sensitive ones by name only).</summary>
    public string? ChangedColumns { get; set; }

    /// <summary>The app user who made the change; null for anonymous, background-job or out-of-app changes.</summary>
    public string? AppUserId { get; set; }

    /// <summary>The PostgreSQL role that ran the statement.</summary>
    public string DbUser { get; set; } = default!;

    /// <summary>PostgreSQL transaction id: rows sharing it were changed together.</summary>
    public long TransactionId { get; set; }

    /// <summary>UTC.</summary>
    public DateTimeOffset ChangedAt { get; set; }
}
