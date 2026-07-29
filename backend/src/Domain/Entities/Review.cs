using MoleculeByMakeover.Domain.Common;

namespace MoleculeByMakeover.Domain.Entities;

public class Review : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;

    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public int Rating { get; set; }
    public string? Comment { get; set; }
    public bool IsApproved { get; set; }
}
