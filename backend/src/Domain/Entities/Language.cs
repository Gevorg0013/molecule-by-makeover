using MoleculeByMakeover.Domain.Common;

namespace MoleculeByMakeover.Domain.Entities;

public class Language : ReferenceEntity
{
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
}
