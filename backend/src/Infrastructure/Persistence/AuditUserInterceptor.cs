using System.Data.Common;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

namespace MoleculeByMakeover.Infrastructure.Persistence;

// Tells the database who is saving: before each SaveChanges it sets the session variable
// app.user_id, which the log_data_change() trigger copies into DataChangeLogs.AppUserId.
// The connection is held open across the save so the setting applies to the save's statements,
// then released; Npgsql resets session state when the connection goes back to the pool, so the
// value can't leak to another request. Always written (empty when there is no user) for the same reason.
public class AuditUserInterceptor(IHttpContextAccessor httpContextAccessor) : SaveChangesInterceptor
{
    private const string SetUserSql = "SELECT set_config('app.user_id', @userId, false)";

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is { } context && context.Database.IsRelational())
        {
            context.Database.OpenConnection();
            using var command = CreateCommand(context);
            command.ExecuteNonQuery();
        }
        return result;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is { } context && context.Database.IsRelational())
        {
            await context.Database.OpenConnectionAsync(cancellationToken);
            await using var command = CreateCommand(context);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        return result;
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        Release(eventData.Context);
        return result;
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        await ReleaseAsync(eventData.Context);
        return result;
    }

    public override void SaveChangesFailed(DbContextErrorEventData eventData) => Release(eventData.Context);

    public override Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default) =>
        ReleaseAsync(eventData.Context);

    public override void SaveChangesCanceled(DbContextEventData eventData) => Release(eventData.Context);

    public override Task SaveChangesCanceledAsync(DbContextEventData eventData, CancellationToken cancellationToken = default) =>
        ReleaseAsync(eventData.Context);

    private DbCommand CreateCommand(DbContext context)
    {
        var command = context.Database.GetDbConnection().CreateCommand();
        command.Transaction = context.Database.CurrentTransaction?.GetDbTransaction();
        command.CommandText = SetUserSql;

        var parameter = command.CreateParameter();
        parameter.ParameterName = "userId";
        parameter.Value = CurrentUserId() ?? string.Empty;
        command.Parameters.Add(parameter);
        return command;
    }

    private string? CurrentUserId()
    {
        var user = httpContextAccessor.HttpContext?.User;
        return user?.FindFirstValue(ClaimTypes.NameIdentifier) ?? user?.FindFirstValue("sub");
    }

    // Balances the OpenConnection above (EF reference-counts opens, so an outer open stays open).
    private static void Release(DbContext? context)
    {
        if (context is not null && context.Database.IsRelational()) context.Database.CloseConnection();
    }

    private static async Task ReleaseAsync(DbContext? context)
    {
        if (context is not null && context.Database.IsRelational()) await context.Database.CloseConnectionAsync();
    }
}
