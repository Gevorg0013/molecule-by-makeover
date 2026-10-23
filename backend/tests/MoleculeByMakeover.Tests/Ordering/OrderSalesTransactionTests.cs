using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MoleculeByMakeover.Application.Common.Interfaces;
using MoleculeByMakeover.Application.Features.Ordering;
using MoleculeByMakeover.Domain.Entities;
using MoleculeByMakeover.Domain.Enums;
using MoleculeByMakeover.Domain.ValueObjects;
using MoleculeByMakeover.Infrastructure.Persistence;
using MoleculeByMakeover.Infrastructure.Persistence.Repositories;
using NSubstitute;

namespace MoleculeByMakeover.Tests.Ordering;

// Runs OrderService against the real UnitOfWork on an in-memory store, so the tests also prove
// that events appended to an already-saved sale are actually inserted.
public class OrderSalesTransactionTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();
    private readonly Guid _adminId = Guid.NewGuid();

    [Fact]
    public async Task Payment_confirmation_marks_the_sale_paid()
    {
        var (order, _, _) = await SeedAsync();

        await using (var db = NewContext())
            await NewService(db).MarkPaidAsync("pi_test_123", "evt_1");

        await using var check = NewContext();
        var sale = await check.SalesTransactions.Include(s => s.Events).SingleAsync();
        Assert.Equal(SalesTransactionStatus.Paid, sale.Status);
        Assert.NotNull(sale.CompletedAt);
        var paid = Assert.Single(sale.Events, e => e.EventType == SalesTransactionEventType.PaymentSucceeded);
        Assert.Equal(SalesEventSource.PaymentProvider, paid.Source);
        Assert.Contains("evt_1", paid.Data);
        Assert.Equal(PaymentStatus.Paid, (await check.Orders.SingleAsync(o => o.Id == order.Id)).PaymentStatus);
    }

    [Fact]
    public async Task Expired_reservation_cancels_the_sale_and_records_released_stock()
    {
        var (_, product, _) = await SeedAsync(reservationExpired: true);

        await using (var db = NewContext())
            Assert.Equal(1, await NewService(db).ReleaseExpiredReservationsAsync());

        await using var check = NewContext();
        var sale = await check.SalesTransactions.Include(s => s.Events).SingleAsync();
        Assert.Equal(SalesTransactionStatus.Cancelled, sale.Status);
        Assert.Contains(sale.Events, e => e.EventType == SalesTransactionEventType.ReservationExpired);
        var released = Assert.Single(sale.Events, e => e.EventType == SalesTransactionEventType.StockReleased);
        Assert.Contains("\"stockAfter\":5", released.Data);
        Assert.Equal(5, (await check.Products.SingleAsync(p => p.Id == product.Id)).Stock);
    }

    [Fact]
    public async Task Admin_status_change_is_attributed_to_the_admin()
    {
        var (order, _, _) = await SeedAsync();

        await using (var db = NewContext())
            await NewService(db).UpdateStatusAsync(order.Id, new UpdateOrderStatusRequest(OrderStatus.Cancelled));

        await using var check = NewContext();
        var sale = await check.SalesTransactions.Include(s => s.Events).SingleAsync();
        var changed = Assert.Single(sale.Events, e => e.EventType == SalesTransactionEventType.StatusChanged);
        Assert.Equal(_adminId, changed.ActorUserId);
        Assert.Equal(SalesEventSource.Admin, changed.Source);
        Assert.Equal(SalesTransactionStatus.Cancelled, sale.Status);
    }

    [Fact]
    public async Task Refund_webhook_is_recorded_without_changing_the_order()
    {
        var (order, _, _) = await SeedAsync();

        await using (var db = NewContext())
            await NewService(db).RecordPaymentEventAsync("pi_test_123", PaymentEventType.Refunded, "evt_9");

        await using var check = NewContext();
        var sale = await check.SalesTransactions.Include(s => s.Events).SingleAsync();
        Assert.Equal(SalesTransactionStatus.Refunded, sale.Status);
        Assert.Single(sale.Events, e => e.EventType == SalesTransactionEventType.Refunded);
        Assert.Equal(OrderStatus.Pending, (await check.Orders.SingleAsync(o => o.Id == order.Id)).Status);
    }

    private AppDbContext NewContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(_databaseName).Options);

    private OrderService NewService(AppDbContext db)
    {
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.UserId.Returns(_adminId);
        return new OrderService(new UnitOfWork(db), currentUser, NullLogger<OrderService>.Instance);
    }

    private async Task<(Order Order, Product Product, SalesTransaction Sale)> SeedAsync(bool reservationExpired = false)
    {
        await using var db = NewContext();
        var product = new Product { Sku = "SER-01", Stock = 3, Price = 10_000m };
        var order = new Order
        {
            OrderNumber = "MBM-TEST-1",
            UserId = Guid.NewGuid(),
            GrandTotal = 20_000m,
            SubTotal = 20_000m,
            Currency = "AMD",
            PaymentProvider = "stripe",
            PaymentReference = "pi_test_123",
            ShippingAddress = new Address { FullName = "A", Phone = "1", Country = "AM", City = "Yerevan", AddressLine1 = "1 Main St" },
            StockReservationExpiresAt = DateTimeOffset.UtcNow.AddMinutes(reservationExpired ? -5 : 30)
        };
        order.Items.Add(new OrderItem
        {
            OrderId = order.Id, ProductId = product.Id, ProductNameSnapshot = "Serum", SkuSnapshot = "SER-01",
            UnitPriceSnapshot = 10_000m, Quantity = 2, LineTotal = 20_000m
        });
        var sale = new SalesTransaction
        {
            UserId = order.UserId, OrderId = order.Id, OrderNumber = order.OrderNumber, Status = SalesTransactionStatus.AwaitingPayment,
            GrandTotal = 20_000m, Currency = "AMD", PaymentProvider = "stripe", PaymentReference = "pi_test_123", StartedAt = DateTimeOffset.UtcNow
        };
        sale.Record(SalesTransactionEventType.OrderCreated, SalesEventSource.Customer, "Order created");

        db.AddRange(product, order, sale);
        await db.SaveChangesAsync();
        return (order, product, sale);
    }
}
