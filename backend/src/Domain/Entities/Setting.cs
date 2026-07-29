namespace MoleculeByMakeover.Domain.Entities;

public class Setting
{
    public string Key { get; set; } = default!;
    public string? Value { get; set; }
    public string Group { get; set; } = "general";
}
