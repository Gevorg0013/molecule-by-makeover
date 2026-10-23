namespace MoleculeByMakeover.Domain.Enums;

public enum OrderStatus
{
    Pending = 0,
    Processing = 1,
    Shipped = 2,
    Delivered = 3,
    Completed = 4,
    Cancelled = 5
}

public enum PaymentStatus
{
    Pending = 0,
    Paid = 1,
    Failed = 2,
    Refunded = 3
}

public enum DiscountType
{
    None = 0,
    Percent = 1,
    Fixed = 2
}

public enum AuthenticationFailureReason
{
    UnknownAccount = 1,
    InvalidPassword = 2,
    AccountInactive = 3
}

public enum SalesTransactionStatus
{
    Started = 0,
    Rejected = 1,
    AwaitingPayment = 2,
    Paid = 3,
    Cancelled = 4,
    Refunded = 5,
    Completed = 6
}

public enum SalesTransactionEventType
{
    CheckoutStarted = 1,
    CheckoutRejected = 2,
    CouponApplied = 3,
    PaymentSessionCreated = 4,
    PaymentSessionFailed = 5,
    OrderCreated = 6,
    StockReserved = 7,
    PaymentSucceeded = 8,
    PaymentFailed = 9,
    Refunded = 10,
    PaymentEventReceived = 11,
    StatusChanged = 12,
    ReservationExpired = 13,
    StockReleased = 14
}

public enum SalesEventSource
{
    Customer = 1,
    Admin = 2,
    PaymentProvider = 3,
    System = 4
}
