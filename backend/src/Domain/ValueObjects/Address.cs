namespace MoleculeByMakeover.Domain.ValueObjects;

public class Address
{
    public string FullName { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string Country { get; set; } = default!;
    public string City { get; set; } = default!;
    public string AddressLine1 { get; set; } = default!;
    public string? AddressLine2 { get; set; }
    public string? PostalCode { get; set; }
}
