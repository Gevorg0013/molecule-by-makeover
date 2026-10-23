using Npgsql;
using NpgsqlTypes;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace MoleculeByMakeover.API.Logging;

// Serilog batched sink that stores log events in the "AppLogs" table (created by the EF migration).
// Uses raw Npgsql instead of EF so that persisting logs never produces more logs, and so a log
// write can't interfere with a request's DbContext. Batching, retry and back-off are handled by
// Serilog; failures are reported to SelfLog, never thrown into the app.
public sealed class DatabaseLogSink(string connectionString) : IBatchedLogEventSink, IDisposable
{
    private const string InsertSql = """
        INSERT INTO "AppLogs" ("Timestamp", "Level", "Message", "MessageTemplate", "Exception", "SourceContext", "RequestPath", "UserId", "Properties")
        VALUES (@timestamp, @level, @message, @template, @exception, @source, @path, @user, @properties)
        """;

    private static readonly JsonValueFormatter ValueFormatter = new(typeTagName: null);
    private readonly NpgsqlDataSource _dataSource = NpgsqlDataSource.Create(connectionString);

    public async Task EmitBatchAsync(IReadOnlyCollection<LogEvent> batch)
    {
        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var npgsqlBatch = new NpgsqlBatch(connection);

        foreach (var logEvent in batch)
        {
            var command = new NpgsqlBatchCommand(InsertSql);
            command.Parameters.AddWithValue("timestamp", logEvent.Timestamp.ToUniversalTime());
            command.Parameters.AddWithValue("level", logEvent.Level.ToString());
            command.Parameters.AddWithValue("message", logEvent.RenderMessage());
            command.Parameters.AddWithValue("template", logEvent.MessageTemplate.Text);
            command.Parameters.AddWithValue("exception", (object?)logEvent.Exception?.ToString() ?? DBNull.Value);
            command.Parameters.AddWithValue("source", Truncate(Scalar(logEvent, "SourceContext"), 512));
            command.Parameters.AddWithValue("path", Truncate(Scalar(logEvent, "RequestPath"), 2048));
            command.Parameters.AddWithValue("user", Truncate(Scalar(logEvent, "UserId"), 64));
            command.Parameters.Add(new NpgsqlParameter("properties", NpgsqlDbType.Jsonb) { Value = PropertiesJson(logEvent) });
            npgsqlBatch.BatchCommands.Add(command);
        }

        try
        {
            await npgsqlBatch.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            SelfLog.WriteLine("DatabaseLogSink failed to write {0} event(s): {1}", batch.Count, ex.Message);
            throw; // let Serilog's batching retry with back-off
        }
    }

    public Task OnEmptyBatchAsync() => Task.CompletedTask;

    public void Dispose() => _dataSource.Dispose();

    private static string? Scalar(LogEvent logEvent, string name) =>
        logEvent.Properties.TryGetValue(name, out var value) && value is ScalarValue { Value: not null } scalar
            ? scalar.Value.ToString()
            : null;

    private static object Truncate(string? value, int maxLength) =>
        value is null ? DBNull.Value : value.Length <= maxLength ? value : value[..maxLength];

    private static string PropertiesJson(LogEvent logEvent)
    {
        using var writer = new StringWriter();
        writer.Write('{');
        var first = true;
        foreach (var (name, value) in logEvent.Properties)
        {
            if (!first) writer.Write(',');
            first = false;
            JsonValueFormatter.WriteQuotedJsonString(name, writer);
            writer.Write(':');
            ValueFormatter.Format(value, writer);
        }
        writer.Write('}');
        return writer.ToString();
    }
}
