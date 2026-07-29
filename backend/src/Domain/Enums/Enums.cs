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
