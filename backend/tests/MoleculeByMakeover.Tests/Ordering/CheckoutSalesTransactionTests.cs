using Microsoft.Extensions.Logging.Abstractions;
using MoleculeByMakeover.Application.Common.Interfaces;
using MoleculeByMakeover.Application.Features.Ordering;
using MoleculeByMakeover.Domain.Entities;
using MoleculeByMakeover.Domain.Enums;
using MoleculeByMakeover.Domain.Interfaces;
using MoleculeByMakeover.Domain.ValueObjects;
using MoleculeByMakeover.Shared.Exceptions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace MoleculeByMakeover.Tests.Ordering;

public class CheckoutSalesTransactionTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IPaymentProvider _provider = Substitute.For<IPaymentProvider>();
    private readonly ISalesTransactionWriter _writer = Substitute.For<ISalesTransactionWriter>();
    private readonly List<object> _tracked = [];
    private readonly CheckoutService _sut;
    private readonly Product _product = new() { Sku = "SER-01", Stock = 5, Price = 10_000m };
    private readonly Cart _cart = new() { UserId = UserId };

    public CheckoutSalesTransactionTests()
    {
        _cart.Items.Add(new CartItem { Product = _product, ProductId = _product.Id, Quantity = 2, UnitPriceSnapshot = 10_000m });
        _unitOfWork.Carts.GetActiveByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(_cart);
        _unitOfWork.Orders.Returns(Substitute.For<IOrderRepository>());
        _unitOfWork.When(u => u.TrackNew(Arg.Any<SalesTransaction>())).Do(c => _tracked.Add(c.Arg<SalesTransaction>()));

        _provider.CreateSessionAsync(Arg.Any<PaymentSessionRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PaymentSessionResult("pi_test_123", "secret_value", null));
        var resolver = Substitute.For<IPaymentProviderResolver>();
        resolver.Resolve("stripe").Returns(_provider);

        _sut = new CheckoutService(_unitOfWork, resolver, new CheckoutRequestValidator(), _writer, NullLogger<CheckoutService>.Instance);
    }

    [Fact]
    public async Task Successful_checkout_saves_the_sale_with_the_order()
    {
        var result = await _sut.PlaceOrderAsync(UserId, null, Request(), "https://r", "https://c");

        var sale = Assert.IsType<SalesTransaction>(Assert.Single(_tracked));
        Assert.Equal(SalesTransactionStatus.AwaitingPayment, sale.Status);
        Assert.Equal(result.OrderNumber, sale.OrderNumber);
        Assert.NotNull(sale.OrderId);
        Assert.Equal(20_000m, sale.GrandTotal);
        Assert.Equal("pi_test_123", sale.PaymentReference);
        Assert.Equal(1, sale.ItemCount);
        Assert.Equal(
            [SalesTransactionEventType.CheckoutStarted, SalesTransactionEventType.PaymentSessionCreated,
             SalesTransactionEventType.OrderCreated, SalesTransactionEventType.StockReserved],
            sale.Events.Select(e => e.EventType));
        Assert.Contains("\"stockBefore\":5", sale.Events.Last().Data);
        Assert.Contains("\"stockAfter\":3", sale.Events.Last().Data);
        Assert.DoesNotContain("secret_value", string.Join(' ', sale.Events.Select(e => e.Data)));
        await _writer.DidNotReceiveWithAnyArgs().WriteFailedAttemptAsync(default!);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Out_of_stock_attempt_is_recorded_as_rejected_without_saving_the_order()
    {
        _product.Stock = 1;

        var ex = await Assert.ThrowsAsync<BadRequestException>(() => _sut.PlaceOrderAsync(UserId, null, Request(), "https://r", "https://c"));

        var sale = await SingleFailedAttemptAsync();
        Assert.Equal(SalesTransactionStatus.Rejected, sale.Status);
        Assert.Equal(ex.Message, sale.FailureReason);
        Assert.Null(sale.OrderId);
        Assert.NotNull(sale.CompletedAt);
        Assert.Equal(SalesTransactionEventType.CheckoutRejected, sale.Events.Last().EventType);
        Assert.Empty(_tracked);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _provider.DidNotReceiveWithAnyArgs().CreateSessionAsync(default!);
    }

    [Fact]
    public async Task Payment_session_failure_is_recorded_on_the_rejected_sale()
    {
        _provider.CreateSessionAsync(Arg.Any<PaymentSessionRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("provider unreachable"));

        await Assert.ThrowsAsync<BadRequestException>(() => _sut.PlaceOrderAsync(UserId, null, Request(), "https://r", "https://c"));

        var sale = await SingleFailedAttemptAsync();
        Assert.Equal(SalesTransactionStatus.Rejected, sale.Status);
        Assert.Equal(20_000m, sale.GrandTotal);
        var failed = Assert.Single(sale.Events, e => e.EventType == SalesTransactionEventType.PaymentSessionFailed);
        Assert.False(failed.Succeeded);
        Assert.Equal(5, _product.Stock); // nothing reserved
    }

    [Fact]
    public async Task Invalid_request_is_recorded_without_echoing_customer_input()
    {
        var request = Request() with { ShippingAddress = new Address { FullName = "" } };

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
            _sut.PlaceOrderAsync(UserId, null, request, "https://r", "https://c"));

        var sale = await SingleFailedAttemptAsync();
        Assert.Equal("The checkout request was invalid.", sale.FailureReason);
    }

    private async Task<SalesTransaction> SingleFailedAttemptAsync()
    {
        var call = Assert.Single(_writer.ReceivedCalls());
        await Task.CompletedTask;
        return Assert.IsType<SalesTransaction>(call.GetArguments()[0]);
    }

    private static CheckoutRequest Request() => new(
        new Address
        {
            FullName = "Ani Petrosyan", Phone = "+37491000000", Country = "Armenia", City = "Yerevan", AddressLine1 = "1 Main St"
        },
        "stripe");
}
