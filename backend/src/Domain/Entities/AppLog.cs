namespace MoleculeByMakeover.Domain.Entities;

// One application log event (Serilog) persisted to the database. Rows are inserted by the API's
// database log sink with plain SQL - never through EF - so writing a log can't itself log.
public class AppLog
{
    public long Id { get; set; }

    /// <summary>UTC.</summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>Verbose, Debug, Information, Warning, Error or Fatal.</summary>
    public string Level { get; set; } = default!;

    public string Message { get; set; } = default!;
    public string MessageTemplate { get; set; } = default!;
    public string? Exception { get; set; }
    public string? SourceContext { get; set; }
    public string? RequestPath { get; set; }
    public string? UserId { get; set; }

    /// <summary>All structured properties of the event, as JSON.</summary>
    public string? Properties { get; set; }
}
