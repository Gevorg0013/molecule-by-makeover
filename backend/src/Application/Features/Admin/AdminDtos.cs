namespace MoleculeByMakeover.Application.Features.Admin;

public record SettingDto(string Key, string? Value, string Group);
public record UpdateSettingRequest(string? Value);

public record LanguageDto(int Id, string Code, string Name, bool IsDefault, bool IsActive);
public record LanguageUpsertRequest(string Code, string Name, bool IsDefault, bool IsActive);

public record DashboardStatsDto(
    decimal TotalRevenue,
    int TotalOrders,
    int PendingOrders,
    int TotalCustomers,
    int TotalProducts,
    int LowStockProducts,
    List<TopProductDto> TopProducts);

public record TopProductDto(Guid ProductId, string Name, int UnitsSold, decimal Revenue);
