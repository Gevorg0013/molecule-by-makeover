using MoleculeByMakeover.Domain.Common;

namespace MoleculeByMakeover.Domain.Entities;

public class Role : ReferenceEntity
{
    public string Name { get; set; } = default!;

    public ICollection<UserRole> UserRoles { get; set; } = [];
}
