using System.Text.Json;
using MoleculeByMakeover.Domain.Entities;
using MoleculeByMakeover.Domain.Enums;

namespace MoleculeByMakeover.Application.Features.Ordering;

public static class SalesTransactionRecorder
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Appends a step to the sale's trail. For a transaction loaded from the database, the caller must
    /// also pass the returned event to IUnitOfWork.TrackNew so EF inserts it.
    /// </summary>
    public static SalesTransactionEvent Record(
        this SalesTransaction transaction,
        SalesTransactionEventType eventType,
        SalesEventSource source,
        string message,
        object? data = null,
        bool succeeded = true,
        Guid? actorUserId = null)
    {
        var now = DateTimeOffset.UtcNow;
        var salesEvent = new SalesTransactionEvent
        {
            SalesTransactionId = transaction.Id,
            SalesTransaction = transaction,
            EventType = eventType,
            Source = source,
            Succeeded = succeeded,
            Message = message.Length <= 1000 ? message : message[..1000],
            Data = data is null ? null : JsonSerializer.Serialize(data, JsonOptions),
            ActorUserId = actorUserId,
            OccurredAt = now
        };

        transaction.Events.Add(salesEvent);
        transaction.UpdatedAt = now;
        return salesEvent;
    }

    /// <summary>Moves the sale to a final state.</summary>
    public static void Complete(this SalesTransaction transaction, SalesTransactionStatus status)
    {
        transaction.Status = status;
        transaction.CompletedAt = DateTimeOffset.UtcNow;
        transaction.UpdatedAt = transaction.CompletedAt;
    }
}
